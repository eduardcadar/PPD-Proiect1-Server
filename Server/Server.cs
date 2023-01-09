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
                StreamReader clientIn = new(client.GetStream());
                StreamWriter clientOut = new(client.GetStream())
                {
                    AutoFlush = true
                };

                string msgIn = clientIn.ReadToEnd();
                //get request
                Request? request = JsonSerializer.Deserialize<Request>(msgIn);
                if (request == null)
                {
                    Response response = new()
                    {
                        Type = ResponseType.ERROR,
                        Message = "Request couldn't be deserialized"
                    };
                    string responseMsg = JsonSerializer.Serialize(response);
                    clientOut.Write(responseMsg);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(async (_) => await SolveRequest(request));
                }
            }
        }

        private async Task SolveRequest(Request request)
        {
            switch (request.Type)
            {
                case RequestType.POST_PLANNING:
                    await CreatePlanning(request); break;
                case RequestType.POST_PAYMENT:
                    await CreatePayment(request); break;
                case RequestType.REMOVE_PLANNING:
                    await RemovePlanning(request); break;
                default:
                    Console.WriteLine($"Wrong request type: {request.Type}"); break;
            }
            //send response
        }

        private async Task CreatePlanning(Request request)
        {
            await _service.CreatePlanning(request.Name, request.Cnp, request.Date, request.TreatmentLocation, request.TreatmentType,
                request.TreatmentDate);
        }

        private async Task CreatePayment(Request request)
        {
            await _service.CreatePayment(request.Id, request.Cnp, request.Date, request.Sum);
        }

        private async Task RemovePlanning(Request request)
        {
            await _service.RemovePlanning(request.Id);
        }

        private void VerifySistem()
        {
            Thread.Sleep(_millisecondsToVerify);

            //verify
            _service.VerifyPlannings(_numberOfLocations);
        }
    }
}
