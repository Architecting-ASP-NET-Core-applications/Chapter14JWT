using Chapter14JWT.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.JSInterop;

namespace Chapter14JWT.Client.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient client;
    private readonly JsonSerializerOptions options;
    private readonly AuthenticationStateProviderClient authStateProvider;
    private readonly IJSRuntime js;

    public AuthenticationService(HttpClient client
        , AuthenticationStateProvider authStateProvider
        , IJSRuntime js)
    {
        this.client = client;
        options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        this.authStateProvider = (AuthenticationStateProviderClient)authStateProvider;
        this.js = js;
    }

    public async Task<AuthenticationResponseDto> Login(UserForAuthenticationDto userForAuthentication)
    {
        var authContent = string.Empty;
        AuthenticationResponseDto result;
        var ret = new AuthenticationResponseDto();

        var content = JsonSerializer.Serialize(userForAuthentication);
        var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

        JsonContent content2 = JsonContent.Create(new UserForAuthenticationDto() { Username = $"{userForAuthentication.Username}", Password = $"{userForAuthentication.Password}" });

        var authResult = await client.PostAsync("api/v1/login", content2);

        if (authResult.StatusCode is not System.Net.HttpStatusCode.OK)
        {
            ret = new AuthenticationResponseDto()
            {
                StatusCode = authResult.StatusCode
            };
        }
        else
        {
            try
            {
                authContent = await authResult.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<AuthenticationResponseDto>(authContent, options);

                if (!authResult.IsSuccessStatusCode)
                    return result;

                //await localStorage.SetItemAsync("authToken", result.Token);
                //await localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                authStateProvider.NotifyUserAuthentication(result.Token);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

                // Here parse the JWT from machine
                var claims = JwtParser.ParseClaimsFromJwt(result.Token);

                ret = new AuthenticationResponseDto
                {
                    IsAuthenticationSuccessful = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (message != null) { }
            }
        }
        return ret;
    }

    public async Task Logout()
    {
        // Call logout
        var authContent = string.Empty;
        var bodyContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var authResult = await client.PostAsync("account/logout", bodyContent);

        await js.InvokeVoidAsync("removeToken", "authToken");
        await js.InvokeVoidAsync("removeToken", "refreshToken");
        authStateProvider.NotifyUserLogout();
        client.DefaultRequestHeaders.Authorization = null;
    }
    
    public async Task<string> RefreshToken()
    {        
        var token = await js.InvokeAsync<string> ("authToken");
        var refreshToken = await js.InvokeAsync<string>("refreshToken");

        var tokenDto = JsonSerializer.Serialize(new RefreshToken { Token = token, RefreshActualToken = refreshToken });
        var bodyContent = new StringContent(tokenDto, Encoding.UTF8, "application/json");

        var refreshResult = await client.PostAsync("token/refresh", bodyContent);
        var refreshContent = await refreshResult.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthenticationResponseDto>(refreshContent, options);

        if (!refreshResult.IsSuccessStatusCode)
        { throw new AuthenticationException("Something went wrong during the refresh token action"); }

        await js.InvokeVoidAsync("authToken", result.Token);
        await js.InvokeVoidAsync("refreshToken", result.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
        return result.Token;
    }
    
    public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var claimsJson = string.Empty;
        foreach (var item in jsonBytes)
        { claimsJson += (char)item; }

        var keyValuePairs = JsonSerializer.Deserialize<JwtContent>(claimsJson);
        if (keyValuePairs != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, keyValuePairs.Role));
            if (keyValuePairs.Inhabitant)
                claims.Add(new Claim(ClaimTypes.Role, nameof(keyValuePairs.Inhabitant)));
            if (keyValuePairs.Adjuster)
                claims.Add(new Claim(ClaimTypes.Role, nameof(keyValuePairs.Adjuster)));
            if (keyValuePairs.Calibrator)
                claims.Add(new Claim(ClaimTypes.Role, nameof(keyValuePairs.Calibrator)));
            if (keyValuePairs.Manufacturer)
                claims.Add(new Claim(ClaimTypes.Role, nameof(keyValuePairs.Manufacturer)));
            if (keyValuePairs.Manufacturer)
                claims.Add(new Claim(ClaimTypes.Role, nameof(keyValuePairs.Manufacturer)));
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            default:
                break;
        }
        return Convert.FromBase64String(base64);
    }

    public Task<RegistrationResponseDto> RegisterUser(UserForRegistrationDto userForRegistration)
    {
        throw new NotImplementedException();
    }

}