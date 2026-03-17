using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class LogIntentoLoginRepository(ERPNetDbContext context) : ILogIntentoLoginRepository
{
    public async Task AddAsync(LogIntentoLogin log)
    {
        await context.IntentosLogin.AddAsync(log);
    }

    public async Task<int> CountRecentFailedByEmailAsync(string email, DateTime desde)
    {
        return await context.IntentosLogin
            .CountAsync(l => l.NombreUsuario == email && !l.Exitoso && l.FechaIntento >= desde);
    }

    public async Task<int> CountRecentFailedByIpAsync(string ip, DateTime desde)
    {
        return await context.IntentosLogin
            .CountAsync(l => l.DireccionIp == ip && !l.Exitoso && l.FechaIntento >= desde);
    }

}
