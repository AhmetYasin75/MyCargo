using CargoTrack.DTO.DTOs.Cargos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Cargos
{
    public class UpdateCargoValidator : AbstractValidator<UpdateCargoDto>
    {
        public UpdateCargoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ReceiverId).NotEmpty().WithMessage("Alıcı seçilmelidir.");
            RuleFor(x => x.DestinationBranchId).NotEmpty().WithMessage("Varış şubesi seçilmelidir.");

            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Ağırlık 0'dan büyük olmalıdır.");
            RuleFor(x => x.Width).GreaterThan(0).WithMessage("En 0'dan büyük olmalıdır.");
            RuleFor(x => x.Length).GreaterThan(0).WithMessage("Boy 0'dan büyük olmalıdır.");
            RuleFor(x => x.Height).GreaterThan(0).WithMessage("Yükseklik 0'dan büyük olmalıdır.");

            RuleFor(x => x.CargoType).IsInEnum().WithMessage("Geçersiz kargo tipi.");
            RuleFor(x => x.DeliveryType).IsInEnum().WithMessage("Geçersiz teslimat tipi.");
        }
    }
}