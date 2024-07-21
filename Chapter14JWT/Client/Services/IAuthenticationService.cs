using Chapter14JWT.Client.Models;
using System.Security.Claims;

namespace Chapter14JWT.Client.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="userForRegistration">The user for registration.</param>
    /// <returns></returns>
    Task<RegistrationResponseDto> RegisterUser(UserForRegistrationDto userForRegistration);
    /// <summary>
    /// Logins the specified user for authentication.
    /// </summary>
    /// <param name="userForAuthentication">The user for authentication.</param>
    /// <returns></returns>
    Task<AuthenticationResponseDto> Login(UserForAuthenticationDto userForAuthentication);
    /// <summary>
    /// Logouts this instance.
    /// </summary>
    /// <returns></returns>
    Task Logout();
    /// <summary>
    /// Refreshes the token.
    /// </summary>
    /// <returns></returns>
    Task<string> RefreshToken();
    /// <summary>
    /// Parses the claims from JWT.
    /// </summary>
    /// <param name="jwt">The JWT.</param>
    /// <returns></returns>
    IEnumerable<Claim> ParseClaimsFromJwt(string jwt);
}