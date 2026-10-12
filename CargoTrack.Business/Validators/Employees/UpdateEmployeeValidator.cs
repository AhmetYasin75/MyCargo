using CargoTrack.DTO.DTOs.EmployeeDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Employees
{
    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Personel seçilmelidir.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Ünvan girilmelidir.")
                .MaximumLength(100).WithMessage("Ünvan en fazla 100 karakter olabilir.");

            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Şube seçilmelidir.");
        }
    }
}