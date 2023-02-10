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

    [HttpGet, Route("Profile"), Authorize]
    public IActionResult Profile()
    {
        var name = _userManager.FindUserById(HttpContext.User.Identity!.Name!)!.Name;
        return Ok(new { Name = name });
    }

    [HttpGet, Route("Rename/{newName}"), Authorize]
    public IActionResult Rename(string newName)
    {
        _userManager.RenameUserById(HttpContext.User.Identity!.Name!, newName);
        return Ok();
    }

    [HttpGet, Route("{apiName}/SearchUser/{keyword}"), Authorize]
    public async Task<IActionResult> SearchUser(string apiName, string keyword)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider {apiName}.".BuildResponseMessageWithCode(1));
        return Ok(await ma!.SearchUserAsync(keyword));
    }

    [HttpGet, Route("{apiName}/bind/{identifier}"), Authorize]
    public IActionResult Bind(string apiName, string identifier)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out _))
            return BadRequest($"Unknown api provider {apiName}.".BuildResponseMessageWithCode(1));
        _userManager.BindMusicApiService(HttpContext.User.Identity!.Name!, apiName, identifier);
        return Ok();
    }

    [HttpGet, Route("bindinfo"), Authorize]
    public IActionResult BindInfo()
    {
        var user = _userManager.FindUserById(HttpContext.User.Identity!.Name!)!;
        return Ok(user.MusicApiServiceBindings.ToArray());
    }

    [HttpGet, Route("{apiName}/myplaylists"), Authorize]
    public async Task<IActionResult> MyPlaylists(string apiName)
    {
        var user = _userManager.FindUserById(HttpContext.User.Identity!.Name!)!;
        if (!user.TryGetMusicApiServiceBinding(apiName, out var identifier))
            return BadRequest($"You should bind your {apiName} Account first.".BuildResponseMessageWithCode(2));

        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider {apiName}.".BuildResponseMessageWithCode(1));

        var playlists = await ma!.GetUserPlayListAsync(identifier!);

        return Ok(playlists);
    }

    [HttpGet, Route("{apiName}/playlistmusics/{id}"), Authorize]
    public async Task<IActionResult> PlaylistMusics(string apiName, string id, [FromQuery] int page = 1)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider {apiName}.".BuildResponseMessageWithCode(1));
        if (string.IsNullOrEmpty(id))
            return BadRequest("Id cannot be null".BuildResponseMessageWithCode(3));
        if (page <= 0)
            return BadRequest("You need to set page with a number in N*.".BuildResponseMessageWithCode(4));
        var musics = await ma!.GetMusicsByPlaylistAsync(id, (page - 1) * 10);
        return Ok(musics);
    }

    [HttpPost, Route("setcredential/{apiName}")]
    public async Task<IActionResult> SetCred(string apiName, [FromBody] string? cred)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            return BadRequest($"Unknown api provider {apiName}.".BuildResponseMessageWithCode(1));
        if (string.IsNullOrEmpty(cred))
            return BadRequest("You need to set your credential in request body.".BuildResponseMessageWithCode(3));
        if (await ma!.TrySetCredentialAsync(cred))
            return Ok();
        else
            return
                StatusCode(502, $"Check your credential.".BuildResponseMessageWithCode(4));
    }
}