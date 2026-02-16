using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Validators;
using ERPNet.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Validation;

public class CreateEmpleadoRequestValidatorTests
{
    private readonly IEmpleadoRepository _repo = Substitute.For<IEmpleadoRepository>();
    private readonly CreateEmpleadoRequestValidator _sut;

    public CreateEmpleadoRequestValidatorTests()
    {
        _sut = new CreateEmpleadoRequestValidator(_repo);
    }

    [Fact(DisplayName = "Empleado: request valido pasa validacion")]
    public async Task RequestValido_PasaValidacion()
    {
        _repo.ExisteDniAsync("12345678Z").Returns(false);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García López",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empleado: nombre vacio falla")]
    public async Task NombreVacio_Falla()
    {
        var request = new CreateEmpleadoRequest
        {
            Nombre = "",
            Apellidos = "García",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact(DisplayName = "Empleado: apellidos vacios falla")]
    public async Task ApellidosVacios_Falla()
    {
        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Apellidos);
    }

    [Theory(DisplayName = "Empleado: DNI formato invalido falla")]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("ABCDEFGHZ")]
    [InlineData("123456789")]
    [InlineData("1234567ZZ")]
    public async Task DniFormatoInvalido_Falla(string dni)
    {
        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García",
            Dni = dni,
            SeccionId = 1
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Dni);
    }

    [Fact(DisplayName = "Empleado: DNI duplicado falla MustAsync")]
    public async Task DniDuplicado_FallaMustAsync()
    {
        _repo.ExisteDniAsync("12345678Z").Returns(true);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Dni)
            .WithErrorMessage("Ya existe un empleado con ese DNI.");
    }

    [Fact(DisplayName = "Empleado: SeccionId 0 falla")]
    public async Task SeccionIdCero_Falla()
    {
        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García",
            Dni = "12345678Z",
            SeccionId = 0
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.SeccionId);
    }

    [Fact(DisplayName = "Empleado: EncargadoId 0 falla")]
    public async Task EncargadoIdCero_Falla()
    {
        _repo.ExisteDniAsync("12345678Z").Returns(false);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García",
            Dni = "12345678Z",
            SeccionId = 1,
            EncargadoId = 0
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.EncargadoId);
    }

    [Fact(DisplayName = "Empleado: EncargadoId null no falla")]
    public async Task EncargadoIdNull_NoCausaError()
    {
        _repo.ExisteDniAsync("12345678Z").Returns(false);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Juan",
            Apellidos = "García",
            Dni = "12345678Z",
            SeccionId = 1,
            EncargadoId = null
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.EncargadoId);
    }
}

public class CreateMaquinariaRequestValidatorTests
{
    private readonly IMaquinariaRepository _repo = Substitute.For<IMaquinariaRepository>();
    private readonly CreateMaquinariaRequestValidator _sut;

    public CreateMaquinariaRequestValidatorTests()
    {
        _sut = new CreateMaquinariaRequestValidator(_repo);
    }

    [Fact(DisplayName = "Maquinaria: request valido pasa validacion")]
    public async Task RequestValido_PasaValidacion()
    {
        _repo.ExisteCodigoAsync("MAQ-001").Returns(false);

        var request = new CreateMaquinariaRequest
        {
            Nombre = "Torno CNC",
            Codigo = "MAQ-001"
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Maquinaria: nombre vacio falla")]
    public async Task NombreVacio_Falla()
    {
        var request = new CreateMaquinariaRequest
        {
            Nombre = "",
            Codigo = "MAQ-001"
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact(DisplayName = "Maquinaria: codigo vacio falla")]
    public async Task CodigoVacio_Falla()
    {
        var request = new CreateMaquinariaRequest
        {
            Nombre = "Torno",
            Codigo = ""
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }

    [Fact(DisplayName = "Maquinaria: codigo duplicado falla MustAsync")]
    public async Task CodigoDuplicado_FallaMustAsync()
    {
        _repo.ExisteCodigoAsync("MAQ-001").Returns(true);

        var request = new CreateMaquinariaRequest
        {
            Nombre = "Torno",
            Codigo = "MAQ-001"
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Codigo)
            .WithErrorMessage("Ya existe una máquina con ese código.");
    }

    [Fact(DisplayName = "Maquinaria: ubicacion larga falla")]
    public async Task UbicacionLarga_Falla()
    {
        _repo.ExisteCodigoAsync("MAQ-001").Returns(false);

        var request = new CreateMaquinariaRequest
        {
            Nombre = "Torno",
            Codigo = "MAQ-001",
            Ubicacion = new string('A', 201)
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Ubicacion);
    }

    [Fact(DisplayName = "Maquinaria: SeccionId 0 falla")]
    public async Task SeccionIdCero_Falla()
    {
        _repo.ExisteCodigoAsync("MAQ-001").Returns(false);

        var request = new CreateMaquinariaRequest
        {
            Nombre = "Torno",
            Codigo = "MAQ-001",
            SeccionId = 0
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.SeccionId);
    }
}

public class CreateRolRequestValidatorTests
{
    private readonly IRolRepository _repo = Substitute.For<IRolRepository>();
    private readonly CreateRolRequestValidator _sut;

    public CreateRolRequestValidatorTests()
    {
        _sut = new CreateRolRequestValidator(_repo);
    }

    [Fact(DisplayName = "Rol: request valido pasa validacion")]
    public async Task RequestValido_PasaValidacion()
    {
        _repo.ExisteNombreAsync("Admin").Returns(false);

        var request = new CreateRolRequest { Nombre = "Admin" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Rol: nombre vacio falla")]
    public async Task NombreVacio_Falla()
    {
        var request = new CreateRolRequest { Nombre = "" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact(DisplayName = "Rol: nombre duplicado falla MustAsync")]
    public async Task NombreDuplicado_FallaMustAsync()
    {
        _repo.ExisteNombreAsync("Admin").Returns(true);

        var request = new CreateRolRequest { Nombre = "Admin" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("Ya existe un rol con ese nombre.");
    }

    [Fact(DisplayName = "Rol: nombre largo falla")]
    public async Task NombreLargo_Falla()
    {
        var request = new CreateRolRequest { Nombre = new string('A', 101) };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact(DisplayName = "Rol: descripcion larga falla")]
    public async Task DescripcionLarga_Falla()
    {
        _repo.ExisteNombreAsync("Admin").Returns(false);

        var request = new CreateRolRequest
        {
            Nombre = "Admin",
            Descripcion = new string('A', 501)
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact(DisplayName = "Rol: descripcion null no falla")]
    public async Task DescripcionNull_NoCausaError()
    {
        _repo.ExisteNombreAsync("Admin").Returns(false);

        var request = new CreateRolRequest { Nombre = "Admin", Descripcion = null };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Descripcion);
    }
}
