using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class TreatmentLocationDbRepo : ITreatmentLocationRepo
    {
        private readonly DbContextOptions<DatabaseContext> _options;

        public TreatmentLocationDbRepo(DbContextOptions<DatabaseContext> options)
        {
            _options = options;
        }

        private DatabaseContext InitializeDbContext()
        {
            DatabaseContext dbContext = new(_options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        public async Task<LocationTreatment> Add(LocationTreatment locationTreatment)
        {
            var dbContext = InitializeDbContext();
            var dbLocationTreatment = EntityUtils.LocationTreatmentToDbLocationTreatment(locationTreatment);
            await dbContext.LocationTreatments.AddAsync(dbLocationTreatment);
            await dbContext.SaveChangesAsync();
            return locationTreatment;
        }

        public async Task AddRange(List<LocationTreatment> locationTreatments)
        {
            var dbContext = InitializeDbContext();
            foreach (LocationTreatment locationTreatment in locationTreatments)
            {
                var dbLocationTreatment = EntityUtils.LocationTreatmentToDbLocationTreatment(locationTreatment);
                await dbContext.LocationTreatments.AddAsync(dbLocationTreatment);
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task Clear()
        {
            var dbContext = InitializeDbContext();
            dbContext.LocationTreatments.RemoveRange(dbContext.LocationTreatments);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<LocationTreatment>> GetAll()
        {
            var dbContext = InitializeDbContext();
            var dbLocationTreatments = await dbContext.LocationTreatments.Include(lt => lt.Treatment).ToListAsync();
            var locationTreatments = dbLocationTreatments
                .Select(EntityUtils.DbLocationTreatmentToLocationTreatment) .ToList();
            return locationTreatments;
        }
    }
}
