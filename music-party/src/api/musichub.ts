import * as sr from "@microsoft/signalr"
export class connection {
    private _conn: sr.HubConnection;
    private _refresh: any;
    constructor(url: string, fn: any, refresh: any, onlineUsersUpdate: () => void, chatUpdate: (chat: { name: string, content: string }) => void) {
        this._conn = new sr.HubConnectionBuilder().withUrl(url).build();
        this._conn.on("SetPlaying", fn);
        this._refresh = refresh;
        this._conn.on("OnlineUsersUpdated", onlineUsersUpdate);
        this._conn.on("ChatUpdated", chatUpdate);
    }
    public start(): void {
        if (this._conn.state === sr.HubConnectionState.Disconnected) {
            this._conn.start().then(() => {
                console.log("music hub: " + this._conn.state);
            })
        }
    }
    public async addMusicToPlayList(id: string): Promise<boolean> {
        const ret = await this._conn.invoke("AddMusicToPlayList", id);
        this._refresh();
        return ret;
    }
    public async getPlayList(): Promise<any> {
        return await this._conn.invoke("GetPlayList");
    }
    public async nextSong(): Promise<any> {
        return await this._conn.invoke("NextSong");
    }
    public async rename(newName: string): Promise<any> {
        return await this._conn.invoke("rename", newName);
    }
    public async getOnlineUsers(): Promise<string[]> {
        return await this._conn.invoke("GetOnlineUsers");
    }
    public async ChatSay(content: string): Promise<any> {
        return await this._conn.invoke("ChatSay", content);
    }

}
export interface music {
    url: string;
    name: string;
    artist: string;
}