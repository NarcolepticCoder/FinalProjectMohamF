using System.Net.Http.Headers;

public class ApiTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private readonly LocalTokenService _tokenService;


    public ApiTokenHandler(IHttpContextAccessor httpContextAccessor, LocalTokenService tokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
    

    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var token = _tokenService.CreateApiToken(user);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
        }

        return await base.SendAsync(request, cancellationToken);
    }


}
