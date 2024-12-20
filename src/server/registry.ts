import { exit } from "process";
import { cfg } from "../common/config.js";
import NeteaseMusicProvider from "../common/lib/netease/index.js";
import { MusicProvider } from "../common/lib/core.js";
import BilibiliMusicProvider from "../common/lib/bilibili/index.js";

let _providers: MusicProvider[] = []

cfg.provider.map(x => {
    switch (x.name) {
        case "netease":
            _providers = _providers.concat(new NeteaseMusicProvider(x.cookie))
            break
        case "bilibili":
            _providers = _providers.concat(new BilibiliMusicProvider())
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