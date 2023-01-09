using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class PlanningDbRepo : IPlanningRepo
    {
        private readonly DatabaseContext _dbContext;

        public PlanningDbRepo(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<Planning> Add(Planning planning)
        {
            int numberOfPlannings = await _dbContext.Plannings
                .Where(p => p.TreatmentLocation == planning.TreatmentLocation
                    && p.TreatmentType == planning.Treatment.TreatmentType
                    && (p.TreatmentDate >= planning.TreatmentDate && p.TreatmentDate < planning.TreatmentDate.AddMinutes(planning.Treatment.Duration)
                        || planning.TreatmentDate >= p.TreatmentDate && planning.TreatmentDate < p.TreatmentDate.AddMinutes(p.Treatment.Duration)))
                .CountAsync();
            var locationTreatment = await _dbContext.LocationTreatments
                .SingleAsync(lt => lt.Location == planning.TreatmentLocation && lt.TreatmentType == planning.Treatment.TreatmentType);
            if (numberOfPlannings >= locationTreatment.MaxPatients)
                throw new Exception("Programare nereusita (nu mai sunt locuri disponibile)");
            var dbPlanning = EntityUtils.PlanningToDbPlanning(planning);
            dbPlanning.Treatment = null;
            await _dbContext.Plannings.AddAsync(dbPlanning);
            await _dbContext.SaveChangesAsync();
            planning.Id = dbPlanning.Id;
            planning.Treatment = EntityUtils.DbTreatmentToTreatment(await _dbContext.Treatments
                .SingleOrDefaultAsync(t => t.TreatmentType == planning.Treatment.TreatmentType));
            return planning;
        }

        public async Task<Planning> Delete(int idPlanning)
        {
            var dbPlanning = await _dbContext.Plannings
                .SingleOrDefaultAsync(p => p.Id == idPlanning);
            if (dbPlanning == null)
                throw new Exception("The planning doesn't exist");

            var planningPayments = await _dbContext.Payments
                .Where(p => p.PlanningId == idPlanning).ToListAsync();
            foreach (var payment in planningPayments)
                payment.PlanningId = null;
            
            _dbContext.Plannings.Remove(dbPlanning);
            await _dbContext.SaveChangesAsync();
            return EntityUtils.DbPlanningToPlanning(dbPlanning);
        }

        public async Task<List<Planning>> GetAll()
        {
            var dbPlannings = await _dbContext.Plannings.ToListAsync();
            var plannings = dbPlannings
                .Select(EntityUtils.DbPlanningToPlanning).ToList();
            return plannings;
        }
    }
}
