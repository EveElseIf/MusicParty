import yaml from "js-yaml"
import fs from "node:fs"

type Config = {
    app: {
        port: number
    },
    redis: {
        url: string
    },
    providers: {
        name: "netease" | "bilibili"
        cookie: string
        [key: string]: string
    }[]
}


const file = (process.env.NODE_ENV !== "production" ? "config.local.yaml" : "config.yaml");

const content = fs.readFileSync(file).toString()
const _cfg = yaml.load(content) as Config
const cfg = _cfg

export { cfg, type Config }