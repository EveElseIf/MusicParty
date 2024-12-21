import express from "express";
import morgan from "morgan";
import cookieParser from "cookie-parser";
import ViteExpress from "vite-express";
// import * as trpcExpress from "@trpc/server/adapters/express";
import { appRouter, createContext } from "./router.js";
import { genId } from "./utils.js";
import { cfg } from "../common/config.js";

const app = express();

app.use(morgan("combined", {
  skip: (req, resp) => req.originalUrl.startsWith("/node_modules") || req.originalUrl.startsWith("/@")
}));

app.use(cookieParser());

const expdate = new Date('9999-12-31T23:59:59.999Z')
app.use((req, resp, next) => {
  if (!req.cookies["id"]) {
    resp.cookie("id", genId(), {
      expires: expdate,
    });
    resp.redirect(req.url);
    resp.end();
  } else {
    next();
  }
});

// app.use("/trpc", trpcExpress.createExpressMiddleware({
//   router: appRouter,
//   createContext: createContext,
// }));

app.get("/hello", (_, res) => {
  res.send("Hello Vite + React + TypeScript!");
});

ViteExpress.listen(app, cfg.app.port, () =>
  console.log("Server is listening on port 3000..."),
);
