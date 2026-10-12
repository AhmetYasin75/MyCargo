namespace CargoTrack.DTO.DTOs.TransferCenterDtos
{
    // Neden: Arama ve sayfalama değerleri URL'den (?search=...&page=2) bu nesneye dolar.
    public class TransferCenterFilterDto
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}