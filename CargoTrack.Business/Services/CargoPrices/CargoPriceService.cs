using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.CargoPrices;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using System.ComponentModel.DataAnnotations;

namespace CargoTrack.Business.Services.CargoPrices
{
    // Neden: Fiyat kuralı sayıları Admin tarafından değiştirilebilir. Yetki kontrolü servis içinde.
    // Ekleme/silme/pasif yapma bilinçli olarak yok: her tip+bölge için tek aktif kural olmalı,
    // yoksa CargoPricingService fiyat hesaplayamaz.
    public class CargoPriceService(
        ICargoPriceRepository _repository,
        ICargoAccessService _accessService) : ICargoPriceService
    {
        public async Task<List<ResultCargoPriceDto>> GetAllAsync()
        {
            _accessService.EnsureAdmin();

            var rules = await _repository.GetAllAsync();

            return rules
                .OrderBy(r => r.DeliveryType)
                .ThenBy(r => r.Zone)
                .Select(ToResultDto)
                .ToList();
        }

        public async Task<UpdateCargoPriceDto> GetByIdAsync(Guid id)
        {
            _accessService.EnsureAdmin();

            var rule = await _repository.GetByIdAsync(id);
            if (rule is null)
            {
                throw new ValidationException("Fiyat kuralı bulunamadı.");
            }

            var dto = rule.Adapt<UpdateCargoPriceDto>();

            // Neden: Veritabanında 0.02, formda 2 (yüzde) gösterilir.
            dto.InsuranceRatePercent = rule.InsuranceRate * 100;

            return dto;
        }

        public async Task UpdateAsync(UpdateCargoPriceDto dto)
        {
            _accessService.EnsureAdmin();

            // Neden: Yeni nesne üretmiyoruz, mevcut kuralı çekip sadece sayıları değiştiriyoruz.
            // Teslimat tipi, bölge ve aktiflik formdan gelse bile değiştirilmez (kural kimliği sabit).
            var rule = await _repository.GetByIdAsync(dto.Id);
            if (rule is null)
            {
                throw new ValidationException("Fiyat kuralı bulunamadı.");
            }

            rule.BasePrice = dto.BasePrice!.Value;
            rule.PricePerDesi = dto.PricePerDesi!.Value;
            rule.PricePerKg = dto.PricePerKg!.Value;

            // Neden: Formda yüzde (2) girilir, veritabanında oran (0.02) saklanır.
            rule.InsuranceRate = dto.InsuranceRatePercent!.Value / 100m;

            await _repository.UpdateAsync(rule);
        }

        private static ResultCargoPriceDto ToResultDto(CargoPrice rule)
        {
            var dto = rule.Adapt<ResultCargoPriceDto>();
            dto.InsuranceRatePercent = rule.InsuranceRate * 100;
            return dto;
        }
    }
}