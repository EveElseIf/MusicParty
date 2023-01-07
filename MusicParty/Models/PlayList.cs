namespace MusicParty.Models;

public record PlayList(string Id, string Name, IEnumerable<Music>? Musics = null);