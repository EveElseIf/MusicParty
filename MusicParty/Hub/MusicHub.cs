using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MusicParty.Models;

namespace MusicParty.Hub;

[Authorize]
public class MusicHub : Microsoft.AspNetCore.SignalR.Hub
{
    private static (PlayableMusic, string enqueuerId)? NowPlaying { get; set; }
    private static Queue<(Music, string enqueuerId)> MusicQueue { get; } = new();
    private static HashSet<string> OnlineUsers { get; } = new();
    private static Queue<(string name, string content)> Last5Chat { get; } = new();
    private static DateTime _time = default;
    private static bool _loopStarted = false;
    private readonly NeteaseApi _neteaseApi;
    private readonly IHubContext<MusicHub> _context;
    private readonly UserManager _userManager;
    private readonly ILogger<MusicHub> _logger;

    public MusicHub(NeteaseApi neteaseApi, IHubContext<MusicHub> context, UserManager userManager,
        ILogger<MusicHub> logger)
    {
        _neteaseApi = neteaseApi;
        _context = context;
        _userManager = userManager;
        _logger = logger;
        if (!_loopStarted)
        {
            _loopStarted = true;
            _ = Loop();
        }
    }

    public override async Task OnConnectedAsync()
    {
        if (NowPlaying is not null)
        {
            await SetPlaying2(Clients.Caller, NowPlaying?.Item1,
                _userManager.FindUserById(NowPlaying?.enqueuerId).Name, (int)(DateTime.Now - _time).TotalSeconds);
        }

        await Clients.Caller.SendAsync("QueueUpdated");

        OnlineUsers.Add(Context.User.Identity.Name);
        await Clients.All.SendAsync("OnlineUsersUpdated");

        if (Last5Chat.Count > 0)
        {
            foreach (var chat in Last5Chat)
            {
                await Clients.Caller.SendAsync("ChatUpdated", new { Name = chat.name, Content = chat.content });
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        OnlineUsers.RemoveWhere(x => x == Context.User.Identity.Name);
        await Clients.All.SendAsync("OnlineUsersUpdated");
    }

    private async Task Loop()
    {
        while (true)
        {
            if (NowPlaying is null)
            {
                if (MusicQueue.TryDequeue(out var music))
                {
                    try
                    {
                        await _context.Clients.All.SendAsync("QueueUpdated");
                        // start playing
                        NowPlaying = (await _neteaseApi.GetPlayableMusicAsync(music.Item1), music.enqueuerId);
                        _time = DateTime.Now;
                        await SetPlaying(NowPlaying?.Item1,
                            _userManager.FindUserById(NowPlaying?.enqueuerId).Name, 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, null);
                    }
                }
            }
            else
            {
                if ((DateTime.Now - _time).TotalMilliseconds >= NowPlaying?.Item1.Length) // playing is over
                {
                    NowPlaying = null;
                }
            }

            await Task.Delay(1000);
        }
    }

    // Remote invokable
    public async Task AddMusicToQueue(string id)
    {
        var music = await _neteaseApi.GetMusicAsync(id);
        MusicQueue.Enqueue((music, Context.User.Identity.Name));
        await Clients.All.SendAsync("QueueUpdated");
    }

    public record MusicEnqueueOrder(Music Music, string Enqueuer);

    public IEnumerable<MusicEnqueueOrder> GetMusicQueue()
    {
        return MusicQueue.Select(x => new MusicEnqueueOrder(x.Item1, _userManager.FindUserById(x.enqueuerId).Name))
            .ToList();
    }

    public void NextSong()
    {
        NowPlaying = null;
    }

    public async Task Rename(string newName)
    {
        _userManager.RenameUserById(Context.User.Identity.Name, newName);
        await Clients.All.SendAsync("OnlineUsersUpdated");
    }

    public IEnumerable<string> GetOnlineUsers()
    {
        return OnlineUsers.Select(x => _userManager.FindUserById(x)?.Name).ToList();
    }

    public async Task ChatSay(string content)
    {
        var model = new { Name = _userManager.FindUserById(Context.User.Identity.Name).Name, Content = content };
        while (Last5Chat.Count >= 5) Last5Chat.Dequeue();
        Last5Chat.Enqueue((model.Name, model.Content));
        await Clients.All.SendAsync("ChatUpdated", model);
    }

    public async Task RequestSetNowPlaying()
    {
        if (NowPlaying is not null)
        {
            await SetPlaying2(Clients.Caller, NowPlaying?.Item1, _userManager.FindUserById(NowPlaying?.enqueuerId).Name,
                (int)(DateTime.Now - _time).TotalSeconds);
        }
    }

    public async Task RequestQueueUpdate()
    {
        await Clients.All.SendAsync("QueueUpdated");
    }
    // End remote invokable

    public async Task SetPlaying(PlayableMusic music, string enqueuer, int playedTime)
    {
        await _context.Clients.All.SendAsync(nameof(SetPlaying), music, enqueuer, playedTime);
    }

    public async Task SetPlaying2(IClientProxy target, PlayableMusic music, string enqueuer, int playedTime)
    {
        await target.SendAsync(nameof(SetPlaying), music, enqueuer, playedTime);
    }
}