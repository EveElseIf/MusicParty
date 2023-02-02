namespace MusicParty.Models;

public record PlayableMusic(string Id, string Name, string Artist, string Url, long Length) : Music(Id,
    Name, Artist);