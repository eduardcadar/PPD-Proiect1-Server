using Domain.Domain;

namespace Domain.Repository
{
    public interface IPlanningRepo
    {
        Task<Planning> Add(Planning planificare);
        Task<Planning> Delete(int idPlanning);
        Task<List<Planning>> GetAll();
    }
}
