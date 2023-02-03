using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicParty.MusicApi;

namespace MusicParty.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly IEnumerable<IMusicApi> _musicApis;
    private readonly UserManager _userManager;
    private readonly ILogger<ApiController> _logger;

    public ApiController(IEnumerable<IMusicApi> musicApis, UserManager userManager, ILogger<ApiController> logger)
    {
        _musicApis = musicApis;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet, Route("MusicServices")]
    public IActionResult MusicServices() => Ok(_musicApis.Select(a => a.ServiceName).ToList());

    [HttpGet, Route("New")]
    public async Task<IActionResult> New()
    {
        var id = HttpContext.User.Identity!.Name;
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

        return Ok();
    }

    [HttpGet, Route("Profile"), Authorize]
    public IActionResult Profile()
    {
        var name = _userManager.FindUserById(HttpContext.User.Identity!.Name!)!.Name;
        return Ok(new { Name = name });
    }

    [HttpGet, Route("Rename/{name}"), Authorize]
    public IActionResult Rename(string name)
    {
        _userManager.RenameUserById(HttpContext.User.Identity!.Name!, name);
        return Ok();
    }

    [HttpGet, Route("{apiName}/SearchUser/{keyword}"), Authorize]
    public async Task<IActionResult> SearchUser(string apiName, string keyword)
    {
        if (string.IsNullOrEmpty(apiName)) return BadRequest("Specify an api provider.");
        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider ${apiName}.");
        return Ok(await ma!.SearchUserAsync(keyword));
    }

    [HttpGet, Route("{apiName}/bind/{identifier}"), Authorize]
    public IActionResult Bind(string apiName, string identifier)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out _))
            return BadRequest($"Unknown api provider ${apiName}.");
        _userManager.BindMusicApiService(HttpContext.User.Identity!.Name!, apiName, identifier);
        return Ok();
    }

    [HttpGet, Route("{apiName}/myplaylists"), Authorize]
    public async Task<IActionResult> MyPlaylists(string apiName)
    {
        var user = _userManager.FindUserById(HttpContext.User.Identity!.Name!)!;
        if (!user.TryGetMusicApiServiceBinding(apiName, out var identifier))
            return BadRequest($"You should bind your {apiName} Account first.");

        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider ${apiName}.");

        var playlists = await ma!.GetUserPlayListAsync(identifier!);

        return Ok(playlists);
    }

    [HttpGet, Route("{api}/playlistmusics/{id}"), Authorize]
    public async Task<IActionResult> PlaylistMusics(string api, string id, [FromQuery] int page = 0)
    {
        if (!_musicApis.TryGetMusicApi(api, out var ma))
            return BadRequest($"Unknown api provider ${api}.");
        var musics = await ma!.GetMusicsByPlaylistAsync(id, page * 10);
        return Ok(musics);
    }
}