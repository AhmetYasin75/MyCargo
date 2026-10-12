namespace CargoTrack.DTO.DTOs.BranchDtos
{
    public class CreateBranchDto
    {
        public required string Name { get; set; }
        public Guid CityId { get; set; }
    }
}