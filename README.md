# Music Party
## How to Build
This repo contains the front-end and the back-end projects.
### Front End Building
The front-end project is in the "music-party" folder. You need to use pnpm to build it.

`cd music-party && pnpm install && pnpm build`
### Back End Building
The back-end project is in the "MusicParty" folder.
You need .NET SDK 6.0 to build it.
`cd MusicParty && dotnet build`
## How to Use
### Front End
Run `pnpm start`.
### Music Api
This repo contains a Netease Cloud Music api adapter. If you want to use it, please complete the music api settings in MusicParty/appsettings.json. Then, start a Netease Cloud Music Api Server by `npx NeteaseCloudMusicApi@latest`.

You can build your own music api adapter, please read the additional documents.
### Back End
Complete front end url and music api settings in MusicParty/appsettings.json and run `dotnet run -c release`.
## Use Docker
1. Clone this repo, then just only complete the Music Api account settings in MusicParty/appsettings.json. For example, the phone number of your Netease Cloud Music account.
2. Run `docker compose up`. Notice that the default port is "2706", you can modify it in docker-compose.yml.