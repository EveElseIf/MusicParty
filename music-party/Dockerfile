FROM node:slim
WORKDIR /
COPY . .
RUN npm install -g pnpm
RUN pnpm install
RUN pnpm build
ENTRYPOINT [ "pnpm" , "start" ]
