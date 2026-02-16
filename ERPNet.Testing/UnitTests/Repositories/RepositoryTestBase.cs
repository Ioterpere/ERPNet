using ERPNet.Infrastructure.Database.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Testing.UnitTests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected ERPNetDbContext Context { get; }

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ERPNetDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ERPNetDbContext(options);
        Context.Database.EnsureCreated();
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
