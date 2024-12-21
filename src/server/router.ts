import { initTRPC, TRPCError } from "@trpc/server";
import * as trpcExpress from "@trpc/server/adapters/express";
import * as cookie from "cookie";
import { bilibiliProviderName, MusicProviderUserProfile, neteaseProviderName, Provider, Room } from '../common/lib/core.js';
import { z } from "zod";
import data from "./data.js";
import { genId } from "./utils.js";
import registry from "./registry.js";

export const createContext = async ({
    req,
    res,
}: trpcExpress.CreateExpressContextOptions) => {
    function getIdFromHeader() {
        if (req.headers.cookie) {
            const cookies = cookie.parse(req.headers.cookie)
            return cookies["id"] ?? null
        }
        return null
    }
    return {
        id: getIdFromHeader()
    };
};
type Context = Awaited<ReturnType<typeof createContext>>;

const t = initTRPC.context<Context>().create();

const authProcedure = t.procedure.use(({ ctx, next }) => {
    if (!ctx.id) {
        throw new TRPCError({ code: "UNAUTHORIZED" });
    }
    return next();
});

export const appRouter = t.router({
    getCurrentUser: authProcedure
        .query(async (opts) => {
            const id = opts.ctx.id!
            const result = await data.getUser(id)
            if (!result) {
                const user = { id, name: id.slice(0, 6) }
                await data.setUser(user)
                return user
            } else {
                return result
            }
        }),
    changeCurrentUserName: authProcedure
        .input(z.string())
        .mutation(async (opts) => {
            return await data.setUser({
                id: opts.ctx.id!,
                name: opts.input
            })
        }),
    getAvailableMusicProviders: authProcedure
        .query(async (opts) => {
            // TODO
            const names: Provider[] = [
                neteaseProviderName,
                bilibiliProviderName,
            ]
            return names
        }),
    getOnlineUsers: authProcedure
        .input(z.object({ roomId: z.string() }))
        .query(async (opts) => {
            return await data.getRoomUsers(opts.input.roomId)
        }),
    getRooms: authProcedure
        .query(async (opts) => {
            return await data.getRooms()
        }),
    getRoom: authProcedure
        .input(z.object({ roomId: z.string() }))
        .query(async (opts) => {
            return await data.getRoom(opts.input.roomId)
        }),
    createRoom: authProcedure
        .input(z.object({ name: z.string() }))
        .mutation(async (opts) => {
            const room: Room = {
                id: genId(),
                name: opts.input.name
            }
            return await data.addRoom(room)
        }),
    joinRoom: authProcedure
        .input(z.object({ roomId: z.string() }))
        .mutation(async (opts) => {
            const user = await data.getUser(opts.ctx.id!);
            if (!user) throw new TRPCError({ code: "NOT_FOUND" });
            await data.roomUserHeatbeat(opts.input.roomId, user);
            return await data.addRoomUser(opts.input.roomId, user)
        }),
    userRoomHeartBeat: authProcedure
        .input(z.object({ roomId: z.string() }))
        .mutation(async (opts) => {
            const user = await data.getUser(opts.ctx.id!);
            if (!user) throw new TRPCError({ code: "NOT_FOUND" });
            return await data.roomUserHeatbeat(opts.input.roomId, user);
        }),
    searchUserWithProvider: authProcedure
        .input(z.object({
            keyword: z.string(),
            offset: z.number().optional(),
            provider: z.string()
        }))
        .query(async (opts) => {
            const provider = registry.providers.get(opts.input.provider)
            if (!provider) throw new TRPCError({ code: "BAD_REQUEST" })
            return await provider.searchUser(opts.input.keyword, opts.input.offset ?? 0)
        }),
    bindCurrentUserWithProfile: authProcedure
        .input(z.object({
            provider: z.string(),
            id: z.string(),
            name: z.string()
        }))
        .mutation(async (opts) => {
            const user = await data.getUser(opts.ctx.id!);
            if (!user) throw new TRPCError({ code: "NOT_FOUND" });
            const profile = opts.input as MusicProviderUserProfile
            return await data.setUserProfile(user, profile)
        }),
});

// export type definition of API
export type AppRouter = typeof appRouter;