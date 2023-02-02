using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MusicParty.Models;

namespace MusicParty;

public class UserManager
{
    private readonly IHttpContextAccessor _accessor;
    private readonly List<User> _users = new();

    public UserManager(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private void CreateUser(string id, string name)
    {
        _users.Add(new User(id, id, null));
    }

    public async Task LoginAsync(string id)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, id)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "any"));
        await _accessor.HttpContext.SignInAsync("Cookies", user, new AuthenticationProperties()
        {
            ExpiresUtc = DateTimeOffset.MaxValue
        });
        CreateUser(id, id);
    }

    public async Task LogoutAsync(string id)
    {
        await _accessor.HttpContext.SignOutAsync("Cookies");
        _users.RemoveAll(x => x.Id == id);
    }

    public User? FindUserById(string id)
    {
        return _users.Find(x => x.Id == id);
    }

    public void RenameUserById(string id, string newName)
    {
        var user = _users.Find(x => x.Id == id);
        _users.Remove(user);
        _users.Add(user with { Name = newName });
    }

    public void UserBindNeteaseUid(string id, string uid)
    {
        var user = _users.Find(x => x.Id == id);
        _users.Remove(user);
        _users.Add(user with { NeteaseUid = uid });
    }
}