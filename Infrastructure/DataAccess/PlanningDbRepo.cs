using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class PlanningDbRepo : IPlanningRepo
    {
        private readonly DbContextOptions<DatabaseContext> _options;

        public PlanningDbRepo(DbContextOptions<DatabaseContext> options)
        {
            _options = options;
        }

        private DatabaseContext InitializeDbContext()
        {
            DatabaseContext dbContext = new(_options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        public async Task<Planning> Add(Planning planning)
        {
            var dbContext = InitializeDbContext();
            int numberOfPlannings = await dbContext.Plannings
                .Where(p => p.TreatmentLocation == planning.TreatmentLocation
                    && p.TreatmentType == planning.Treatment.TreatmentType
                    && (p.TreatmentDate >= planning.TreatmentDate && p.TreatmentDate < planning.TreatmentDate.AddMinutes(planning.Treatment.Duration)
                        || planning.TreatmentDate >= p.TreatmentDate && planning.TreatmentDate < p.TreatmentDate.AddMinutes(p.Treatment.Duration)))
                .CountAsync();
            var locationTreatment = await dbContext.LocationTreatments
                .SingleAsync(lt => lt.Location == planning.TreatmentLocation && lt.TreatmentType == planning.Treatment.TreatmentType);
            if (numberOfPlannings >= locationTreatment.MaxPatients)
                throw new Exception("Programare nereusita (nu mai sunt locuri disponibile)");
            var dbPlanning = EntityUtils.PlanningToDbPlanning(planning);
            dbPlanning.Treatment = null;
            await dbContext.Plannings.AddAsync(dbPlanning);
            await dbContext.SaveChangesAsync();
            planning.Id = dbPlanning.Id;
            planning.Treatment = EntityUtils.DbTreatmentToTreatment(await dbContext.Treatments
                .SingleOrDefaultAsync(t => t.TreatmentType == planning.Treatment.TreatmentType));
            return planning;
        }

        public async Task<Planning> Delete(int idPlanning)
        {
            var dbContext = InitializeDbContext();
            var dbPlanning = await dbContext.Plannings
                .SingleOrDefaultAsync(p => p.Id == idPlanning);
            if (dbPlanning == null)
                throw new Exception("The planning doesn't exist");

            var planningPayments = await dbContext.Payments
                .Where(p => p.PlanningId == idPlanning).ToListAsync();
            foreach (var payment in planningPayments)
                payment.PlanningId = null;
            
            dbContext.Plannings.Remove(dbPlanning);
            await dbContext.SaveChangesAsync();
            return EntityUtils.DbPlanningToPlanning(dbPlanning);
        }

        public async Task<List<Planning>> GetAll()
        {
            var dbContext = InitializeDbContext();
            var dbPlannings = await dbContext.Plannings.Include(p => p.Treatment).ToListAsync();
            var plannings = dbPlannings
                .Select(EntityUtils.DbPlanningToPlanning).ToList();
            return plannings;
        }
    }
}
