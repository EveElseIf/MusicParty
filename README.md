# Music Party
## How to Build
This repo contains the front-end and the back-end projects.
### Front End Building
The front-end project is in the "music-party" folder. You need to use pnpm to build it.

`pnpm install && pnpm build`

Or you can develop it by using

`pnpm dev`
### Back End Building
The back-end project is in the "MusicParty" folder. Before using it, you should complete some setup.
- Start a [NeteaseCloudMusicApi](https://github.com/Binaryify/NeteaseCloudMusicApi) server.
- Edit MusicParty/appsettings.json to complete the configuration.

After that, use .NET SDK 6.0 to build it.

`cd MusicParty && dotnet build`

Or you can run it by using

`dotnet run`