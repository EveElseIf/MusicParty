using System.Text.Json.Nodes;
using MusicParty.Models;

namespace MusicParty;

public class NeteaseApi
{
    private readonly HttpClient _http = new();
    private readonly string _url;

    public NeteaseApi(string url, string id, string password)
    {
        _url = url;

        if (File.Exists("cookie.txt"))
        {
            _http.DefaultRequestHeaders.Add("Cookie", File.ReadAllLines("cookie.txt"));
        }
        else
        {
            var req = _http.GetAsync(url + $"/login/cellphone?phone={id}&password={password}").Result;
            if ((int)JsonNode.Parse(req.Content.ReadAsStringAsync().Result)["code"] != 200)
                throw new Exception("Unable to login netease account.");
            _ = req.Headers.TryGetValues("Set-Cookie", out var cookies);
            _http.DefaultRequestHeaders.Add("Cookie", cookies);
            File.WriteAllLines("cookie.txt", cookies);
        }
    }

    public async Task<PlayableMusic> GetPlayableMusicAsync(Music music)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/url/v1?id={music.Id}&level=exhigh");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to get playable music, id: {music.Id}");
        var url = (string)j["data"][0]["url"];
        var length = (long)j["data"][0]["time"];
        return new PlayableMusic(music.Id, music.Name, music.Artist, url, length);
    }

    public async Task<Music> GetMusicAsync(string id)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/detail?ids={id}");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to get music, id: {id}");
        var name = (string)j["songs"][0]["name"];
        var ar = (string)j["songs"][0]["ar"][0]["name"];
        return new Music(id, name, ar);
    }
}