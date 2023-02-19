export async function getProfile(): Promise<User> {
  const resp = await fetch("/api/profile");
  const j = await resp.json();
  return { name: j["name"] };
}

export async function getMusicApis(): Promise<string[]> {
  const resp = await fetch("/api/musicservices");
  const j = await resp.json();
  return j;
}

export async function searchUsers(
  keyword: string,
  apiName: string
): Promise<MusicServiceUser[]> {
  const resp = await fetch(`/api/${apiName}/searchuser/${keyword}`);
  const j = await resp.json();
  return j;
}

export async function bindAccount(identifier: string, apiName: string) {
  return fetch(`/api/${apiName}/bind/${identifier}`);
}

export async function getBindInfo() {
  return (await fetch(`/api/bindinfo`)).json();
}

export async function getMyPlaylist(apiName: string): Promise<Playlist[]> {
  const resp = await fetch(`/api/${apiName}/myplaylists`);
  const j = await resp.json();
  if (!resp.ok) {
    const err = j as { code: number; message: string };
    if (err.code === 1) throw "UnknownApi";
    if (err.code === 2) throw "NeedBind";
  }
  return j;
}

export async function getMusicsByPlaylist(
  id: string,
  page: number,
  apiName: string
): Promise<Music[]> {
  const resp = await fetch(`/api/${apiName}/playlistmusics/${id}?page=${page}`);
  const j = await resp.json();
  return j;
}

export interface User {
  name: string;
}

export interface MusicServiceUser {
  identifier: string;
  name: string;
}

export interface Playlist {
  id: string;
  name: string;
}

export interface Music {
  id: string;
  name: string;
  artists: string[];
}
