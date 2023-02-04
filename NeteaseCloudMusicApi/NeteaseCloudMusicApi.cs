using System.Text.Json.Nodes;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;

namespace MusicParty.MusicApi.NeteaseCloudMusic;

public class NeteaseCloudMusicApi : IMusicApi
{
    private readonly HttpClient _http = new();
    private readonly string _url;
    private readonly string _phoneNo;
    private readonly string _password;
    private readonly bool _smsLogin;

    public NeteaseCloudMusicApi(string url, string phoneNo, string password, bool smsLogin)
    {
        _url = url;
        _phoneNo = phoneNo;
        _password = password;
        _smsLogin = smsLogin;
    }

    public string ServiceName => "NeteaseCloudMusic";

    public void Login()
    {
        Console.WriteLine("You are going to login your Netease Cloud Music Account...");
        if (string.IsNullOrEmpty(_phoneNo))
        {
            throw new LoginException(
                "The phone number of your Netease Cloud Music Account is null, please set it in appsettings.json");
        }

        if (File.Exists("cookie.txt"))
        {
            Console.WriteLine("You have logged in before, if you want to login again, please delete cookie.txt.");
            _http.DefaultRequestHeaders.Add("Cookie", File.ReadAllLines("cookie.txt"));
        }
        else
        {
            List<string> cookies;
            try
            {
                if (_smsLogin)
                {
                    Console.WriteLine("Using sms to login...");
                    cookies = CaptchaLogin(_url, _phoneNo).ToList();
                }
                else if (!string.IsNullOrEmpty(_password))
                {
                    Console.WriteLine(
                        "Notice: if you want to use QR code login, keep your password empty in appsettings.json.");
                    Console.WriteLine("Using password to login... ");
                    cookies = PasswordLogin(_url, _phoneNo, _password).ToList();
                }
                else
                {
                    cookies = QRCodeLogin(_url).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new LoginException("Login failed.", ex);
            }

            _http.DefaultRequestHeaders.Add("Cookie", cookies);
            File.WriteAllLines("cookie.txt", cookies);
        }

        Console.WriteLine("Login success!");
    }

    private IEnumerable<string> PasswordLogin(string url, string phoneNo, string password)
    {
        var req = _http.GetAsync(url + $"/login/cellphone?phone={phoneNo}&password={password}").Result;
        req.EnsureSuccessStatusCode();
        return req.Headers.GetValues("Set-Cookie");
    }

    private IEnumerable<string> CaptchaLogin(string url, string phoneNo)
    {
        _http.GetAsync(url + $"/captcha/sent?phone={phoneNo}").Result.EnsureSuccessStatusCode();
        string captcha;
        while (true)
        {
            Console.WriteLine("Please enter your captcha:");
            captcha = Console.ReadLine() ?? "";
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
        req2.EnsureSuccessStatusCode();
        return req2.Headers.GetValues("Set-Cookie");
    }

    private IEnumerable<string> QRCodeLogin(string url)
    {
        var keyJson = _http.GetStringAsync(url + $"/login/qr/key?timestamp={GetTimestamp()}").Result;
        var key = JsonNode.Parse(keyJson)!["data"]!["unikey"]!.GetValue<string>();
        var qr = _http.GetStringAsync(url + $"/login/qr/create?key={key}&qrimg=true").Result;
        var qrimg = JsonNode.Parse(qr)!["data"]!["qrimg"]!.GetValue<string>();
        Console.WriteLine("Scan your QR code:");
        PrintQRCode(qrimg);
        while (true)
        {
            Task.Delay(3000).Wait();
            var req = _http.GetAsync(url + $"/login/qr/check?key={key}&timestamp={GetTimestamp()}").Result;
            var code = JsonNode.Parse(req.Content.ReadAsStringAsync().Result)!["code"]!.GetValue<int>();
            if (code == 800)
                throw new Exception("Timeout.");
            if (code == 803)
            {
                return req.Headers.GetValues("Set-Cookie");
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
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get playable music, id: {music.Id}");
        var url = (string)j["data"]![0]!["url"]!;
        var length = (long)j["data"]![0]!["time"]!;
        return new PlayableMusic(music) { Url = url.Replace("http", "https"), Length = length };
    }

    public async Task<Music> GetMusicByIdAsync(string id)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/detail?ids={id}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200 || j["songs"]!.AsArray().Count == 0)
            throw new Exception($"Unable to get music, id: {id}");
        var name = (string)j["songs"]![0]!["name"]!;
        var ar = j["songs"]![0]!["ar"]!.AsArray().Select(x => x!["name"]!.GetValue<string>()).ToArray();
        return new Music(id, name, ar);
    }

    public Task<IEnumerable<Music>> SearchMusicByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<MusicServiceUser>> SearchUserAsync(string keyword)
    {
        var resp = await _http.GetStringAsync(_url + $"/search?type=1002&keywords={keyword}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to search user, keyword: {keyword}");

        return j["result"]!["userprofiles"]!.AsArray()
            .Select(x => new MusicServiceUser(x!["userId"]!.GetValue<long>().ToString(), (string)x["nickname"]!))
            .ToList();
    }

    public async Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier)
    {
        var resp = await _http.GetStringAsync(_url + $"/user/playlist?uid={userIdentifier}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get user playlist, user identifier: {userIdentifier}");

        return (from b in j["playlist"]!.AsArray()
            let id = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            select new PlayList(id, name)).ToList();
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(_url + $"/playlist/track/all?id={id}&limit=10&offset={offset}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get playlist musics, playlist id: {id}.");

        return (from b in j["songs"]!.AsArray()
            let id2 = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            let artists = b["ar"].AsArray().Select(y => (string)y["name"]).ToArray()
            select new Music(id2, name, artists)).ToList();
    }
}