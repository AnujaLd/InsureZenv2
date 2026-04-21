using Microsoft.EntityFrameworkCore;
using InsureZenv2.src.Data;
using InsureZenv2.src.Models;

namespace InsureZenv2.src.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<bool> SaveChangesAsync();
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
