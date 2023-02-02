export async function getProfile(): Promise<User> {
    const resp = await fetch("/api/profile");
    const j = await resp.json();
    return { name: j["name"] };
}

export async function searchNeteaseUsers(keyword: string): Promise<NeteaseUser[]> {
    const resp = await fetch(`/api/searchuser/${keyword}`);
    const j = await resp.json();
    return j;
}

export async function bindNeteaseAccount(uid: string) {
    return fetch(`/api/bind/${uid}`);
}

export async function getMyPlaylist(): Promise<Playlist[]> {
    const resp = await fetch("/api/myplaylists");
    if (resp.status === 400) throw "400";
    const j = await resp.json();
    return j;
}

export async function getMusicsByPlaylist(id: string, page: number): Promise<Music[]> {
    const resp = await fetch(`/api/playlistmusics/${id}?page=${page}`);
    const j = await resp.json();
    return j;
}

export interface User {
    name: string;
}

export interface NeteaseUser {
    uid: string,
    name: string
}

export interface Playlist {
    id: string;
    name: string;
}

export interface Music {
    id: string,
    name: string,
    artist: string
}