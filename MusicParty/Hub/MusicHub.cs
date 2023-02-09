using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MusicParty.MusicApi;

namespace MusicParty.Hub;

[Authorize]
public class MusicHub : Microsoft.AspNetCore.SignalR.Hub
{
    private static HashSet<string> OnlineUsers { get; } = new();
    private static List<string> DuplicatedConnectionIds { get; } = new();
    private static Queue<(string name, string content)> Last5Chat { get; } = new();
    private readonly IEnumerable<IMusicApi> _musicApis;
    private readonly MusicBroadcaster _musicBroadcaster;
    private readonly UserManager _userManager;
    private readonly ILogger<MusicHub> _logger;

    public MusicHub(IEnumerable<IMusicApi> musicApis, MusicBroadcaster musicBroadcaster,
        UserManager userManager,
        ILogger<MusicHub> logger)
    {
        _musicApis = musicApis;
        _musicBroadcaster = musicBroadcaster;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (OnlineUsers.Contains(Context.User!.Identity!.Name!)) // don't allow login twice
        {
            DuplicatedConnectionIds.Add(Context.ConnectionId);
            await Clients.Caller.SendAsync("Abort", "You have already logged in.");
            Context.Abort();
            return;
        }

        OnlineUsers.Add(Context.User.Identity.Name!);
        await OnlineUserLogin(Clients.Others, Context.User.Identity.Name!);
        if (Last5Chat.Count > 0)
        {
            foreach (var chat in Last5Chat)
            {
                await NewChat(Clients.Caller, chat.name, chat.content);
            }
        }

        if (_musicBroadcaster.NowPlaying is not null)
        {
            var (music, enqueuerId) = _musicBroadcaster.NowPlaying.Value;
            await SetNowPlaying(Clients.Caller, music, _userManager.FindUserById(enqueuerId)!.Name,
                (int)(DateTime.Now - _musicBroadcaster.NowPlayingStartedTime).TotalSeconds);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (DuplicatedConnectionIds.Contains(Context.ConnectionId))
        {
            DuplicatedConnectionIds.Remove(Context.ConnectionId);
            return;
        }

        OnlineUsers.Remove(Context.User!.Identity!.Name!);
        await OnlineUserLogout(Context.User.Identity.Name!);
    }

    #region Remote invokable

    public async Task EnqueueMusic(string id, string apiName)
    {
        if (!_musicApis.TryGetMusicApi(apiName, out var ma))
            throw new HubException($"Unknown api provider {apiName}.");
        try
        {
            var music = await ma!.GetMusicByIdAsync(id);
            await _musicBroadcaster.EnqueueMusic(music, apiName, Context.User!.Identity!.Name!);
        }
        catch (Exception ex)
        {
            throw new HubException($"Failed to enqueue music, id: {id}", ex);
        }
    }

    public async Task RequestSetNowPlaying()
    {
        if (_musicBroadcaster.NowPlaying is null) return;
        var (music, enqueuerId) = _musicBroadcaster.NowPlaying.Value;
        await SetNowPlaying(Clients.Caller, music, _userManager.FindUserById(enqueuerId)!.Name,
            (int)(DateTime.Now - _musicBroadcaster.NowPlayingStartedTime).TotalSeconds);
    }

    public record MusicEnqueueOrder(string ActionId, Music Music, string EnqueuerName);

    public IEnumerable<MusicEnqueueOrder> GetMusicQueue()
    {
        return _musicBroadcaster.GetQueue().Select(x =>
            new MusicEnqueueOrder(x.ActionId, x.Music, _userManager.FindUserById(x.EnqueuerId)!.Name)).ToList();
    }

    public async Task NextSong()
    {
        await _musicBroadcaster.NextSong(Context.User!.Identity!.Name!);
    }

    public async Task TopSong(string actionId)
    {
        await _musicBroadcaster.TopSong(actionId, Context.User!.Identity!.Name!);
    }

    public async Task Rename(string newName)
    {
        _userManager.RenameUserById(Context.User!.Identity!.Name!, newName);
        await OnlineUserRename(Context.User.Identity.Name!);
    }

    public record User(string Id, string Name);

    public IEnumerable<User> GetOnlineUsers()
    {
        return OnlineUsers.Select(x => new User(x, _userManager.FindUserById(x)!.Name)).ToList();
    }

    public async Task ChatSay(string content)
    {
        var name = _userManager.FindUserById(Context.User!.Identity!.Name!)!.Name;
        while (Last5Chat.Count >= 5) Last5Chat.Dequeue();
        Last5Chat.Enqueue((name, content));
        await NewChat(Clients.All, name, content);
    }

    #endregion

    private async Task SetNowPlaying(IClientProxy target, PlayableMusic music, string enqueuerName, int playedTime)
    {
        await target.SendAsync(nameof(SetNowPlaying), music, enqueuerName, playedTime);
    }

    private async Task OnlineUserLogin(IClientProxy target, string id)
    {
        await target.SendAsync(nameof(OnlineUserLogin), id, _userManager.FindUserById(id)!.Name);
    }

    private async Task OnlineUserLogout(string id)
    {
        await Clients.All.SendAsync(nameof(OnlineUserLogout), id);
    }

    private async Task OnlineUserRename(string id)
    {
        await Clients.All.SendAsync(nameof(OnlineUserRename), id, _userManager.FindUserById(id)!.Name);
    }

    private async Task NewChat(IClientProxy target, string name, string content)
    {
        await target.SendAsync(nameof(NewChat), name, content);
    }
}