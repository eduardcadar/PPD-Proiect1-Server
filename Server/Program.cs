using Application;
using Domain.Domain;
using Domain.Repository;
using Infrastructure.DataAccess;
using Infrastructure.Persistence;
using System.Configuration;

Console.WriteLine("Server starting...");

string connectionString = ConfigurationManager.AppSettings["connectionString"];

DatabaseContext dbContext = new();

IPlanningRepo planningRepo = new PlanningDbRepo(dbContext);
IPaymentRepo paymentRepo = new PaymentDbRepo(dbContext);
ITreatmentRepo treatmentRepo = new TreatmentDbRepo(dbContext);
ITreatmentLocationRepo treatmentLocationRepo = new TreatmentLocationDbRepo(dbContext);

Service planningsService = new(planningRepo, paymentRepo, treatmentRepo, treatmentLocationRepo);

List<Treatment> treatments = new()
{
    new() { TreatmentType = 1, Cost = 50, Duration = 120 },
    new() { TreatmentType = 2, Cost = 20, Duration = 20 },
    new() { TreatmentType = 3, Cost = 40, Duration = 30 },
    new() { TreatmentType = 4, Cost = 100, Duration = 60 },
    new() { TreatmentType = 5, Cost = 30, Duration = 30 }
};

List<LocationTreatment> locationTreatments = new()
{
    new() { Location = 1, Treatment = treatments.First(t => t.TreatmentType == 1), MaxPatients = 3 },
    new() { Location = 1, Treatment = treatments.First(t => t.TreatmentType == 2), MaxPatients = 1 },
    new() { Location = 1, Treatment = treatments.First(t => t.TreatmentType == 3), MaxPatients = 1 },
    new() { Location = 1, Treatment = treatments.First(t => t.TreatmentType == 4), MaxPatients = 2 },
    new() { Location = 1, Treatment = treatments.First(t => t.TreatmentType == 5), MaxPatients = 1 }
};
int numberOfLocations = 5;
for (int i = 2; i <= numberOfLocations; i++)
    foreach (Treatment treatment in treatments)
        locationTreatments.Add(new()
        {
            Location = i,
            Treatment = treatment,
            MaxPatients = locationTreatments
                .First(lt => lt.Location == i && lt.Treatment.TreatmentType == treatment.TreatmentType).MaxPatients * (i - 1)
        });

int noThreads = 10;
int millisecondsToRun = 180000;
int millisecondsToVerify = 5000;
Server.Server server = new(planningsService);
server.SetTreatments(treatments, locationTreatments, numberOfLocations);
await server.StartServer(noThreads, millisecondsToRun, millisecondsToVerify);
