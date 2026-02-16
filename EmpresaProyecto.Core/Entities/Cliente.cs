using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpresaProyecto.Core.Entities
{
    public class Cliente
    {
        [Key]
        public string IdCliente { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Nombre { get; set; }

        [Required]
        [MaxLength(200)]
        public required string ApellidoPaterno { get; set; }

        [MaxLength(200)]
        public string? ApellidoMaterno { get; set; }

        [MaxLength(500)]
        public required string Correo { get; set; }

        [Required]
        [MaxLength(15)]
        public required string Telefono { get; set; }
    }

    public class ValidatorClient : AbstractValidator<Cliente>
    {
        public ValidatorClient()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres");

            RuleFor(x => x.ApellidoPaterno)
                .NotEmpty().WithMessage("El apellido paterno es obligatorio")
                .MinimumLength(3).WithMessage("El apellido paterno debe tener al menos 2 caracteres")
                .MaximumLength(200).WithMessage("El apellido paterno no puede superar los 200 caracteres");
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es obligatorio")
                .EmailAddress().WithMessage("El correo debe tener un formato válido")
                .MaximumLength(500).WithMessage("El correo no puede superar los 500 caracteres");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es obligatorio")
                //.Matches(@"^\d+$").WithMessage("El teléfono solo debe contener números")
                .MinimumLength(10).WithMessage("El teléfono debe tener al menos 10 dígitos")
                .MaximumLength(15).WithMessage("El teléfono no puede superar los 15 dígitos");

        }

    }

}
