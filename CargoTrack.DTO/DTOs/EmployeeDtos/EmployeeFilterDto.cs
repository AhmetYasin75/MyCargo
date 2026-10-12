namespace CargoTrack.DTO.DTOs.EmployeeDtos
{
    // Neden: Arama, şube filtresi ve sayfalama değerleri URL'den bu nesneye dolar.
    public class EmployeeFilterDto
    {
        public string? Search { get; set; }
        public Guid? BranchId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}