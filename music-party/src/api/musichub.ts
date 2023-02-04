import * as sr from "@microsoft/signalr"
export class Connection {
    private _conn: sr.HubConnection;
    constructor(url: string,
        setNowPlaying: (music: music, enqueuerName: string, playedTime: number) => void,
        musicEnqueued: (music: music, enqueuerName: string) => void,
        musicDequeued: () => void,
        onlineUserLogin: (id: string, name: string) => void,
        onlineUserLogout: (id: string) => void,
        onlineUserRename: (id: string, newName: string) => void,
        newChat: (name: string, content: string) => void,
        globalMessage: (content: string) => void,
        abort: (msg: string) => void
    ) {
        this._conn = new sr.HubConnectionBuilder().withUrl(url).build();
        this._conn.on("SetNowPlaying", setNowPlaying);
        this._conn.on("MusicEnqueued", musicEnqueued);
        this._conn.on("MusicDequeued", musicDequeued);
        this._conn.on("OnlineUserLogin", onlineUserLogin);
        this._conn.on("OnlineUserLogout", onlineUserLogout);
        this._conn.on("OnlineUserRename", onlineUserRename);
        this._conn.on("NewChat", newChat);
        this._conn.on("GlobalMessage", globalMessage);
        this._conn.on("Abort", abort);
    }
    public async start(): Promise<any> {
        if (this._conn.state === sr.HubConnectionState.Disconnected) {
            await this._conn.start();
            console.log("music hub: " + this._conn.state);
        }
    }
    public async enqueueMusic(id: string, apiName: string): Promise<void> {
        await this._conn.invoke("EnqueueMusic", id, apiName);
    }
    public async requestSetNowPlaying(): Promise<void> {
        await this._conn.invoke("RequestSetNowPlaying");
    }
    public async getMusicQueue(): Promise<{ music: music, enqueuerName: string }[]> {
        return await this._conn.invoke("GetMusicQueue");
    }
    public async nextSong(): Promise<void> {
        await this._conn.invoke("NextSong");
    }
    public async rename(newName: string): Promise<void> {
        await this._conn.invoke("Rename", newName);
    }
    public async getOnlineUsers(): Promise<{ id: string, name: string }[]> {
        return await this._conn.invoke("GetOnlineUsers");
    }
    public async chatSay(content: string): Promise<void> {
        await this._conn.invoke("ChatSay", content);
    }
}
export interface music {
    url: string;
    name: string;
    artists: string[];
}
