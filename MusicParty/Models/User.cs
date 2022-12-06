namespace MusicParty.Models;

public record User(string Id, string Name, IEnumerable<PlayList> PlayLists);