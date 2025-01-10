using FluentValidation;
using Quokka.Models;

public class UsuarioValidator : AbstractValidator<Usuario>
{
    public UsuarioValidator()
    {
        RuleFor(u => u.Correo).NotEmpty().EmailAddress().WithMessage("Email inválido");
        RuleFor(u => u.Contrasena).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
    }
}
