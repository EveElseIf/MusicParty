using System.Text.Json.Serialization;

namespace MusicParty.MusicApi;

public record PlayableMusic : Music
{
    public PlayableMusic(Music parent) : base(parent)
    {
    }

    public string Url { get; init; } = "";
    public long Length { get; init; }
    [JsonIgnore] public bool NeedProxy { get; init; }
    [JsonIgnore] public string? TargetUrl { get; init; }
    [JsonIgnore] public string? Referer { get; init; }
}