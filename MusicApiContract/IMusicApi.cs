namespace MusicParty.MusicApi;

public interface IMusicApi
{
    string ServiceName { get; }
    Task<bool> TrySetCredentialAsync(string cred);
    Task<Music> GetMusicByIdAsync(string id);
    Task<Music[]> SearchMusicByNameAsync(string name, string musicname);
    Task<PlayableMusic> GetPlayableMusicAsync(Music music);
    Task<IEnumerable<MusicServiceUser>> SearchUserAsync(string keyword);
    Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier);
    Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0);
}