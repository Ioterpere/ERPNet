using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface ILogRepository
{
    void Add(Log log);
    Task<List<Log>> GetFilteredAsync(LogFilter request);
}
