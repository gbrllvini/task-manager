using Microsoft.AspNetCore.Identity;

namespace TaskManager.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid> {
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
