using Microsoft.AspNetCore.Mvc;
using System;

namespace MusicParty.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicProxyController : ControllerBase
{
    private static readonly HttpClient _http = new();
    private static CancellationTokenSource _tokenSource = new();
    private static int _currentLength;
    private static byte[]? _currentBuf;
    private static int _read;

    public MusicProxyController()
    {
    }

    [Route("Stream")]
    public async IAsyncEnumerable<byte> Stream()
    {
        if (_currentBuf is null)
            yield break;
        HttpContext.Response.Headers.ContentLength = _currentLength;
        HttpContext.Response.Headers.ContentType = "video/mp4";
        HttpContext.Response.Headers.Connection = "keep-alive";
        for (long i = 0; i < _currentLength; i++)
        {
            if (_tokenSource.IsCancellationRequested)
            {
                HttpContext.Abort();
                yield break;
            }

            if (i == _read)
                continue;

            yield return _currentBuf[i];
        }
    }


    public static async Task StartProxyAsync(string url, string referer)
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();
        var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Referrer = new Uri(referer);
        message.Headers.UserAgent.ParseAdd("Mozilla/5.0");
        message.Headers.Host = new Uri(url).Host;
        var resp = await _http.SendAsync(message);
        _currentLength = int.Parse(resp.Content.Headers.GetValues("Content-Length").First());
        _currentBuf = new byte[_currentLength];
        _ = Task.Run(async () =>
        {
            var stream = await resp.Content.ReadAsStreamAsync();
            var buf = new byte[1024];
            int read;
            while ((read = await stream.ReadAsync(buf.AsMemory(0, 1024))) > 0
                   && !_tokenSource.IsCancellationRequested)
            {
                WriteBuffer(buf, read, _currentBuf, _read);
                _read += read;
            }

            await stream.DisposeAsync();
        });
    }

    private static void WriteBuffer(byte[] buf, int length, byte[] dest, int offset)
    {
        Array.Copy(buf, 0, dest, offset, length);
    }
}