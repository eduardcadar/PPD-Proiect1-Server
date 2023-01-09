using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class TreatmentLocationDbRepo : ITreatmentLocationRepo
    {
        private readonly DatabaseContext _dbContext;

        public TreatmentLocationDbRepo(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<LocationTreatment> Add(LocationTreatment locationTreatment)
        {
            var dbLocationTreatment = EntityUtils.LocationTreatmentToDbLocationTreatment(locationTreatment);
            await _dbContext.LocationTreatments.AddAsync(dbLocationTreatment);
            await _dbContext.SaveChangesAsync();
            return locationTreatment;
        }

        public async Task AddRange(List<LocationTreatment> locationTreatments)
        {
            foreach (LocationTreatment locationTreatment in locationTreatments)
            {
                var dbLocationTreatment = EntityUtils.LocationTreatmentToDbLocationTreatment(locationTreatment);
                await _dbContext.LocationTreatments.AddAsync(dbLocationTreatment);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task Clear()
        {
            _dbContext.LocationTreatments.RemoveRange(_dbContext.LocationTreatments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<LocationTreatment>> GetAll()
        {
            var dbLocationTreatments = await _dbContext.LocationTreatments.ToListAsync();
            var locationTreatments = dbLocationTreatments
                .Select(EntityUtils.DbLocationTreatmentToLocationTreatment) .ToList();
            return locationTreatments;
        }
    }
}
