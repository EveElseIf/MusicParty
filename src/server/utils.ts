import crypot from "crypto"

export function genId() {
    return crypot.randomBytes(16).toString("hex").toLowerCase();
}