using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.CargoPrices;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CargoTrack.Business.Services.Pricing
{
    public class CargoPricingService(
        ICargoPriceRepository _priceRepository,
        IBranchRepository _branchRepository) : ICargoPricingService
    {
        public async Task<CargoPriceResultDto> CalculateAsync(
            double weight, double desi, DeliveryType deliveryType,
            Guid originBranchId, Guid destinationBranchId, bool hasInsurance)
        {
            var origin = await _branchRepository.GetByIdAsync(originBranchId);
            var destination = await _branchRepository.GetByIdAsync(destinationBranchId);

            if (origin is null || destination is null)
            {
                throw new ValidationException("Branch Not Found");
            }

            // Neden: Bölgeyi kullanıcıya sormuyoruz, şubelerin şehrine bakıp kendimiz buluyoruz.
            var zone = origin.CityId == destination.CityId ? PriceZone.SameCity : PriceZone.Domestic;

            var rule = await _priceRepository.GetActivePriceAsync(deliveryType, zone);
            if (rule is null)
            {
                throw new ValidationException("Bu teslimat tipi ve bölge için aktif fiyat kuralı bulunamadı.");
            }

            // Neden: Hacimli ama hafif kargo da yer kaplar. Desi ile ağırlıktan büyük olan esas alınır.
            decimal variablePart = desi >= weight
                ? (decimal)desi * rule.PricePerDesi
                : (decimal)weight * rule.PricePerKg;

            decimal subtotal = rule.BasePrice + variablePart;

            // Neden: Sigorta, ek hizmet olarak toplam tutarın belirli bir yüzdesidir.
            decimal insurance = hasInsurance ? subtotal * rule.InsuranceRate : 0;

            return new CargoPriceResultDto
            {
                Zone = zone,
                Price = Math.Round(subtotal + insurance, 2)
            };
        }
    }
}