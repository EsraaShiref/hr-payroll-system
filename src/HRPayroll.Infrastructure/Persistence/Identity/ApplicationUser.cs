using Microsoft.AspNetCore.Identity;

namespace HRPayroll.Infrastructure.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? EmployeeId { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
