import yaml from "js-yaml"
import fs from "node:fs"

type Config = {
    app: {
        port: number
    },
    redis: {
        url: string
    },
    provider: {
        name: "netease" | "bilibili"
        cookie: string
        [key: string]: string
    }[]
}


const file = (process.env.NODE_ENV !== "production" ? "config.local.yaml" : "config.yaml");

const content = fs.readFileSync(file).toString()
const cfg = yaml.load(content) as Config

export { cfg, type Config }