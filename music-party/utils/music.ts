import * as sr from "@microsoft/signalr"
export class connection {
    private _conn: sr.HubConnection;
    private _refresh: any;
    constructor(url: string, fn: any, refresh: any) {
        this._conn = new sr.HubConnectionBuilder().withUrl(url).build();
        this._conn.on("SetPlaying", fn);
        this._refresh = refresh;
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
}
export interface music {
    url: string;
    name: string;
    artist: string;
}