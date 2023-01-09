using Domain.Domain;

namespace Domain.Repository
{
    public interface ITreatmentRepo
    {
        Task<Treatment> Add(Treatment treatment);
        Task AddRange(List<Treatment> treatments);
        Task Clear();
        Task<List<Treatment>> GetAll();
    }
}
