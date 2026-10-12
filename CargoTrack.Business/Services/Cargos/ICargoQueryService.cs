using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.DTO.DTOs.Common;

namespace CargoTrack.Business.Services.Cargos
{
    // Neden: Listeleme/detay (okuma) işleri ayrı serviste. CargoService yazma işlerine (oluştur, durum değiştir...) odaklı kalır.
    public interface ICargoQueryService
    {
        Task<PagedResult<ResultCargoDto>> GetPagedAsync(CargoFilterDto filter);
        Task<CargoDetailDto> GetDetailAsync(Guid id);
    }
}