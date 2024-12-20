export type ProviderName = string
export interface MusicProvider {
    name: ProviderName;
    searchUser(keyword: string): Promise<TODO>;
    bindUserWithProfile(user: User, profile: MusicProviderUserProfile): Promise<TODO>
    getProviderSpecifiedProfile(user: User): Promise<MusicProviderUserProfile>
    searchMusicByName(name: string): Promise<TODO>;
    getMusicById(id: string): Promise<TODO>;
    getUserPlaylist(user: User): Promise<TODO>;
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<TODO>
}
export interface MusicProviderUserProfile {
    providerName: ProviderName
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