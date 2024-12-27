export type Provider = "NETEASE" | "BILIBILI"

export const neteaseProviderName: Provider = "NETEASE"
export const bilibiliProviderName: Provider = "BILIBILI"

export interface MusicProvider {
    provider: Provider;
    searchUser(keyword: string, offset: number): Promise<MusicProviderUserProfile[]>;
    bindUserWithProfile(user: User, profile: MusicProviderUserProfile): Promise<TODO>
    getUserProfile(user: User): Promise<MusicProviderUserProfile>
    searchMusicByName(name: string): Promise<TODO>;
    getMusicById(id: string): Promise<TODO>;
    getUserPlaylist(user: User): Promise<TODO>;
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<TODO>
}
export interface MusicProviderUserProfile {
    provider: Provider
    id: string
    name: string
}

type TODO = any

export interface User {
    id: string
    name: string
}

export interface Room {
    id: string
    name: string
}