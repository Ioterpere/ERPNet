using FluentValidation;

namespace ERPNet.Application.Auth.DTOs.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio.");
    }
}
