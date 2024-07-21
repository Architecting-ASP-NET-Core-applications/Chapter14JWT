namespace Chapter14JWT.Client.Models;

public interface IAuthService
{
    Task<bool> Login(UserLogin userLogin);
    Task Logout();
}