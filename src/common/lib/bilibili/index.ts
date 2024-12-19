import { MusicProvider } from "../core.js";

export const bilibiliProviderName = "BILIBILI"

class BilibiliMusicProvider implements MusicProvider {
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
    getUserPlaylist(userId: string): Promise<any> {
        throw new Error("Method not implemented.");
    }
    getMusicFromPlaylist(playlistId: string, offset: number): Promise<any> {
        throw new Error("Method not implemented.");
    }

}

export default BilibiliMusicProvider