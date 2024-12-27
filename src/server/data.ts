import { Redis } from "ioredis"
import { cfg } from "../common/config.js"
import { MusicProviderUserProfile, Provider, Room, User } from "../common/lib/core.js";
import { exit } from "process";

const r = new Redis(cfg.redis.url);

const keys = {

    user: {
        info: (id: string) => `user:${id}:info`, // hash
    },

    room: {
        rooms: "room:rooms", // id set
        info: (roomId: string) => `room:${roomId}:info`, // hash
        users: (roomId: string) => `room:${roomId}:users`, // id set
    },

    heartbeat: {
        "roomUsers": "hb:room-users", // hearbeats to clean user from room, zset "roomId-userId" with ts sec
    },

    mutex: {
        clean: { // cleaning tasks
            heartbert: "mx:clean:hb", // clean heartbeat expired objects
        }
    }
}

async function cleanHeartbeat() {
    const ok = await r.set(keys.mutex.clean.heartbert, 1, "EX", 60, "NX") === "OK"
    if (!ok) {
        return
    }

    const cleanRoomUser = async () => {
        const heartbeats = await r.zrangebyscore(keys.heartbeat.roomUsers, "-inf", Math.floor(Date.now() / 1000))
        for (const hb of heartbeats) {
            const [roomId, userId] = hb.split("-")
            const user = await data.getUser(userId)
            if (!user) continue
            await data.removeRoomUser(roomId, user.id)
            await r.zrem(keys.heartbeat.roomUsers, hb)
        }
    }

    try {
        await cleanRoomUser()
    } catch (ex) {
        console.warn(ex)
    }

    await r.del(keys.mutex.clean.heartbert)
}

r.ping().then(_ => {
    console.log("Redis connected")
    setInterval(cleanHeartbeat, 30 * 1000)
}).catch(err => {
    console.error(err)
    exit(-1)
})

const data = {
    async getUser(id: string): Promise<User | null> {
        const k = keys.user.info(id)
        const ret = await r.hgetall(k)
        return ret as unknown as User
    },
    async setUser(user: User) {
        const k = keys.user.info(user.id)
        return await r.hset(k, user)
    },
    async checkUserExists(id: string) {
        const k = keys.user.info(id)
        return await r.exists(k) == 1
    },
    async addRoom(room: Room) {
        const k = keys.room.info(room.id)
        return await r.hset(k, room) + await r.sadd(keys.room.rooms, room.id);
    },
    async getRoom(roomId: string): Promise<Room | null> {
        const k = keys.room.info(roomId)
        const ret = await r.hgetall(k)
        return ret as unknown as Room
    },
    async getRooms(): Promise<Room[]> {
        const k = keys.room.rooms
        const ids = await r.smembers(k)
        const ret = []
        for (const id of ids) {
            const room = await this.getRoom(id)
            if (room !== null) {
                ret.push(room)
            }
        }
        return ret
    },
    async getRoomUserNames(roomId: string): Promise<string[]> {
        const k = keys.room.users(roomId)
        const ids = await r.smembers(k)
        const pl = r.pipeline()
        ids.forEach(x => pl.hget(keys.user.info(x), "name"))
        const result = await pl.exec()
        const ret = []
        for (const res of result!) {
            if (!!res[0]) {
                console.error(res[0])
            } else {
                ret.push(res[1] as string)
            }
        }
        return ret
    },
    async addRoomUser(roomId: string, userId: string) {
        // const k = keys.roomUsers(roomId)
        // return await r.sadd(k, userId)
        const k = keys.room.users(roomId)
        return await r.sadd(k, userId)
    },
    async removeRoomUser(roomId: string, userId: string) {
        const k = keys.room.users(roomId)
        return await r.srem(k, userId)
    },
    async roomUserHeatbeat(roomId: string, userId: string) {
        const k = keys.heartbeat.roomUsers
        return await r.zadd(k, Math.floor(Date.now() / 1000) + 60, `${roomId}-${userId}`)
    },
    async setUserProfile(userId: string, profile: MusicProviderUserProfile) {
        const k = keys.user.info(userId)
        return await r.hset(k, profile.provider, JSON.stringify(profile))
    },
    async getUserProfile(userId: string, provider: Provider): Promise<MusicProviderUserProfile | null> {
        const k = keys.user.info(userId)
        const result = await r.hget(k, provider)
        if (!result) return null
        else return result as unknown as MusicProviderUserProfile
    }
}

export default data