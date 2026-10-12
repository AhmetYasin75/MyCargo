using CargoTrack.DTO.DTOs.Cargos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Cargos
{
    public class ChangeCargoStatusValidator : AbstractValidator<ChangeCargoStatusDto>
    {
        public ChangeCargoStatusValidator()
        {
            RuleFor(x => x.CargoId).NotEmpty().WithMessage("Kargo seçilmelidir.");
            RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Geçersiz kargo durumu.");

            // Neden: Hareket geçmişinde "neden değişti" görünsün. Hareket kaydında açıklama boş olamaz.
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama girilmelidir.")
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            // Not: PerformedByUserId kuralı kaldırıldı. Bu alan formdan gelmez, servis giriş yapan kullanıcıdan doldurur.
        }
    }
}