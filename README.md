# Music Party
## No Chinese?
别急，主要是因为输代码懒得切中文，所以有空再写。
## How to Build
This repo contains the front-end project and the back-end projects (including music api projects).
### Requirements
- .NET SDK 6.0
- nodejs with pnpm package manager
### Front End Building
The front-end project is in the "music-party" folder. You need to use pnpm to build it.

`cd music-party && pnpm install && pnpm build`
### Back End Building
The back-end project is in the "MusicParty" folder.
You need .NET SDK 6.0 to build it.
`cd MusicParty && dotnet build`
## How to Use
Read this part after you know how to build the projects.
### Front End
`cd music-party && pnpm start`
### Music Api
Now, there are Netease Cloud Music, QQ Music and Bilibili api supports in this repo.

Edit settings in MusicParty/appsettings.json to specify which music api you want to enable, turn the MusicApi.XXX.Enabled to false if you don't want to use it.

For Netease Cloud Music and QQ Music, you need the cookie of a certain account to access the music services. Or for Netease Cloud Music, you can use phone number and password instead. For Bilibili, you need SESSDATA, just part of the cookie.

If you want to use Netease Cloud Music API, start a server by running `npx NeteaseCloudMusicApi@latest`.

If you want to use QQ Music API, follow this [instruction](https://github.com/jsososo/QQMusicApi) to start a server.

You can write your own music api adapter, just implement the interface in MusicApiContract/IMusicApi.cs and register it in MusicParty/Program.cs.
### Back End
Provide your front-end server url and music api server urls in MusicParty/appsettings.json and run `cd MusicParty && dotnet run -c release`.
## Use Docker
I strongly recommend you to use this method to deloy this repo.
1. Clone this repo, and complete the settings in docker-compose.yml. You can disable music apis which you don't want to use. If you want to use a certain music api, you should provide the corresponding cookie or some other credential. You can't start the project without any music api.
2. Run `docker compose up`. Notice that the default port is "2706", you can modify it in docker-compose.yml.