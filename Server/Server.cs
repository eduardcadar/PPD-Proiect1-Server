using Application;
using Domain.Domain;
using Server.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Server
{
    public class Server
    {
        private readonly Service _service;
        private int _millisecondsToVerify;
        private readonly int _numberOfLocations;

        public Server(Service planningsService, int numberOfLocations)
        {
            _numberOfLocations = numberOfLocations;
            _service = planningsService;
        }

        public async Task SetTreatments(List<Treatment> treatments, List<LocationTreatment> locationTreatments)
        {
            await _service.SetTreatments(treatments, locationTreatments);
        }

            public void StartServer(int noThreads, int millisecondsToRun, int millisecondsToVerify)
        {
            _millisecondsToVerify = millisecondsToVerify;
            ThreadPool.SetMaxThreads(noThreads, noThreads);
            
            System.Timers.Timer timer = new()
            {
                Interval = millisecondsToRun
            };
            timer.Elapsed += (sender, e) => {
                timer.Dispose();
                Console.WriteLine("S-a terminat timpul, serverul se inchide...");
                Environment.Exit(0);
            };

            //start verifying thread
            Thread thread = new(new ThreadStart(VerifySistem));
            thread.Start();

            timer.Start();

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 11000;

            TcpListener server = new(ipAddress, port);
            server.Start();

            while (true)
            {
                //start listening
                Console.WriteLine("Waiting for client to connect...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected!");
                ThreadPool.QueueUserWorkItem(async (_) => await SolveClient(client));
            }
        }

        private async Task SolveClient(TcpClient client)
        {
            bool connected = true;
            StreamReader clientIn = new(client.GetStream());
            StreamWriter clientOut = new(client.GetStream())
            {
                AutoFlush = true
            };

            while (connected)
            {
                Console.WriteLine("Receiving req from client...");
                string msgIn = clientIn.ReadLine();
                Console.WriteLine($"Received request: {msgIn}");
                Request? request = JsonSerializer.Deserialize<Request>(msgIn);
                Response response;
                if (request == null)
                {
                    response = new()
                    {
                        Type = ResponseType.ERROR,
                        Message = "Request couldn't be deserialized"
                    };
                }
                else
                {
                    try
                    {
                        if (request.Type == RequestType.DISCONNECTING)
                        {
                            connected = false;
                            break;
                        }
                        response = request.Type switch
                        {
                            RequestType.POST_PLANNING => await CreatePlanning(request),
                            RequestType.POST_PAYMENT => await CreatePayment(request),
                            RequestType.REMOVE_PLANNING => await RemovePlanning(request),
                            _ => new()
                            {
                                Type = ResponseType.ERROR,
                                Message = $"Wrong request type: {request.Type}"
                            },
                        };
                    }
                    catch (Exception ex)
                    {
                        response = new()
                        {
                            Type = ResponseType.ERROR,
                            Message = ex.Message
                        };
                    }
                }
                string responseMsg = JsonSerializer.Serialize(response);
                Console.WriteLine($"Sendind response to client: {responseMsg}");
                clientOut.WriteLine(responseMsg);
            }
        }

        private async Task<Response> CreatePlanning(Request request)
        {
            Console.WriteLine("Creating planning...");
            Planning planning = await _service.CreatePlanning(request.Name, request.Cnp, request.Date, request.TreatmentLocation, request.TreatmentType,
                request.TreatmentDate);
            return new()
            {
                Type = ResponseType.OK,
                Id = planning.Id,
                Name = planning.Name,
                Cnp = planning.Cnp,
                Date = planning.Date,
                TreatmentLocation = planning.TreatmentLocation,
                Treatment = planning.Treatment,
                TreatmentDate = planning.TreatmentDate
            };
        }

        private async Task<Response> CreatePayment(Request request)
        {
            Console.WriteLine("Creating payment...");
            Payment payment = await _service.CreatePayment(request.Id, request.Cnp, request.Date, request.Sum);
            return new()
            {
                Type = ResponseType.OK,
                Id = payment.Id,
                PlanningId = payment.PlanningId,
                Date = payment.Date,
                Cnp = payment.Cnp,
                Sum = payment.Sum
            };
        }

        private async Task<Response> RemovePlanning(Request request)
        {
            Console.WriteLine("Removing planning...");
            await _service.RemovePlanning(request.Id);
            return new()
            {
                Type = ResponseType.OK
            };
        }

        private async void VerifySistem()
        {
            while (true)
            {
                Thread.Sleep(_millisecondsToVerify);

                //verify
                Console.WriteLine("Verifying system...");
                await _service.VerifyPlannings(_numberOfLocations);
                Console.WriteLine("Verification complete!");
            }
        }
    }
}
