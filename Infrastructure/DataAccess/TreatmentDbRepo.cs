using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class TreatmentDbRepo : ITreatmentRepo
    {
        private readonly DatabaseContext _dbContext;

        public TreatmentDbRepo(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<Treatment> Add(Treatment treatment)
        {
            var dbTreatment = EntityUtils.TreatmentToDbTreatment(treatment);
            await _dbContext.Treatments.AddAsync(dbTreatment);
            await _dbContext.SaveChangesAsync();
            return treatment;
        }

        public async Task AddRange(List<Treatment> treatments)
        {
            foreach (Treatment treatment in treatments)
            {
                var dbTreatment = EntityUtils.TreatmentToDbTreatment(treatment);
                await _dbContext.Treatments.AddAsync(dbTreatment);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task Clear()
        {
            _dbContext.Treatments.RemoveRange(_dbContext.Treatments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Treatment>> GetAll()
        {
            var dbTreatments = await _dbContext.Treatments.ToListAsync();
            var treatments = dbTreatments
                .Select(EntityUtils.DbTreatmentToTreatment).ToList();
            return treatments;
        }
    }
}
