namespace MusicParty;

public class PreprocessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UserManager _userManager;

    public PreprocessMiddleware(RequestDelegate next, UserManager userManager)
    {
        _next = next;
        _userManager = userManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ensure user login with a valid id.
        var id = context.User.Identity!.Name;
        if (string.IsNullOrEmpty(id))
        {
            var newId = Guid.NewGuid().ToString()[..8];
            await _userManager.LoginAsync(newId);
        }

        if (!string.IsNullOrEmpty(id) && _userManager.FindUserById(id) is null)
        {
            await _userManager.LogoutAsync(id);
            var newId = Guid.NewGuid().ToString()[..8];
            await _userManager.LoginAsync(newId);
        }

        await _next(context);
    }
}

public static class PreprocessMiddlewareExtension
{
    public static IApplicationBuilder UsePreprocess(this IApplicationBuilder builder) =>
        builder.UseMiddleware<PreprocessMiddleware>();
}