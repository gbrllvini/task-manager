using TaskManager.Application.Auth;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Application.Services.Interfaces;

public interface ITokenService {
    AccessTokenResult CreateAccessToken(ApplicationUser user);
}
