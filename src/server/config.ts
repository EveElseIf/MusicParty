import yaml from "js-yaml"
import fs from "node:fs"
import { Provider } from "../common/lib/core.js"

type Config = {
    app: {
        port: number
    },
    redis: {
        url: string
    },
    providers: {
        name: Provider
        cookie: string
        [key: string]: string
    }[]
}

let content = ""

if (process.env.CONFIG_B64) {
    const data = process.env.CONFIG_B64
    content = atob(data)
} else {
    const file = (process.env.NODE_ENV !== "production" ? "config.local.yaml" : "config.yaml");
    content = fs.readFileSync(file).toString()
}
const _cfg = yaml.load(content) as Config
const cfg = _cfg

if (cfg.providers.length === 0) {
    console.error("you need to set at least one music provider")
    process.exit(-1)
}

export { cfg, type Config }