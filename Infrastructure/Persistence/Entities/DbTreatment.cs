using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities
{
    public class DbTreatment
    {
        [Key]
        public int TreatmentType { get; set; }
        public int Cost { get; set; }
        public int Duration { get; set; }
    }
}
