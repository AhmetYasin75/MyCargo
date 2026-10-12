using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Business.Services.Pricing
{
    public interface ICargoPricingService
    {
        Task<CargoPriceResultDto> CalculateAsync(
            double weight, double desi, DeliveryType deliveryType,
            Guid originBranchId, Guid destinationBranchId, bool hasInsurance);
    }
}