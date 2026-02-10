using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface ILogIntentoLoginRepository
{
    Task AddAsync(LogIntentoLogin log);
    Task<int> CountRecentFailedByEmailAsync(string email, DateTime desde);
    Task<int> CountRecentFailedByIpAsync(string ip, DateTime desde);
    Task SaveChangesAsync();
}
