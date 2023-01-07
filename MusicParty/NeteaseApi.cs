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

    public async Task<IEnumerable<NeteaseUser>> SearchNeteaseUsersAsync(string keyword)
    {
        var resp = await _http.GetStringAsync(_url + $"/search?type=1002&keywords={keyword}");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to search user, keyword: {keyword}");

        return j["result"]["userprofiles"].AsArray()
            .Select(x => new NeteaseUser(x["userId"].GetValue<long>().ToString(), (string)x["nickname"])).ToList();
    }

    public async Task<IEnumerable<PlayList>> GetUserPlaylistAsync(string uid)
    {
        var resp = await _http.GetStringAsync(_url + $"/user/playlist?uid={uid}");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to get user playlist, uid: {uid}");

        //return j["result"]["playlist"].AsArray()
        //    .Select(x => new PlayList(x["id"].GetValue<long>().ToString(), (string)x["name"])).ToList();

        return (from b in j["playlist"].AsArray()
            let id = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            select new PlayList(id, name)).ToList();
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(_url + $"/playlist/track/all?id={id}&limit=10&offset={offset}");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to get playlist musics, playlist id: {id}");
        //return j["songs"].AsArray().Select(x => new Music(j["id"].GetValue<long>().ToString(), (string)j["name"],
        //    string.Join(" / ", j["ar"].AsArray().Select(y => (string)y["name"]))));

        return (from b in j["songs"].AsArray()
            let id2 = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            let artist = string.Join(" / ", b["ar"].AsArray().Select(y => (string)y["name"]))
            select new Music(id2, name, artist)).ToList();
    }
}