using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Arama, filtre ve sayfalama değerleri URL'den (?search=...&status=...) bu nesneye dolar.
    public class CargoFilterDto
    {
        public string Search { get; set; }
        public CargoStatus? Status { get; set; }
        public CargoListScope Scope { get; set; } = CargoListScope.All;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}