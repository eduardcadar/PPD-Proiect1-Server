using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class PaymentDbRepo : IPaymentRepo
    {
        private readonly DbContextOptions<DatabaseContext> _options;

        public PaymentDbRepo(DbContextOptions<DatabaseContext> options)
        {
            _options = options;
        }

        private DatabaseContext InitializeDbContext()
        {
            DatabaseContext dbContext = new(_options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        public async Task<Payment> Add(Payment payment)
        {
            var dbContext = InitializeDbContext();
            var dbPayment = EntityUtils.PaymentToDbPayment(payment);
            await dbContext.Payments.AddAsync(dbPayment);
            await dbContext.SaveChangesAsync();
            payment.Id = dbPayment.Id;
            return payment;
        }

        public async Task<List<Payment>> GetAll()
        {
            var dbContext = InitializeDbContext();
            var dbPayments = await dbContext.Payments.ToListAsync();
            var payments = dbPayments
                .Select(EntityUtils.DbPaymentToPayment).ToList();
            return payments;
        }
    }
}
