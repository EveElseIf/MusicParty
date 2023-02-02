using System.Text.Json.Nodes;
using MusicParty.Models;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;

namespace MusicParty;

public class NeteaseApi
{
    private readonly HttpClient _http = new();
    private readonly string _url;

    public NeteaseApi(string url, string phoneNo, string password, bool smsLogin)
    {
        _url = url;

        if (File.Exists("cookie.txt"))
        {
            Console.WriteLine("You have logined before, if you want to login again, please delete cookie.txt.");
            _http.DefaultRequestHeaders.Add("Cookie", File.ReadAllLines("cookie.txt"));
        }
        else
        {
            List<string> cookies;
            if (smsLogin)
            {
                Console.WriteLine("Using sms to login...");
                cookies = CaptchaLogin(url, phoneNo).ToList();
            }
            else if (!string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Notice: if you want to use QR code login, keep your password empty in appsettings.json.");
                Console.WriteLine("Using password to login... ");
                cookies = PasswordLogin(url, phoneNo, password).ToList();
            }
            else
            {
                cookies = QRCodeLogin(url).ToList();
            }
            Console.WriteLine("Login success!");

            _http.DefaultRequestHeaders.Add("Cookie", cookies);
            File.WriteAllLines("cookie.txt", cookies);
        }
    }

    private IEnumerable<string> PasswordLogin(string url, string phoneNo, string password)
    {
        var req = _http.GetAsync(url + $"/login/cellphone?phone={phoneNo}&password={password}").Result;
        if ((int)JsonNode.Parse(req.Content.ReadAsStringAsync().Result)["code"] != 200)
            throw new Exception("Unable to login netease account.");
        _ = req.Headers.TryGetValues("Set-Cookie", out var cookies);
        return cookies;
    }

    private IEnumerable<string> CaptchaLogin(string url, string phoneNo)
    {
        _http.GetAsync(url + $"/captcha/sent?phone={phoneNo}").Result.EnsureSuccessStatusCode();
        string captcha;
        while (true)
        {
            Console.WriteLine("Please enter your captcha:");
            captcha = Console.ReadLine();
            var req = _http.GetAsync(url + $"/captcha/verify?phone={phoneNo}&captcha={captcha}").Result;
            try
            {
                req.EnsureSuccessStatusCode();
                break;
            }
            catch
            {
                Console.WriteLine("Captcha wrong, please retry.");
            }
        }

        var req2 = _http.GetAsync(url + $"/login/cellphone?phone={phoneNo}&captcha={captcha}").Result;
        try
        {
            req2.EnsureSuccessStatusCode();
            _ = req2.Headers.TryGetValues("Set-Cookie", out var cookies);
            return cookies;
        }
        catch
        {
            throw new Exception("Unable to login netease account.");
        }
    }

    private IEnumerable<string> QRCodeLogin(string url)
    {
        var keyJson = _http.GetStringAsync(url + $"/login/qr/key?timestamp={GetTimestamp()}").Result;
        var key = JsonNode.Parse(keyJson)["data"]["unikey"].GetValue<string>();
        var qr = _http.GetStringAsync(url + $"/login/qr/create?key={key}&qrimg=true").Result;
        var qrimg = JsonNode.Parse(qr)["data"]["qrimg"].GetValue<string>();
        Console.WriteLine("Scan your QR code:");
        PrintQRCode(qrimg);
        while (true)
        {
            Task.Delay(3000).Wait();
            var req = _http.GetAsync(url + $"/login/qr/check?key={key}&timestamp={GetTimestamp()}").Result;
            var code = JsonNode.Parse(req.Content.ReadAsStringAsync().Result)["code"].GetValue<int>();
            if (code == 800)
                throw new Exception("Timeout.");
            if (code == 803)
            {
                _ = req.Headers.TryGetValues("Set-Cookie", out var cookies);
                return cookies;
            }
        }
    }

    private void PrintQRCode(string base64)
    {
        var bytes = Convert.FromBase64String(base64[22..]);
        var reader = new BarcodeReader<Rgba32>();
        var result = reader.Decode(Image.Load<Rgba32>(bytes));
        var g = new QRCodeGenerator();
        var qrdata = g.CreateQrCode(result.Text, QRCodeGenerator.ECCLevel.L);
        var qrcode = new AsciiQRCode(qrdata);
        var graph = qrcode.GetGraphic(1, "A", "B", false);
        foreach (var c in graph)
        {
            Console.BackgroundColor = ConsoleColor.White;
            if (c == '\n') Console.WriteLine();
            if (c == 'A')
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("  ");
            }

            if (c == 'B')
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write("  ");
            }
        }

        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine(base64);
    }

    private string GetTimestamp() => DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

    public async Task<PlayableMusic> GetPlayableMusicAsync(Music music)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/url/v1?id={music.Id}&level=exhigh");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200)
            throw new Exception($"Unable to get playable music, id: {music.Id}");
        var url = (string)j["data"][0]["url"];
        var length = (long)j["data"][0]["time"];
        return new PlayableMusic(music.Id, music.Name, music.Artist, url.Replace("http", "https"), length);
    }

    public async Task<Music> GetMusicAsync(string id)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/detail?ids={id}");
        var j = JsonNode.Parse(resp);
        if ((int)j["code"] != 200 || j["songs"].AsArray().Count == 0)
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