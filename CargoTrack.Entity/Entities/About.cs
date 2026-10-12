using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class About : BaseEntity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
    }
}