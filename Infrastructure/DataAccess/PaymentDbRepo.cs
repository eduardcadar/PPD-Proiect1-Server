using Domain.Domain;
using Domain.Repository;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public class PaymentDbRepo : IPaymentRepo
    {
        private readonly DatabaseContext _dbContext;

        public PaymentDbRepo(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<Payment> Add(Payment payment)
        {
            var dbPayment = EntityUtils.PaymentToDbPayment(payment);
            await _dbContext.Payments.AddAsync(dbPayment);
            await _dbContext.SaveChangesAsync();
            payment.Id = dbPayment.Id;
            return payment;
        }

        public async Task<List<Payment>> GetAll()
        {
            var dbPayments = await _dbContext.Payments.ToListAsync();
            var payments = dbPayments
                .Select(EntityUtils.DbPaymentToPayment).ToList();
            return payments;
        }
    }
}
