using CargoTrack.DTO.DTOs.TransferCenterDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.TransferCenters
{
    public class UpdateTransferCenterValidator : AbstractValidator<UpdateTransferCenterDto>
    {
        public UpdateTransferCenterValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Transfer merkezi seçilmelidir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Merkez adı girilmelidir.")
                .MaximumLength(100).WithMessage("Merkez adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("Şehir seçilmelidir.");
        }
    }
}