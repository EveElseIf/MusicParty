using System.Text.Json;
using System.Text.Json.Nodes;

namespace MusicParty.MusicApi.Bilibili;

public class BilibiliApi : IMusicApi
{
    private readonly string _sessdata;
    private readonly string _phoneNo;
    private readonly HttpClient _http = new();
    public string ServiceName => "Bilibili";

    public BilibiliApi(string sessdata, string phoneNo)
    {
        _sessdata = sessdata;
        _phoneNo = phoneNo;
    }

    public void Login()
    {
        Console.WriteLine("You are going to login your Bilibili Account...");
        if (!string.IsNullOrEmpty(_sessdata))
        {
            SESSDATALogin(_sessdata).Wait();
        }
        else
        {
            if (string.IsNullOrEmpty(_phoneNo))
                throw new LoginException(
                    "You must set SESSDATA or phone number of your bilibili account in appsettings.json.");
            QRCodeLogin().Wait();
        }

        Console.WriteLine("Login success!");
    }

    private async Task SESSDATALogin(string sessdata)
    {
        if (!await CheckSESSDATAAsync(sessdata))
            throw new LoginException($"Login failed, check your SESSDATA.");
        _http.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={sessdata}");
        var resp2 = await _http.GetAsync("https://www.bilibili.com");
        var cookies = resp2.Headers.GetValues("Set-Cookie");
        _http.DefaultRequestHeaders.Add("Cookie", cookies);
    }

    private async Task<bool> CheckSESSDATAAsync(string sessdata)
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={sessdata}");
        var resp = await http.GetStringAsync("https://api.bilibili.com/nav");
        var j = JsonNode.Parse(resp)!;
        return j["code"]!.GetValue<int>() == 0;
    }

    private async Task QRCodeLogin()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TrySetCredentialAsync(string cred)
    {
        if (!await CheckSESSDATAAsync(cred))
            return false;
        _http.DefaultRequestHeaders.Remove("Cookie");
        _http.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={cred}");
        var resp = await _http.GetAsync("https://www.bilibili.com");
        var cookies = resp.Headers.GetValues("Set-Cookie");
        _http.DefaultRequestHeaders.Add("Cookie", cookies);
        return true;
    }

    public async Task<Music> GetMusicByIdAsync(string id)
    {
        var resp = await _http.GetStringAsync($"https://api.bilibili.com/x/web-interface/view?bvid={id}");
        var j = JsonSerializer.Deserialize<BVQueryJson.RootObject>(resp);
        if (j is null || j.code != 0 || j.data is null)
            throw new Exception($"Unable to get playable music, message: {resp}");
        return new Music($"{j.data.bvid},{j.data.cid}", j.data.title, new[] { j.data.owner.name });
    }

    public async Task<IEnumerable<Music>> SearchMusicByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<PlayableMusic> GetPlayableMusicAsync(Music music)
    {
        var ids = music.Id.Split(',');
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/player/playurl?bvid={ids[0]}&cid={ids[1]}&fnval=16");
        var j = JsonSerializer.Deserialize<PlayUrlJson.RootObject>(resp);
        if (j is null || j.code != 0 || j.data is null)
            throw new Exception($"Unable to get playable music, message: {resp}");

        return new PlayableMusic(music)
        {
            Url = $"/musicproxy?timestamp={DateTimeOffset.Now.ToUnixTimeSeconds()}",
            Length = j.data.dash.duration * 1000,
            NeedProxy = true, TargetUrl = j.data.dash.audio.OrderByDescending(x => x.id).First().baseUrl,
            Referer = "https://www.bilibili.com"
        };
    }

    public async Task<IEnumerable<MusicServiceUser>> SearchUserAsync(string keyword)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/web-interface/search/type?search_type=bili_user&keyword={keyword}");
        var j = JsonSerializer.Deserialize<SearchUserJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Search user failed, message: {resp}");
        if (j.data?.result is null)
            return Array.Empty<MusicServiceUser>();
        return j.data.result.Select(x => new MusicServiceUser(x.mid.ToString(), x.uname));
    }

    public async Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/v3/fav/folder/created/list-all?type=2&up_mid={userIdentifier}");
        var j = JsonSerializer.Deserialize<UserFavsJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Unable to get user playlist, message: ${resp}");
        if (j.data?.list is null)
            return Array.Empty<PlayList>();
        return j.data.list.Select(x => new PlayList(x.id.ToString(), x.title));
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/v3/fav/resource/list?platform=web&media_id={id}&ps=10&pn={offset / 10 + 1}");
        var j = JsonSerializer.Deserialize<FavDetailJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Unable to get playlist musics, message: {resp}");
        if (j.data?.medias is null)
            return Array.Empty<Music>();
        return j.data.medias.Where(x => x.title != "已失效视频" && x.type == 2)
            .Select(x => new Music(x.bvid, x.title, new[] { x.upper.name }));
    }

    #region JsonClasses

    private class SearchUserJson
    {
        public class RootObject
        {
            public long code { get; init; }
            public Data? data { get; init; }
        }

        public class Data
        {
            public Result[]? result { get; init; }
        }

        public class Result
        {
            public long mid { get; init; }
            public string uname { get; init; }
        }
    }

    private class UserFavsJson
    {
        public record RootObject(
            long code,
            Data? data
        );

        public record Data(
            List[]? list
        );

        public record List(
            long id,
            string title
        );
    }

    private class FavDetailJson
    {
        public record RootObject(
            long code,
            Data? data
        );

        public record Data(
            Medias[]? medias
        );

        public record Medias(
            long type,
            string title,
            Upper1 upper,
            string bvid
        );

        public record Upper1(
            string name
        );
    }

    private class BVQueryJson
    {
        public record RootObject(
            long code,
            Data? data
        );

        public record Data(
            string bvid,
            string title,
            Owner owner,
            long cid
        );

        public record Owner(
            string name
        );
    }

    private class PlayUrlJson
    {
        public class RootObject
        {
            public long code { get; set; }
            public Data? data { get; set; }
        }

        public class Data
        {
            public Dash dash { get; set; }
        }

        public class Dash
        {
            public long duration { get; set; }
            public Audio[] audio { get; set; }
        }

        public class Audio
        {
            public long id { get; set; }
            public string baseUrl { get; set; }
        }
    }

    #endregion
}