namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: "En yoğun şubeler" listesinin bir satırı.
    public class BranchLoadDto
    {
        public string BranchName { get; set; } = string.Empty;
        public int CargoCount { get; set; }
    }
}