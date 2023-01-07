import * as sr from "@microsoft/signalr"
export class connection {
    private _conn: sr.HubConnection;
    constructor(url: string, fn: any, queueUpdate: () => void, onlineUsersUpdate: () => void, chatUpdate: (chat: { name: string, content: string }) => void) {
        this._conn = new sr.HubConnectionBuilder().withUrl(url).build();
        this._conn.on("SetPlaying", fn);
        this._conn.on("QueueUpdated", queueUpdate);
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
    public async addMusicToPlayList(id: string): Promise<any> {
        return await this._conn.invoke("AddMusicToPlayList", id);
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