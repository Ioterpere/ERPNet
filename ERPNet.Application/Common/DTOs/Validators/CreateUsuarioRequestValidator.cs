using ERPNet.Domain.Repositories;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateUsuarioRequestValidator : AbstractValidator<CreateUsuarioRequest>
{
    public CreateUsuarioRequestValidator(IUsuarioRepository usuarioRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .MaximumLength(256).WithMessage("El email no puede tener más de 256 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .MustAsync(async (email, ct) => !await usuarioRepository.ExisteEmailAsync(email))
                .WithMessage("Ya existe un usuario con ese email.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un dígito.");

        RuleFor(x => x.EmpleadoId)
            .GreaterThan(0).WithMessage("El EmpleadoId debe ser mayor que 0.");
    }
}
