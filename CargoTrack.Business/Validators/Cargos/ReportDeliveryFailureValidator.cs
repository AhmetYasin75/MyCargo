using CargoTrack.DTO.DTOs.Cargos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Cargos
{
    public class ReportDeliveryFailureValidator : AbstractValidator<ReportDeliveryFailureDto>
    {
        public ReportDeliveryFailureValidator()
        {
            RuleFor(x => x.CargoId).NotEmpty().WithMessage("Kargo seçilmelidir.");
            RuleFor(x => x.Reason).IsInEnum().WithMessage("Geçersiz başarısızlık nedeni.");

            // Neden: Case "gerekçe ile izlenir" diyor, açıklama boş bırakılamaz.
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama girilmelidir.")
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            // Not: PerformedByUserId kuralı kaldırıldı. İşlemi yapanı servis belirler.
        }
    }
}