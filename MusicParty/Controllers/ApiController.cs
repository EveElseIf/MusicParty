using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MusicParty.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly NeteaseApi _neteaseApi;
    private readonly UserManager _userManager;
    private readonly ILogger<ApiController> _logger;

    public ApiController(NeteaseApi neteaseApi, UserManager userManager, ILogger<ApiController> logger)
    {
        _neteaseApi = neteaseApi;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet, Route("Test")]
    public IActionResult Test()
    {
        return Ok(new { Test = "Hello World" });
    }

    [HttpGet, Route("New")]
    public async Task<IActionResult> New()
    {
        var id = HttpContext.User.Identity.Name;
        if (string.IsNullOrEmpty(id))
        {
            var id2 = Guid.NewGuid().ToString()[..8];
            await _userManager.LoginAsync(id2);
        }

        if (!string.IsNullOrEmpty(id) && _userManager.FindUserById(id) is null)
        {
            await _userManager.LogoutAsync(id);
            var id2 = Guid.NewGuid().ToString()[..8];
            await _userManager.LoginAsync(id2);
        }

        return Ok();
    }

    [HttpGet, Route("Profile"), Authorize]
    public async Task<IActionResult> Profile()
    {
        var name = _userManager.FindUserById(HttpContext.User.Identity.Name)?.Name;
        return Ok(new { Name = name });
    }

    [HttpGet, Route("Rename/{name}"), Authorize]
    public async Task<IActionResult> Rename(string name)
    {
        _userManager.RenameUserById(HttpContext.User.Identity.Name, name);
        return Ok();
    }

    [HttpGet, Route("SearchUser/{keyword}"), Authorize]
    public async Task<IActionResult> SearchUser(string keyword)
    {
        return Ok(await _neteaseApi.SearchNeteaseUsersAsync(keyword));
    }

    [HttpGet, Route("bind/{uid}"), Authorize]
    public async Task<IActionResult> Bind(string uid)
    {
        _userManager.UserBindNeteaseUid(HttpContext.User.Identity.Name, uid);
        return Ok();
    }

    [HttpGet, Route("myplaylists"), Authorize]
    public async Task<IActionResult> MyPlaylists()
    {
        if (_userManager.FindUserById(HttpContext.User.Identity.Name) is null ||
            string.IsNullOrEmpty(_userManager.FindUserById(HttpContext.User.Identity.Name).NeteaseUid))
        {
            return BadRequest("You should bind your Netease Account first.");
        }

        var playlists =
            await _neteaseApi.GetUserPlaylistAsync(_userManager.FindUserById(HttpContext.User.Identity.Name)
                .NeteaseUid);
        return Ok(playlists);
    }

    [HttpGet, Route("playlistmusics/{id}"), Authorize]
    public async Task<IActionResult> PlaylistMusics(string id, [FromQuery] int page = 0)
    {
        var musics = await _neteaseApi.GetMusicsByPlaylistAsync(id, page * 10);
        return Ok(musics);
    }
}