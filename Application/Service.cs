﻿using Domain.Domain;
using Domain.Repository;
using System.Globalization;
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
            string fileName = $"./Files/verif{DateTime.Now}";
            List <Planning> plannings = await _planningRepo.GetAll();
            List<Payment> payments = await _paymentRepo.GetAll();
            List<Treatment> treatments = await _treatmentRepo.GetAll();
            List<LocationTreatment> locationTreatments = await _treatmentLocationRepo.GetAll();
            StringBuilder sb = new();
            StringBuilder sbNotPaid = new();
            StringBuilder sbNumberOfPlannings = new();
            sb.AppendLine("-----VERIFICARE-----");

            for (int locationNumber = 1; locationNumber <= numberOfLocations; locationNumber++)
            {
                sbNotPaid.Clear();
                sbNumberOfPlannings.Clear();
                List<Planning> locationPlannings = plannings
                    .Where(p => p.TreatmentLocation == locationNumber).ToList();
                List<int> locationPlanningsIds = locationPlannings
                    .Select(p => p.TreatmentLocation).ToList();
                List<Payment> locationPayments = payments
                    .Where(p => p.PlanningId != null)
                    .Where(p => locationPlanningsIds.Contains((int)p.PlanningId!)).ToList();
                int locationSum = locationPayments.Select(p => p.Sum).Sum();

                sb.AppendLine($"Ora verificarii: {DateTime.Now}")
                    .AppendLine($"Sold locatie {locationNumber}: {locationSum}");

                // lista programarilor neplatite
                foreach (Planning planning in locationPlannings)
                {
                    List<Payment> planningPayments = payments
                        .Where(p => p.PlanningId == planning.Id).ToList();
                    int sum = planningPayments.Select(p => p.Sum).Sum();
                    if (sum != planning.Treatment.Cost)
                        sbNotPaid.AppendLine($"Planning {planning.Id}: treatment costs {planning.Treatment.Cost}, but {sum} was paid");
                }

                // pentru fiecare tip de programare (tratament)
                foreach (Treatment treatment in treatments)
                {
                    LocationTreatment locationTreatment = locationTreatments
                        .Single(lt => lt.Location == locationNumber
                        && lt.Treatment.TreatmentType == treatment.TreatmentType);
                    sbNumberOfPlannings.AppendLine($"---Treatment {treatment.TreatmentType}, max plannings: {locationTreatment.MaxPatients}");
                    HashSet<TimeOnly> changes = new()
                    {
                        TimeOnly.Parse("10:00"),
                        TimeOnly.Parse("18:00")
                    };
                    var locationTreatmentPlannings = plannings
                        .Where(p => p.Treatment.TreatmentType == treatment.TreatmentType && p.TreatmentLocation == locationNumber);
                    changes.UnionWith(locationTreatmentPlannings.Select(p => TimeOnly.FromDateTime(p.TreatmentDate)));
                    changes.UnionWith(locationTreatmentPlannings.Select(p => TimeOnly.FromDateTime(p.TreatmentDate.AddMinutes(p.Treatment.Duration))));
                    List<TimeOnly> changesList = changes.ToList();
                    changesList.Sort((p1, p2) => p1.CompareTo(p2));
                    for (int j = 0; j < changesList.Count - 1; j++)
                    {
                        TimeOnly change = changesList[j];
                        TimeOnly nextChange = changesList[j + 1];
                        int numberOfPlannings = locationTreatmentPlannings.Where(p =>
                            {
                                var planningTimeOnly = TimeOnly.FromDateTime(p.TreatmentDate);
                                return planningTimeOnly >= change && planningTimeOnly < nextChange
                                    || change >= planningTimeOnly && change < planningTimeOnly.AddMinutes(p.Treatment.Duration);
                            }).Count();

                        sbNumberOfPlannings.AppendLine($"{change}-{nextChange}: {numberOfPlannings} plannings");
                    };
                }
                sb.Append(sbNotPaid);
                sb.Append(sbNumberOfPlannings);
            } 
            File.AppendAllText(fileName, sb.ToString());
        }
    }
}
