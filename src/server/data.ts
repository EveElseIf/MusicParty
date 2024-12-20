import { Redis } from "ioredis"
import { cfg } from "../common/config.js"
import { Room, User } from "../common/lib/core.js";
import { exit } from "process";

const r = new Redis(cfg.redis.url);

const keys = {
    user: (id: string) => `user.${id}`, // single user, hash id->user
    rooms: "room.rooms", // rooms, hash id->name
    roomUsers: (roomId: string) => `room.${roomId}.users`, // users in room, hash id->name

    heartbeat: {
        "roomUsers": "hb.room.users", // hearbeats to clean user from room, zset "roomId.userId" with UTC sec
    },

    mutex: {
        clean: { // cleaning tasks
            heartbert: "mx.clean.hb", // clean heartbeat expired objects
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
            const [roomId, userId] = hb.split(".")
            const user = await data.getUser(userId)
            if (!user) continue
            await data.removeRoomUser(roomId, user)
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
        const k = keys.user(id)
        const ret = await r.hgetall(k);
        if (Object.keys(ret).length === 0) return null
        return ret as unknown as User
    },
    async setUser(user: User): Promise<number> {
        const k = keys.user(user.id)
        return await r.hset(k, user)
    },
    async addRoom(room: Room): Promise<number> {
        const k = keys.rooms
        return await r.hset(k, [room.id, room.name])
    },
    async getRoom(roomId: string): Promise<Room | null> {
        const k = keys.rooms
        const name = await r.hget(k, roomId)
        if (!name) return null
        return {
            id: roomId,
            name
        }
    },
    async getRooms(): Promise<Room[]> {
        const k = keys.rooms
        const result = await r.hgetall(k)
        return Object.entries(result).map(x => ({ id: x[0], name: x[1] }))
    },
    async getRoomUsers(roomId: string): Promise<User[]> {
        const k = keys.roomUsers(roomId)
        const result = await r.hgetall(k)
        return Object.entries(result).map(x => ({ id: x[0], name: x[1] }))
    },
    async addRoomUser(roomId: string, user: User): Promise<number> {
        const k = keys.roomUsers(roomId)
        return await r.hset(k, [user.id, user.name])
    },
    async removeRoomUser(roomId: string, user: User): Promise<number> {
        const k = keys.roomUsers(roomId)
        return await r.hdel(k, user.id)
    },
    async roomUserHeatbeat(roomId: string, user: User): Promise<number> {
        const k = keys.heartbeat.roomUsers
        return await r.zadd(k, Math.floor(Date.now() / 1000) + 60, `${roomId}.${user.id}`)
    }
}

export default data