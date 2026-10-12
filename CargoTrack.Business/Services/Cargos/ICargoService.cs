using CargoTrack.DTO.DTOs.Cargos;

namespace CargoTrack.Business.Services.Cargos
{
    public interface ICargoService
    {
        Task<List<ResultCargoDto>> GetAllAsync();
        Task<UpdateCargoDto> GetByIdAsync(Guid id);
        Task<ResultCargoDto> GetByTrackCodeAsync(string trackCode);
        Task CreateAsync(CreateCargoDto createCargoDto);
        Task UpdateAsync(UpdateCargoDto updateCargoDto);
        Task DeleteAsync(Guid id);

        // Faz 3
        Task ChangeStatusAsync(ChangeCargoStatusDto changeStatusDto);
        Task<List<CargoMovementDto>> GetMovementsAsync(Guid cargoId);

        // Faz 5
        Task VerifyDeliveryAsync(VerifyDeliveryDto verifyDeliveryDto);
        Task ReportDeliveryFailureAsync(ReportDeliveryFailureDto failureDto);

        // Faz 8C: Details sayfasında hangi işlemlerin yapılabileceği
        Task<CargoActionsDto> GetAvailableActionsAsync(Guid cargoId);
    }
}