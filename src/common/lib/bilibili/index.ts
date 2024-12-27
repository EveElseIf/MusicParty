import { bilibiliProviderName, MusicProvider, MusicProviderUserProfile, Provider, User } from "../core.js";

class BilibiliMusicProvider implements MusicProvider {
    getUserProfile(user: User): Promise<MusicProviderUserProfile> {
        throw new Error("Method not implemented.");
    }
    bindUserWithProfile(user: User, profile: MusicProviderUserProfile): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getUserPlaylist(user: User): Promise<any> {
        throw new Error("Method not implemented.");
    }
    get provider(): Provider {
        return bilibiliProviderName
    }
    searchUser(keyword: string, offset: number): Promise<any> {
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