import { initTRPC, TRPCError } from "@trpc/server";
import * as trpcExpress from "@trpc/server/adapters/express";
import * as cookie from "cookie";
import { ProviderName } from '../common/lib/core.js';
import { neteaseProviderName } from '../common/lib/netease/index.js';
import { bilibiliProviderName } from '../common/lib/bilibili/index.js';

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

const authProcedure = t.procedure.use(async ({ ctx, next }) => {
    if (!ctx.id) {
        throw new TRPCError({ code: "UNAUTHORIZED" });
    }
    return next();
});

export const appRouter = t.router({
    getCurrentUser: authProcedure
        .query(async (opts) => {
            return { id: opts.ctx.id, name: "test1" };
        }),
    getAvailableMusicProviders: authProcedure
        .query(async (opts) => {
            const names: ProviderName[] = [
                neteaseProviderName,
                bilibiliProviderName,
            ]
            return names
        }),
});

// export type definition of API
export type AppRouter = typeof appRouter;