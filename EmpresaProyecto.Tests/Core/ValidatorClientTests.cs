using EmpresaProyecto.Core.Entities;
using FluentValidation.TestHelper;

namespace EmpresaProyecto.Tests.Core
{
    public class ValidatorClientTests
    {
        private readonly ValidatorClient _validator;

        public ValidatorClientTests()
        {
            _validator = new ValidatorClient();
        }

        [Fact]
        public void Should_Have_Error_When_Nombre_Is_Empty()
        {
            var model = new Cliente
            {
                Nombre = "",
                ApellidoPaterno = "Pérez",
                Correo = "correo@test.com",
                Telefono = "5551234567"
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Nombre);
        }

        [Fact]
        public void Should_Have_Error_When_Correo_Is_Invalid()
        {
            var model = new Cliente
            {
                Nombre = "Juan",
                ApellidoPaterno = "Pérez",
                Correo = "correo_invalido",
                Telefono = "5551234567"
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Correo);
        }

        [Fact]
        public void Should_Have_Error_When_Telefono_Is_TooShort()
        {
            var model = new Cliente
            {
                Nombre = "Juan",
                ApellidoPaterno = "Pérez",
                Correo = "juan@test.com",
                Telefono = "12345"
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Telefono);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new Cliente
            {
                Nombre = "Juan",
                ApellidoPaterno = "Pérez",
                ApellidoMaterno = "López",
                Correo = "juan@test.com",
                Telefono = "5551234567"
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}