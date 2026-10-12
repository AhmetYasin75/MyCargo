using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    // Neden: Kargo her zaman iki şube arasında gitmez, arada transfer merkezi olabilir.
    public class TransferCenter : BaseEntity
    {
        public string Name { get; set; }
        public Guid CityId { get; set; }

        public virtual City City { get; set; }
    }
}