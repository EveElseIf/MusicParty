import { MusicProvider, MusicProviderUserProfile, User } from "../core.js";

export const bilibiliProviderName = "BILIBILI"

class BilibiliMusicProvider implements MusicProvider {
    getProviderSpecifiedProfile(user: User): Promise<MusicProviderUserProfile> {
        throw new Error("Method not implemented.");
    }
    bindUserWithProfile(user: User, profile: MusicProviderUserProfile): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getUserPlaylist(user: User): Promise<any> {
        throw new Error("Method not implemented.");
    }
    get name(): string {
        return bilibiliProviderName
    }
    searchUser(keyword: string): Promise<any> {
        throw new Error("Method not implemented.");
    }
    searchMusicByName(name: string): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getMusicById(): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<any> {
        throw new Error("Method not implemented.");
    }

}

export default BilibiliMusicProvider