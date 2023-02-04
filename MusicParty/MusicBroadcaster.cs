using Microsoft.AspNetCore.SignalR;
using MusicParty.Hub;
using MusicParty.MusicApi;

namespace MusicParty;

public class MusicBroadcaster
{
    public (PlayableMusic music, string enqueuerId)? NowPlaying { get; private set; }
    private Queue<(Music music, string service, string enqueuerId)> MusicQueue { get; } = new();
    public DateTime NowPlayingStartedTime { get; private set; }
    private readonly IEnumerable<IMusicApi> _apis;
    private readonly IHubContext<MusicHub> _context;
    private readonly UserManager _userManager;
    private readonly ILogger<MusicBroadcaster> _logger;

    public MusicBroadcaster(IEnumerable<IMusicApi> apis, IHubContext<MusicHub> context, UserManager userManager,
        ILogger<MusicBroadcaster> logger)
    {
        _apis = apis;
        _context = context;
        _userManager = userManager;
        _logger = logger;
        Task.Run(Loop);
    }

    private async Task Loop()
    {
        while (true)
        {
            if (NowPlaying is null)
            {
                if (MusicQueue.TryDequeue(out var musicOrder))
                {
                    await MusicDequeued();
                    if (!_apis.TryGetMusicApi(musicOrder.service, out var ma))
                    {
                        _logger.LogError(new ArgumentException($"Unknown api provider {musicOrder.service}",
                                nameof(musicOrder.service)), "{MusicId} with {Api} play failed, skipping...",
                            musicOrder.music.Id, musicOrder.service);
                        continue;
                    }

                    for (var i = 0;; i++) // try 3 times before skip.
                    {
                        try
                        {
                            NowPlaying = (await ma!.GetPlayableMusicAsync(musicOrder.music), musicOrder.enqueuerId);
                            NowPlayingStartedTime = DateTime.Now;
                            await SetNowPlaying(NowPlaying.Value.music,
                                _userManager.FindUserById(musicOrder.enqueuerId)!.Name);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i >= 2) // failed 3 times, skip.
                            {
                                _logger.LogError(ex, "{MusicId} with {Api} play failed, skipping...",
                                    musicOrder.music.Id, musicOrder.service);
                                await GlobalMessage($"Failed to play {musicOrder.music.Name}, skip to next music.");
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if ((DateTime.Now - NowPlayingStartedTime).TotalMilliseconds >=
                    NowPlaying.Value.music.Length) // play is over
                {
                    NowPlaying = null;
                }
            }

            await Task.Delay(1000);
        }
    }

    public IEnumerable<(Music music, string enqueuerName)> GetQueue()
        => MusicQueue.Select(x => (x.music, _userManager.FindUserById(x.enqueuerId)!.Name)).ToList();

    public async Task EnqueueMusic(Music music, string apiName, string enqueuerId)
    {
        MusicQueue.Enqueue((music, apiName, enqueuerId));
        await MusicEnqueued(music, _userManager.FindUserById(enqueuerId)!.Name);
    }

    public void NextSong()
    {
        NowPlaying = null;
    }

    private async Task SetNowPlaying(PlayableMusic music, string enqueuerName)
    {
        await _context.Clients.All.SendAsync(nameof(SetNowPlaying), music, enqueuerName,
            0); // arg3 is music already played time
    }

    private async Task MusicEnqueued(Music music, string enqueuerName)
    {
        await _context.Clients.All.SendAsync(nameof(MusicEnqueued), music, enqueuerName);
    }

    private async Task MusicDequeued()
    {
        await _context.Clients.All.SendAsync(nameof(MusicDequeued));
    }

    private async Task GlobalMessage(string content)
    {
        await _context.Clients.All.SendAsync(nameof(GlobalMessage), content);
    }
}