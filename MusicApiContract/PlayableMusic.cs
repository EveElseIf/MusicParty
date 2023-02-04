namespace MusicParty.MusicApi;

public record PlayableMusic : Music
{
    public PlayableMusic(Music parent) : base(parent)
    {
    }

    public string Url { get; init; } = "";
    public long Length { get; init; }
}