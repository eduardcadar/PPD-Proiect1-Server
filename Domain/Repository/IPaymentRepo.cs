using Domain.Domain;

namespace Domain.Repository
{
    public interface IPaymentRepo
    {
        public Task<Payment> Add(Payment payment);
        public Task<List<Payment>> GetAll();
    }
}
