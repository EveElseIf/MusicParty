import { initTRPC, TRPCError } from "@trpc/server";
import * as trpcExpress from "@trpc/server/adapters/express";
import { MusicProviderUserProfile, Room } from '../common/lib/core.js';
import { z } from "zod";
import data from "./data.js";
import { genId } from "./utils.js";
import registry from "./registry.js";

const expdate = new Date('9999-12-31T23:59:59.999Z')

export const createContext = async ({
    req,
    res,
}: trpcExpress.CreateExpressContextOptions) => {
    if (!req.cookies["id"] || req.cookies["id"] === "") {
        const id = genId()
        await data.setUser({ id, name: id.slice(0, 6) })
        res.cookie("id", id, {
            expires: expdate,
        });
        res.redirect(req.url);
        res.end();
    }

    const id = req.cookies["id"] as string

    if (!id) {
        res.status(400)
        res.end()
    }

    if (!await data.checkUserExists(id)) {
        res.cookie("id", "", { maxAge: 0 })
        res.redirect(req.url)
        res.end()
    }

    return {
        id
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

const musicProviderProcedure = t.procedure.input(z.object({ providerName: z.string() })).use(({ ctx, next, input }) => {
    const provider = registry.providers.get(input.providerName)
    if (!provider) {
        throw new TRPCError({ code: "BAD_REQUEST", message: `provider ${input.providerName} not found` })
    }
    return next({
        ctx: {
            ...ctx,
            provider
        }
    })
})

export const appRouter = t.router({
    getCurrentUser: authProcedure
        .query(async (opts) => {
            const id = opts.ctx.id!
            const result = await data.getUser(id)
            if (!result) {
                throw new TRPCError({ code: "UNAUTHORIZED" });
            }
            return result;
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
            return registry.providers.values().map(x => x.provider).toArray()
        }),
    getOnlineUsers: authProcedure
        .input(z.object({ roomId: z.string() }))
        .query(async (opts) => {
            return await data.getRoomUserNames(opts.input.roomId)
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
            await data.roomUserHeatbeat(opts.input.roomId, opts.ctx.id!);
            return await data.addRoomUser(opts.input.roomId, opts.ctx.id!)
        }),
    userRoomHeartBeat: authProcedure
        .input(z.object({ roomId: z.string() }))
        .mutation(async (opts) => {
            return await data.roomUserHeatbeat(opts.input.roomId, opts.ctx.id!);
        }),
    searchUserWithProvider: authProcedure
        .input(z.object({
            keyword: z.string(),
            offset: z.number().optional(),
            provider: z.string()
        }))
        .query(async (opts) => {
            const provider = registry.providers.get(opts.input.provider)
            if (!provider) throw new TRPCError({ code: "BAD_REQUEST", message: "invalid provider: " + opts.input.provider })
            return await provider.searchUser(opts.input.keyword, opts.input.offset ?? 0)
        }),
    bindCurrentUserWithProfile: authProcedure
        .input(z.object({
            provider: z.string(),
            id: z.string(),
            name: z.string()
        }))
        .mutation(async (opts) => {
            const profile = opts.input as MusicProviderUserProfile
            return await data.setUserProfile(opts.ctx.id!, profile)
        }),
    enqueueMusic: musicProviderProcedure
        .input(z.object({ targetId: z.string() }))
        .mutation(async (opts) => {
            const music = await opts.ctx.provider.getMusicById(opts.input.targetId);
            await registry.playmanager.enqueueMusic(music);
        }),
});

// export type definition of API
export type AppRouter = typeof appRouter;