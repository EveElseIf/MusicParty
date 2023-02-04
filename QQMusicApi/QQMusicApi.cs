using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace MusicParty.MusicApi.QQMusic;

public class QQMusicApi : IMusicApi
{
    private readonly string _url;
    private HttpClient _http = new();

    public QQMusicApi(string url, string qqNo, string cookie)
    {
        _url = url;
        Console.WriteLine("You are going to login your QQ music account using cookie...");
        if (string.IsNullOrEmpty(cookie))
            throw new Exception("You must set QQ music cookie in appsettings.json.");
        Login(qqNo, cookie).Wait();
    }

    private async Task Login(string qqNo, string cookie)
    {
        (await _http.PostAsync(_url + "/user/setcookie", JsonContent.Create(new { data = cookie })))
            .EnsureSuccessStatusCode();
        var resp = await _http.GetAsync(_url + $"/user/getcookie?id={qqNo}");
        resp.EnsureSuccessStatusCode();
        if (!resp.Headers.TryGetValues("Set-Cookie", out var cookies))
            throw new Exception("Set cookie failed, check your QQ No. in appsettings.json.");
        _http.DefaultRequestHeaders.Add("Cookie", cookies);
    }

    public string ServiceName => "QQMusic";

    public async Task<Music> GetMusicByIdAsync(string id)
    {
        var ids = id.Split(',');
        var resp = await _http.GetStringAsync(_url + $"/song?songmid={ids[0]}");
        var j = JsonNode.Parse(resp)!;
        if (j["result"]!.GetValue<int>() != 100)
            throw new Exception($"Unable to get playable music, id: {id}");
        var name = j["data"]!["track_info"]!["name"]!.GetValue<string>();
        var artists = j["data"]!["track_info"]!["singer"]!.AsArray()
            .Select(x => x!["name"]!.GetValue<string>()).ToArray();
        return new Music(id, name, artists);
    }

    public async Task<IEnumerable<Music>> SearchMusicByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<PlayableMusic> GetPlayableMusicAsync(Music music)
    {
        var ids = music.Id.Split(',');
        var resp1 = await _http.GetStringAsync(_url + $"/song?songmid={ids[0]}");
        var j1 = JsonNode.Parse(resp1)!;
        if (j1["result"]!.GetValue<int>() != 100)
            throw new Exception($"Unable to get playable music, id: {music.Id}");
        var length = j1["data"]!["track_info"]!["interval"]!.GetValue<int>() * 1000;
        var resp2 = await _http.GetStringAsync(_url + $"/song/url?id={ids[0]}&mediaId={ids[1]}&type=320");
        var j2 = JsonNode.Parse(resp2)!;
        if (j2["result"]!.GetValue<int>() != 100 || string.IsNullOrEmpty(j2["data"]!.GetValue<string>()))
            throw new Exception($"Unable to get playable music, id: {music.Id}");
        var url = j2["data"]!.GetValue<string>();
        return new PlayableMusic(music) { Url = url.Replace("http", "https"), Length = length };
    }

    public async Task<IEnumerable<MusicServiceUser>> SearchUserAsync(string keyword)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier)
    {
        var resp = await _http.GetStringAsync(_url + $"/user/songlist?id={userIdentifier}");
        var j = JsonNode.Parse(resp)!;
        if (j["result"]!.GetValue<int>() != 100)
            throw new Exception($"Unable to get user playlist, user identifier: {userIdentifier}");
        var ret = j["data"]!["list"]!.AsArray()
            .Select(x => new PlayList(
                x!["tid"]!.GetValue<long>().ToString(),
                x!["diss_name"]!.GetValue<string>())).Where(x => x.Id != "0").ToArray();
        return ret;
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(_url + $"/songlist?id={id}");
        var j = JsonNode.Parse(resp)!;
        if (j["result"]!.GetValue<int>() != 100)
            throw new Exception($"Unable to get playlist musics, playlist id: {id}.");
        var ret = j["data"]!["songlist"]!.AsArray()
            .Select(x =>
            {
                var artists = x!["singer"]!.AsArray().Select(y => y!["name"]!.GetValue<string>()).ToArray();
                return new Music(x["songmid"]!.GetValue<string>() + ',' + x["strMediaMid"]!.GetValue<string>(),
                    x["songorig"]!.GetValue<string>(), artists);
            }).ToArray();
        return ret;
    }
}