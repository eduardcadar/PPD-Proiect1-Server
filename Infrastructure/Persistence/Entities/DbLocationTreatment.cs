using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities
{
    public class DbLocationTreatment
    {
        public int Location { get; set; }
        [ForeignKey("Treatment")]
        public int TreatmentType { get; set; }
        public DbTreatment Treatment { get; set; }
        public int MaxPatients { get; set; }
    }
}
