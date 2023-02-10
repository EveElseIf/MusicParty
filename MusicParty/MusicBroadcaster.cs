using Microsoft.AspNetCore.SignalR;
using MusicParty.Hub;
using MusicParty.MusicApi;

namespace MusicParty;

public class MusicBroadcaster
{
    public (PlayableMusic music, string enqueuerId)? NowPlaying { get; private set; }
    private ToppableQueue<MusicOrderAction> MusicQueue { get; } = new();
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
                    if (!_apis.TryGetMusicApi(musicOrder.Service, out var ma))
                    {
                        _logger.LogError(new ArgumentException($"Unknown api provider {musicOrder.Service}",
                                nameof(musicOrder.Service)), "{MusicId} with {Api} play failed, skipping...",
                            musicOrder.Music.Id, musicOrder.Service);
                        continue;
                    }

                    for (var i = 0;; i++) // try 3 times before skip.
                    {
                        try
                        {
                            var music = await ma!.GetPlayableMusicAsync(musicOrder.Music);
                            NowPlaying = (music, musicOrder.EnqueuerId);
                            if (music.NeedProxy)
                            {
                                await MusicProxyMiddleware.StartProxyAsync(new MusicProxyRequest(music.TargetUrl!,
                                    "audio/mp4", music.Referer,
                                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.78"));
                            }

                            NowPlayingStartedTime = DateTime.Now;
                            await SetNowPlaying(NowPlaying.Value.music,
                                _userManager.FindUserById(musicOrder.EnqueuerId)!.Name);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i >= 2) // failed 3 times, skip.
                            {
                                _logger.LogError(ex, "{MusicId} with {Api} play failed, skipping...",
                                    musicOrder.Music.Id, musicOrder.Service);
                                await GlobalMessage($"Failed to play {musicOrder.Music.Name}, skip to next music.");
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

    public IEnumerable<MusicOrderAction> GetQueue() => MusicQueue;

    public async Task EnqueueMusic(Music music, string apiName, string enqueuerId)
    {
        var action = new MusicOrderAction(Guid.NewGuid().ToString()[..8], music, apiName, enqueuerId);
        MusicQueue.Enqueue(action);
        await MusicEnqueued(action.ActionId, music, _userManager.FindUserById(enqueuerId)!.Name);
    }

    public async Task NextSong(string operatorId)
    {
        if (NowPlaying is null) return;
        await MusicCut(operatorId, NowPlaying.Value.music);
        NowPlaying = null;
    }

    public async Task TopSong(string actionId, string operatorId)
    {
        MusicQueue.TopItem(x => x.ActionId == actionId);
        await MusicTopped(actionId, _userManager.FindUserById(operatorId)!.Name);
    }

    private async Task SetNowPlaying(PlayableMusic music, string enqueuerName)
    {
        await _context.Clients.All.SendAsync(nameof(SetNowPlaying), music, enqueuerName,
            0); // arg3 is music already played time
    }

    private async Task MusicEnqueued(string actionId, Music music, string enqueuerName)
    {
        await _context.Clients.All.SendAsync(nameof(MusicEnqueued), actionId, music, enqueuerName);
    }

    private async Task MusicDequeued()
    {
        await _context.Clients.All.SendAsync(nameof(MusicDequeued));
    }

    private async Task MusicTopped(string actionId, string operatorName)
    {
        await _context.Clients.All.SendAsync(nameof(MusicTopped), actionId, operatorName);
    }

    private async Task MusicCut(string operatorId, Music music)
    {
        await _context.Clients.All.SendAsync(nameof(MusicCut), _userManager.FindUserById(operatorId)!.Name, music);
    }

    private async Task GlobalMessage(string content)
    {
        await _context.Clients.All.SendAsync(nameof(GlobalMessage), content);
    }

    public record MusicOrderAction(string ActionId, Music Music, string Service, string EnqueuerId);

    private class ToppableQueue<T> : LinkedList<T>
    {
        public void TopItem(Func<T, bool> pred)
        {
            if (Count == 1)
                return;
            var node = Find(this.First(pred))!; // if count == 0, this will throw an exception.
            Remove(node);
            AddBefore(First!, node);
        }

        public void Enqueue(T item)
        {
            AddLast(item);
        }

        public bool TryDequeue(out T? item)
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }
            else
            {
                item = First!.Value;
                RemoveFirst();
                return true;
            }
        }
    }
}