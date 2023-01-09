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
        private int _numberOfLocations;

        public Server(Service planningsService)
        {
            _service = planningsService;
        }

        public void SetTreatments(List<Treatment> treatments, List<LocationTreatment> locationTreatments, int numberOfLocations)
        {
            _numberOfLocations = numberOfLocations;
            _service.SetTreatments(treatments, locationTreatments);
        }

            public async Task StartServer(int noThreads, int millisecondsToRun, int millisecondsToVerify)
        {
            _millisecondsToVerify = millisecondsToVerify;
            ThreadPool.SetMaxThreads(noThreads, noThreads);
            
            System.Timers.Timer timer = new()
            {
                Interval = millisecondsToRun
            };
            timer.Elapsed += (sender, e) => {
                timer.Dispose();

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
                TcpClient client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(async (_) => await SolveClient(client));
                
            }
        }

        private async Task SolveClient(TcpClient client)
        {
            StreamReader clientIn = new(client.GetStream());
            StreamWriter clientOut = new(client.GetStream())
            {
                AutoFlush = true
            };

            string msgIn = clientIn.ReadToEnd();
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
            clientOut.Write(responseMsg);
        }

        private async Task<Response> CreatePlanning(Request request)
        {
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
            await _service.RemovePlanning(request.Id);
            return new()
            {
                Type = ResponseType.OK
            };
        }

        private void VerifySistem()
        {
            Thread.Sleep(_millisecondsToVerify);

            //verify
            _service.VerifyPlannings(_numberOfLocations);
        }
    }
}
