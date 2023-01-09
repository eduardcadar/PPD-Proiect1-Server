using Domain.Domain;

namespace Domain.Repository
{
    public interface ITreatmentLocationRepo
    {
        Task<LocationTreatment> Add(LocationTreatment locationTreatment);
        Task AddRange(List<LocationTreatment> locationTreatments);
        Task Clear();
        Task<List<LocationTreatment>> GetAll();
    }
}
