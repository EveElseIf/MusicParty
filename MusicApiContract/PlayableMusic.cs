namespace MusicParty.MusicApi;

public record PlayableMusic(string Id, string Name, string[] Artist, string Url, long Length);