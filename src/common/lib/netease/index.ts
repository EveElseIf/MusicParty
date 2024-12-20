import { MusicProvider, MusicProviderUserProfile, User } from "../core.js";

export const neteaseProviderName = "NETEASE";

class NeteaseMusicProvider implements MusicProvider {

    cookie: string
    constructor(cookie: string) {
        this.cookie = cookie
    }
    getProviderSpecifiedProfile(user: User): Promise<MusicProviderUserProfile> {
        throw new Error("Method not implemented.");
    }
    bindUserWithProfile(user: User, profile: MusicProviderUserProfile): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getUserPlaylist(user: User): Promise<any> {
        throw new Error("Method not implemented.");
    }

    async init(): Promise<void> {
        //TODO
    }

    public get name(): string {
        return neteaseProviderName;
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

export default NeteaseMusicProvider