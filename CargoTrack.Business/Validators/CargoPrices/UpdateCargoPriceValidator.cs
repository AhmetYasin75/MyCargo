using CargoTrack.DTO.DTOs.CargoPriceDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.CargoPrices
{
    public class UpdateCargoPriceValidator : AbstractValidator<UpdateCargoPriceDto>
    {
        // Neden: Fiyatlar çok büyük girilip kargo ücretlerini bozmasın diye üst sınır var.
        private const decimal MaxPrice = 100000m;

        public UpdateCargoPriceValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Fiyat kuralı seçilmelidir.");

            RuleFor(x => x.BasePrice)
                .NotNull().WithMessage("Temel ücret girilmelidir.")
                .GreaterThanOrEqualTo(0m).WithMessage("Temel ücret negatif olamaz.")
                .LessThanOrEqualTo(MaxPrice).WithMessage($"Temel ücret en fazla {MaxPrice:N0} olabilir.")
                .PrecisionScale(18, 2, false).WithMessage("Temel ücret en fazla 2 ondalık basamaklı olmalıdır.");

            RuleFor(x => x.PricePerDesi)
                .NotNull().WithMessage("Desi başına ücret girilmelidir.")
                .GreaterThanOrEqualTo(0m).WithMessage("Desi başına ücret negatif olamaz.")
                .LessThanOrEqualTo(MaxPrice).WithMessage($"Desi başına ücret en fazla {MaxPrice:N0} olabilir.")
                .PrecisionScale(18, 2, false).WithMessage("Desi başına ücret en fazla 2 ondalık basamaklı olmalıdır.");

            RuleFor(x => x.PricePerKg)
                .NotNull().WithMessage("Kg başına ücret girilmelidir.")
                .GreaterThanOrEqualTo(0m).WithMessage("Kg başına ücret negatif olamaz.")
                .LessThanOrEqualTo(MaxPrice).WithMessage($"Kg başına ücret en fazla {MaxPrice:N0} olabilir.")
                .PrecisionScale(18, 2, false).WithMessage("Kg başına ücret en fazla 2 ondalık basamaklı olmalıdır.");

            // Neden: Veritabanında oran 2 ondalıkla saklanıyor (0.02). Yüzdeyi tam sayı istiyoruz,
            // yoksa %2.5 sessizce %3'e yuvarlanırdı.
            RuleFor(x => x.InsuranceRatePercent)
                .NotNull().WithMessage("Sigorta oranı girilmelidir.")
                .InclusiveBetween(0m, 100m).WithMessage("Sigorta oranı 0 ile 100 arasında olmalıdır.")
                .Must(p => p is null || p.Value == Math.Truncate(p.Value))
                .WithMessage("Sigorta oranı tam sayı olmalıdır (örn. 2).");
        }
    }
}