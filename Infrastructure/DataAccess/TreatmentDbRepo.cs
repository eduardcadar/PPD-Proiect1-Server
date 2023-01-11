using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class TreatmentDbRepo : ITreatmentRepo
    {
        private readonly DbContextOptions<DatabaseContext> _options;

        public TreatmentDbRepo(DbContextOptions<DatabaseContext> options)
        {
            _options = options;
        }

        private DatabaseContext InitializeDbContext()
        {
            DatabaseContext dbContext = new(_options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        public async Task<Treatment> Add(Treatment treatment)
        {
            var dbContext = InitializeDbContext();
            var dbTreatment = EntityUtils.TreatmentToDbTreatment(treatment);
            await dbContext.Treatments.AddAsync(dbTreatment);
            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSql($"SET IDENTITY_INSERT Treatments ON;");
            await dbContext.SaveChangesAsync();
            dbContext.Database.ExecuteSql($"SET IDENTITY_INSERT Treatments OFF;");
            return treatment;
        }

        public async Task AddRange(List<Treatment> treatments)
        {
            var dbContext = InitializeDbContext();
            foreach (Treatment treatment in treatments)
            {
                var dbTreatment = EntityUtils.TreatmentToDbTreatment(treatment);
                await dbContext.Treatments.AddAsync(dbTreatment);
            }
            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSql($"SET IDENTITY_INSERT Treatments ON;");
            await dbContext.SaveChangesAsync();
            dbContext.Database.ExecuteSql($"SET IDENTITY_INSERT Treatments OFF;");
        }

        public async Task Clear()
        {
            var dbContext = InitializeDbContext();
            dbContext.Treatments.RemoveRange(dbContext.Treatments);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Treatment>> GetAll()
        {
            var dbContext = InitializeDbContext();
            var dbTreatments = await dbContext.Treatments.ToListAsync();
            var treatments = dbTreatments
                .Select(EntityUtils.DbTreatmentToTreatment).ToList();
            return treatments;
        }
    }
}
