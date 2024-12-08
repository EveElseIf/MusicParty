using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;
using System;
using System.Net;

namespace MusicParty.MusicApi.NeteaseCloudMusic;

public class NeteaseCloudMusicApi : IMusicApi
{
    private readonly HttpClient _http = new HttpClient(new HttpClientHandler() { UseCookies = false });
    private readonly string _url;
    private readonly string _phoneNo;
    private readonly string _cookie;

    private readonly string _password;

    public NeteaseCloudMusicApi(string url, string phoneNo, string cookie, string password)
    {
        _url = url;
        _phoneNo = phoneNo;
        _cookie = cookie;
        _password = password;
    }

    public string ServiceName => "NeteaseCloudMusic";

    public void Login()
    {
        Console.WriteLine("You are going to login your Netease Cloud Music Account...");

        if (File.Exists("cookie.txt"))
        {
            Console.WriteLine("You have logged in before, if you want to login again, please delete cookie.txt.");
            _http.DefaultRequestHeaders.Add("Cookie", File.ReadAllText("cookie.txt"));
        }
        else
        {
            string cookie;
            try
            {
                if (!string.IsNullOrEmpty(_cookie))
                {
                    cookie = _cookie;
                    if (!CheckCookieAsync(_cookie).Result)
                        throw new LoginException("Login failed, check your cookie.");
                }
                else
                {
                    if (string.IsNullOrEmpty(_phoneNo) || string.IsNullOrEmpty(_password))
                    {
                        throw new LoginException(
                            "The phone number or password of your Netease Cloud Music Account is null, please set it in appsettings.json");
                    }
                    cookie = PhoneNumberLogin(_phoneNo,_password);
                    _http.DefaultRequestHeaders.Add("Cookie", cookie);
                }
            }
            catch (Exception ex)
            {
                throw new LoginException("Login failed.", ex);
            }

            File.WriteAllText("cookie.txt", cookie);
        }

        Console.WriteLine("Login success!");
    }

    private string GetCookieEncoded()
    {
        string cookie;
        cookie = File.ReadAllText("cookie.txt");
        cookie = WebUtility.UrlEncode(cookie);
        return cookie;
    }

    private async Task<bool> CheckCookieAsync(string cookie)
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Cookie", cookie);
        var resp = await http.GetStringAsync($"{_url}/user/account?realIP=183.232.239.22&timestamp={GetTimestamp()}");
        var j = JsonNode.Parse(resp)!;
        return j["profile"].Deserialize<object>() is not null;
    }

    private string PhoneNumberLogin(string phoneNo,string password)
    {
        var rst = _http.GetStringAsync(_url + $"/login/cellphone?realIP=183.232.239.22&phone={phoneNo}&password={password}").Result;
        var cookie = JsonNode.Parse(rst)!["cookie"]!.GetValue<string>();
        return cookie;
    }
    private IEnumerable<string> QRCodeLogin(string url)
    {
        var keyJson = _http.GetStringAsync(url + $"/login/qr/key?realIP=183.232.239.22&timestamp={GetTimestamp()}").Result;
        var key = JsonNode.Parse(keyJson)!["data"]!["unikey"]!.GetValue<string>();
        var qr = _http.GetStringAsync(url + $"/login/qr/create?realIP=183.232.239.22&key={key}&qrimg=true").Result;
        var qrimg = JsonNode.Parse(qr)!["data"]!["qrimg"]!.GetValue<string>();
        Console.WriteLine("Scan your QR code:");
        PrintQRCode(qrimg);
        while (true)
        {
            Task.Delay(3000).Wait();
            var req = _http.GetAsync(url + $"/login/qr/check?realIP=183.232.239.22&key={key}&timestamp={GetTimestamp()}").Result;
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
        var resp = await _http.GetStringAsync(_url + $"/song/url?realIP=183.232.239.22&id={music.Id}&cookie={GetCookieEncoded()}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get playable music, message: {resp}");
        var url = (string)j["data"]![0]!["url"]!;
        var length = (long)j["data"]![0]!["time"]!;
        return new PlayableMusic(music) { Url = url.Replace("http", "https"), Length = length };
    }

    public async Task<bool> TrySetCredentialAsync(string cred)
    {
        if (!await CheckCookieAsync(cred))
            return false;
        _http.DefaultRequestHeaders.Remove("Cookie");
        _http.DefaultRequestHeaders.Add("Cookie", cred);
        await File.WriteAllTextAsync("cookie.txt", cred);
        return true;
    }

    public async Task<Music> GetMusicByIdAsync(string id)
    {
        var resp = await _http.GetStringAsync(_url + $"/song/detail?realIP=183.232.239.22&ids={id}&cookie={GetCookieEncoded()}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200 || j["songs"]!.AsArray().Count == 0)
            throw new Exception($"Unable to get music, message: {resp}");
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
        var resp = await _http.GetStringAsync(_url + $"/search?realIP=183.232.239.22&type=1002&keywords={keyword}&cookie={GetCookieEncoded()}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to search user, message: {resp}");

        return j["result"]!["userprofiles"]!.AsArray()
            .Select(x => new MusicServiceUser(x!["userId"]!.GetValue<long>().ToString(), (string)x["nickname"]!));
    }

    public async Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier)
    {
        var resp = await _http.GetStringAsync(_url + $"/user/playlist?realIP=183.232.239.22&uid={userIdentifier}&cookie={GetCookieEncoded()}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get user playlist, message: ${resp}");

        return from b in j["playlist"]!.AsArray()
            let id = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            select new PlayList(id, name);
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(_url + $"/playlist/track/all?realIP=183.232.239.22&id={id}&limit=10&offset={offset}&cookie={GetCookieEncoded()}");
        var j = JsonNode.Parse(resp)!;
        if ((int)j["code"]! != 200)
            throw new Exception($"Unable to get playlist musics, message: {resp}");

        return from b in j["songs"]!.AsArray()
            let id2 = b["id"].GetValue<long>().ToString()
            let name = (string)b["name"]
            let artists = b["ar"].AsArray().Select(y => (string)y["name"]).ToArray()
            select new Music(id2, name, artists);
    }
}
