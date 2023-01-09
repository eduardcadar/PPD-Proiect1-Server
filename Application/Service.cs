using Domain.Domain;
using Domain.Repository;
using System.Text;

namespace Application
{
    public class Service
    {
        private readonly IPlanningRepo _planningRepo;
        private readonly IPaymentRepo _paymentRepo;
        private readonly ITreatmentRepo _treatmentRepo;
        private readonly ITreatmentLocationRepo _treatmentLocationRepo;

        public Service(IPlanningRepo planningRepo, IPaymentRepo paymentRepo, ITreatmentRepo treatmentRepo, ITreatmentLocationRepo treatmentLocationRepo)
        {
            _planningRepo = planningRepo;
            _paymentRepo = paymentRepo;
            _treatmentRepo = treatmentRepo;
            _treatmentLocationRepo = treatmentLocationRepo;
        }

        public async Task<Planning> CreatePlanning(string name, string cnp, DateTime date,
            int location, int treatmentType, DateTime treatmentDate)
        {
            MutexUtils.MutexOne.WaitOne();
            Planning p = new()
            {
                Name = name,
                Cnp = cnp,
                Date = date,
                TreatmentLocation = location,
                Treatment = new Treatment() { TreatmentType = treatmentType },
                TreatmentDate = treatmentDate
            };
            var planning = await _planningRepo.Add(p);
            MutexUtils.MutexOne.ReleaseMutex();
            return planning;
        }

        public async Task<Payment> CreatePayment(int planningId, string cnp, DateTime date, int sum)
        {
            MutexUtils.MutexTwo.WaitOne();
            Payment p = new()
            {
                PlanningId = planningId,
                Cnp = cnp,
                Date = date,
                Sum = sum
            };
            var payment = await _paymentRepo.Add(p);
            MutexUtils.MutexTwo.ReleaseMutex();
            return payment;
        }

        public async Task RemovePlanning(int planningId)
        {
            MutexUtils.MutexOne.WaitOne();
            MutexUtils.MutexTwo.WaitOne();
            var planning = await _planningRepo.Delete(planningId);
            Payment payment = new()
            {
                PlanningId = null,
                Cnp = planning.Cnp,
                Date = DateTime.Now,
                Sum = -planning.Treatment.Cost
            };
            await _paymentRepo.Add(payment);
            MutexUtils.MutexTwo.ReleaseMutex();
            MutexUtils.MutexOne.ReleaseMutex();
        }

        public async Task<Treatment> CreateTreatment(int treatmentType, int cost,
            int duration)
        {
            Treatment treatment = new()
            {
                TreatmentType = treatmentType,
                Cost = cost,
                Duration = duration
            };
            return await _treatmentRepo.Add(treatment);
        }

        public async Task<LocationTreatment> CreateLocationTreatment(int location,
            int treatmentType, int maxPatients)
        {
            LocationTreatment treatment = new()
            {
                Location = location,
                Treatment = new() { TreatmentType = treatmentType },
                MaxPatients = maxPatients
            };
            return await _treatmentLocationRepo.Add(treatment);
        }

        public async void SetTreatments(List<Treatment> treatments, List<LocationTreatment> locationTreatments)
        {
            await _treatmentRepo.Clear();
            await _treatmentLocationRepo.Clear();
            await _treatmentRepo.AddRange(treatments);
            await _treatmentLocationRepo.AddRange(locationTreatments);
        }

        public async Task VerifyPlannings(int numberOfLocations)
        {
            List<Planning> plannings = await _planningRepo.GetAll();
            List<Payment> payments = await _paymentRepo.GetAll();
            List<Treatment> treatments = await _treatmentRepo.GetAll();
            List<LocationTreatment> locationTreatments = await _treatmentLocationRepo.GetAll();
            StringBuilder sb = new();
            StringBuilder sbNotPaid = new();

            for (int i = 1; i <= numberOfLocations; i++)
            {
                List<int> locationPlanningsIds = plannings
                    .Where(p => p.TreatmentLocation == i)
                    .Select(p => p.TreatmentLocation).ToList();
                List<Payment> locationPayments = payments
                    .Where(p => p.PlanningId != null)
                    .Where(p => locationPlanningsIds.Contains((int)p.PlanningId!)).ToList();
                int locationSum = locationPayments.Select(p => p.Sum).Sum();

                sb.Append($"Ora verificarii: {DateTime.Now}")
                    .AppendLine()
                    .Append($"Sold locatie {i}: {locationSum}")
                    .AppendLine();
            }

            foreach (Planning planning in plannings)
            {
                int numberOfPlannings = plannings
                    .Where(p => p.TreatmentLocation == planning.TreatmentLocation
                    && p.Treatment.TreatmentType == planning.Treatment.TreatmentType
                    && (p.TreatmentDate >= planning.TreatmentDate && p.TreatmentDate < planning.TreatmentDate.AddMinutes(planning.Treatment.Duration)
                        || planning.TreatmentDate >= p.TreatmentDate && planning.TreatmentDate < p.TreatmentDate.AddMinutes(p.Treatment.Duration)))
                    .Count();
                Treatment treatment = planning.Treatment;
                LocationTreatment locationTreatment = locationTreatments
                    .Single(lt => lt.Location == planning.TreatmentLocation
                    && lt.Treatment.TreatmentType == planning.Treatment.TreatmentType);
                if (numberOfPlannings > locationTreatment.MaxPatients)
                    throw new Exception($"Too many planings at the same time with {planning}");

                List<Payment> planningPayments = payments
                    .Where(p => p.PlanningId == planning.Id).ToList();
                int sum = planningPayments.Select(p => p.Sum).Sum();
                if (sum != planning.Treatment.Cost)
                    sbNotPaid.Append($"Treatment costs {treatment.Cost}, but {sum} was paid")
                        .AppendLine();
            }
            sb.Append(sbNotPaid);

        }
    }
}
