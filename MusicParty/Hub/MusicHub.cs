using Microsoft.AspNetCore.SignalR;
using MusicParty.Models;

namespace MusicParty.Hub;

public class MusicHub : Microsoft.AspNetCore.SignalR.Hub
{
    private static PlayableMusic? NowPlaying { get; set; }
    private static Queue<Music> PlayList { get; } = new();
    private static DateTime _time = default;
    private static bool _loopStarted = false;
    private readonly NeteaseApi _neteaseApi;
    private readonly IHubContext<MusicHub> _context;
    private readonly ILogger<MusicHub> _logger;

    public MusicHub(NeteaseApi neteaseApi, IHubContext<MusicHub> context, ILogger<MusicHub> logger)
    {
        _neteaseApi = neteaseApi;
        _context = context;
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
            await SetPlaying2(Clients.Caller, NowPlaying, (int)(DateTime.Now - _time).TotalSeconds);
        }
    }

    private async Task Loop()
    {
        while (true)
        {
            if (NowPlaying is null)
            {
                if (PlayList.TryDequeue(out var music))
                {
                    try
                    {
                        // start playing
                        NowPlaying = await _neteaseApi.GetPlayableMusicAsync(music);
                        _time = DateTime.Now;
                        await SetPlaying(NowPlaying, 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, null);
                    }
                }
            }
            else
            {
                if ((DateTime.Now - _time).TotalMilliseconds >= NowPlaying.Length) // playing is over
                {
                    NowPlaying = null;
                }
            }

            await Task.Delay(1000);
        }
    }

    // Remote invokable
    public async Task<bool> AddMusicToPlayList(string id)
    {
        try
        {
            var music = await _neteaseApi.GetMusicAsync(id);
            PlayList.Enqueue(music);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public IEnumerable<Music> GetPlayList()
    {
        return PlayList.ToArray();
    }

    public async Task SetPlaying(PlayableMusic music, int playedTime)
    {
        await _context.Clients.All.SendAsync(nameof(SetPlaying), music, playedTime);
    }

    public async Task SetPlaying2(IClientProxy target, PlayableMusic music, int playedTime)
    {
        await target.SendAsync(nameof(SetPlaying), music, playedTime);
    }
}