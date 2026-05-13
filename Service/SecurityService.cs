using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;

namespace Sandoohouse.Service;

public class SecurityService
{
    private readonly ApplicationDbContext _context;

    public SecurityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleIpFail(LoginAttempt? ipRecord, string ip)
    {
        if (ipRecord == null)
        {
            ipRecord = new LoginAttempt
            {
                IPAddress = ip,
                AttemptCount = 1
            };
            _context.LoginAttempts.Add(ipRecord);
        }
        else
        {
            ipRecord.AttemptCount++;

            if (ipRecord.AttemptCount >= 5)
            {
                ipRecord.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                ipRecord.AttemptCount = 0;
            }

            _context.LoginAttempts.Update(ipRecord);
        }

        await _context.SaveChangesAsync();
    }
}