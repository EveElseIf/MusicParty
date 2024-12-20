import { initTRPC, TRPCError } from "@trpc/server";
import * as trpcExpress from "@trpc/server/adapters/express";
import * as cookie from "cookie";
import { ProviderName, Room } from '../common/lib/core.js';
import { neteaseProviderName } from '../common/lib/netease/index.js';
import { bilibiliProviderName } from '../common/lib/bilibili/index.js';
import { z } from "zod";
import data from "./data.js";
import { genId } from "./utils.js";

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

const loggingMiddleware = t.middleware(async ({ path, type, next }) => {
    try {
        const result = await next();
        return result;
    } catch (error) {
        console.error(`❌ [${type}] ${path} - Error occurred:`, error);
        throw error;
    }
})

const authProcedure = t.procedure.use(loggingMiddleware).use(({ ctx, next }) => {
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
            const names: ProviderName[] = [
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
});

// export type definition of API
export type AppRouter = typeof appRouter;