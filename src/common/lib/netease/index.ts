import { MusicInfo, MusicProvider, MusicProviderUserProfile, neteaseProviderName, Provider, User } from "../core.js";
import NeteaseCloudMusicApi from "NeteaseCloudMusicApi"
const { search, song_detail } = NeteaseCloudMusicApi

class NeteaseMusicProvider implements MusicProvider {

    cookie: string
    constructor(cookie: string) {
        this.cookie = cookie
    }
    getUserProfile(user: User): Promise<MusicProviderUserProfile> {
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

    public get provider(): Provider {
        return neteaseProviderName;
    }

    async searchUser(keyword: string, offset: number): Promise<MusicProviderUserProfile[]> {
        const result = await search({
            keywords: keyword,
            type: 1002,
            offset: offset,
            cookie: this.cookie,
        })
        if (result.body.code != 200) throw new Error(result.body as any);
        const profiles = (result.body.result as any).userprofiles as any[]
        const ret = profiles.map(x => ({
            provider: neteaseProviderName,
            id: String(x.userId),
            name: x.nickname
        }))
        return ret
    }
    searchMusicByName(name: string): Promise<any> {
        throw new Error("Method not implemented.");
    }
    async getMusicById(id: string): Promise<MusicInfo> {
        const result = await song_detail({ ids: id, cookie: this.cookie })
        if (result.body.code != 200) throw new Error(result.body as any);
        if (result.body.songs.length === 0) {
            throw new Error("music not found")
        }
        const song = result.body.songs[0]
        const info: MusicInfo = {
            provider: neteaseProviderName,
            name: song.name,
            id,
            length: song.dt,
            artists: song.ar.map(x => x.name),
        }
        return info
    }
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<any> {
        throw new Error("Method not implemented.");
    }
}

export default NeteaseMusicProvider