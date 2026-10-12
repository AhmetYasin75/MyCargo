using CargoTrack.DTO.DTOs.CargoPriceDtos;

namespace CargoTrack.Business.Services.CargoPrices
{
    public interface ICargoPriceService
    {
        Task<List<ResultCargoPriceDto>> GetAllAsync();
        Task<UpdateCargoPriceDto> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateCargoPriceDto dto);
    }
}