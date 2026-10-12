using CargoTrack.DTO.DTOs.Cargos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Cargos
{
    public class VerifyDeliveryValidator : AbstractValidator<VerifyDeliveryDto>
    {
        public VerifyDeliveryValidator()
        {
            RuleFor(x => x.CargoId).NotEmpty().WithMessage("Kargo seçilmelidir.");

            RuleFor(x => x.DeliveryCode)
                .NotEmpty().WithMessage("Teslimat kodu girilmelidir.")
                .Matches(@"^\d{6}$").WithMessage("Teslimat kodu 6 haneli bir sayı olmalıdır.");

            RuleFor(x => x.ReceiverName)
                .NotEmpty().WithMessage("Teslim alan kişi girilmelidir.")
                .MaximumLength(100).WithMessage("Teslim alan kişi adı en fazla 100 karakter olabilir.");

            // Not: PerformedByUserId kuralı kaldırıldı. Teslim eden personeli servis belirler.
        }
    }
}