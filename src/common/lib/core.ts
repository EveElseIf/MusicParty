export type ProviderName = string
export interface MusicProvider {
    name: ProviderName;
    searchUser(keyword: string): Promise<TODO>;
    searchMusicByName(name: string): Promise<TODO>;
    getMusicById(): Promise<TODO>;
    getUserPlaylist(userId: string): Promise<TODO>;
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<TODO>
}

type TODO = any