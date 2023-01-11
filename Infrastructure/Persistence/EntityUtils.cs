using Domain.Domain;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence
{
    public static class EntityUtils
    {
        public static DbPayment PaymentToDbPayment(Payment payment)
        {
            return new()
            {
                Id = payment.Id,
                PlanningId = payment.PlanningId,
                Date = payment.Date,
                Cnp = payment.Cnp,
                Sum = payment.Sum
            };
        }

        public static Payment DbPaymentToPayment(DbPayment dbPayment)
        {
            return new()
            {
                Id = dbPayment.Id,
                PlanningId = dbPayment.PlanningId,
                Date = dbPayment.Date,
                Cnp = dbPayment.Cnp,
                Sum = dbPayment.Sum
            };
        }

        public static Planning DbPlanningToPlanning(DbPlanning dbPlanning)
        {
            return new()
            {
                Id = dbPlanning.Id,
                Name = dbPlanning.Name,
                Cnp = dbPlanning.Cnp,
                Date = dbPlanning.Date,
                TreatmentLocation = dbPlanning.TreatmentLocation,
                Treatment = DbTreatmentToTreatment(dbPlanning.Treatment),
                TreatmentDate = dbPlanning.TreatmentDate
            };
        }

        public static DbPlanning PlanningToDbPlanning(Planning planning)
        {
            return new()
            {
                Id = planning.Id,
                Name = planning.Name,
                Cnp = planning.Cnp,
                Date = planning.Date,
                TreatmentLocation = planning.TreatmentLocation,
                Treatment = TreatmentToDbTreatment(planning.Treatment),
                TreatmentDate = planning.TreatmentDate
            };
        }

        public static DbTreatment TreatmentToDbTreatment(Treatment treatment)
        {
            return new()
            {
                TreatmentType = treatment.TreatmentType,
                Cost = treatment.Cost,
                Duration = treatment.Duration
            };
        }

        public static Treatment DbTreatmentToTreatment(DbTreatment dbTreatment)
        {
            return new()
            {
                TreatmentType = dbTreatment.TreatmentType,
                Cost = dbTreatment.Cost,
                Duration = dbTreatment.Duration
            };
        }

        public static DbLocationTreatment LocationTreatmentToDbLocationTreatment(LocationTreatment locationTreatment)
        {
            return new()
            {
                //Treatment = TreatmentToDbTreatment(locationTreatment.Treatment),
                TreatmentType = (int)locationTreatment.Treatment.TreatmentType,
                Location = locationTreatment.Location,
                MaxPatients = locationTreatment.MaxPatients
            };
        }

        public static LocationTreatment DbLocationTreatmentToLocationTreatment(DbLocationTreatment dbLocationTreatment)
        {
            return new()
            {
                Treatment = DbTreatmentToTreatment(dbLocationTreatment.Treatment),
                Location = dbLocationTreatment.Location,
                MaxPatients = dbLocationTreatment.MaxPatients
            };
        }
    }
}
