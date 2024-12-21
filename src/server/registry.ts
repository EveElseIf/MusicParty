import { exit } from "process";
import { cfg } from "../common/config.js";
import NeteaseMusicProvider from "../common/lib/netease/index.js";
import { bilibiliProviderName, MusicProvider, neteaseProviderName } from "../common/lib/core.js";
import BilibiliMusicProvider from "../common/lib/bilibili/index.js";

let _providers: Map<string, MusicProvider> = new Map()

cfg.providers.map(x => {
    switch (x.name) {
        case "netease":
            _providers = _providers.set(neteaseProviderName, new NeteaseMusicProvider(x.cookie))
            break
        case "bilibili":
            _providers = _providers.set(bilibiliProviderName, new BilibiliMusicProvider())
            break
        default:
            console.error(`unsupported provider, name: ${x.name}`)
            exit(-1)
    }
})
const providers = _providers

export default {
    providers
}