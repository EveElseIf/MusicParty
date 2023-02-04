namespace MusicParty.MusicApi;

public record PlayableMusic(string Id, string Name, string[] Artists, string Url, long Length);