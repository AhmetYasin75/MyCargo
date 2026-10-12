using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Fiyat servisi hem ücreti hem bölgeyi döndürür. Bölge, tahmini teslim süresi için de lazım.
    public class CargoPriceResultDto
    {
        public PriceZone Zone { get; set; }
        public decimal Price { get; set; }
    }
}