using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ERPNet.Testing.UnitTests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected ERPNetDbContext Context { get; }
    protected ICurrentUserProvider CurrentUser { get; }

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ERPNetDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ERPNetDbContext(options);
        Context.Database.EnsureCreated();

        CurrentUser = Substitute.For<ICurrentUserProvider>();
        CurrentUser.Current.Returns(new UsuarioContext { Email = "test@test.com", EmpresaId = 1 });

        Context.Empresas.Add(new Empresa { Id = 1, Nombre = "Empresa Test", Activo = true });
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    protected async Task SaveAndClearAsync()
    {
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
