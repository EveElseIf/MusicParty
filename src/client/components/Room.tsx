import { useCallback, useEffect, useRef, useState } from "react";
import { Box, Button, Card, CardActions, CardContent, CardHeader, Input, Popover, Tab, Typography } from "@mui/material";
import { TabContext, TabList, TabPanel } from "@mui/lab";
import { trpc } from "../utils/trpc";
import { neteaseProviderName } from "../../common/lib/netease";
import { bilibiliProviderName } from "../../common/lib/bilibili";
import { useNavigate, useParams } from "react-router";
import { enqueueSnackbar } from "notistack";

const defaultCatch = (err: any) => {
    enqueueSnackbar(err, { variant: "error" })
}

export default function () {
    const { roomId } = useParams() as { roomId: string }
    const nav = useNavigate()
    const roomQuery = trpc.getRoom.useQuery({ roomId })
    if (roomQuery.status === "error" || (roomQuery.status === "success" && !roomQuery.data)) {
        enqueueSnackbar(roomQuery.error?.message ?? "room does not exists", { variant: "error" })
        nav("/")
    }
    const userQuery = trpc.getCurrentUser.useQuery();
    const usersQuery = trpc.getOnlineUsers.useQuery({ roomId }, { enabled: roomQuery.isSuccess && !!roomQuery.data });
    const providerQuery = trpc.getAvailableMusicProviders.useQuery();
    const joinMutation = trpc.joinRoom.useMutation();
    const heartbeatMutation = trpc.userRoomHeartBeat.useMutation();
    useEffect(() => {
        console.log("Hello World")
        let intervalId: any = null
        if (roomQuery.isSuccess && !!roomQuery.data) {
            joinMutation.mutateAsync({ roomId })
                .then(() => {
                    intervalId = setInterval(() => {
                        heartbeatMutation.mutateAsync({ roomId });
                    }, 20 * 1000)
                })
                .catch(err => enqueueSnackbar(err, { variant: "error" }))
        }
        return () => {
            if (intervalId)
                clearInterval(intervalId)
        }
    }, [roomQuery.data])
    const changeNameMutation = trpc.changeCurrentUserName.useMutation();
    const [tab, setTab] = useState(0)

    const ChangeNameButton = useCallback(() => {
        const ref = useRef(null)
        const [open, setOpen] = useState(false)
        const [name, setName] = useState("")
        return <>
            <Button ref={ref} onClick={() => setOpen(true)} variant="contained" fullWidth>
                修改名字
            </Button>
            <Popover open={open} onClose={() => setOpen(false)}
                anchorEl={ref.current}
                anchorOrigin={{
                    vertical: 'bottom',
                    horizontal: 'center',
                }}>
                <Card>
                    <CardContent>
                        <Input placeholder="输入新名字" value={name} onChange={(e) => setName(e.target.value)} />
                    </CardContent>
                    <CardActions>
                        <Button onClick={() => {
                            if (name === "") return
                            setOpen(false)
                            setName("")
                            changeNameMutation.mutateAsync(name)
                                .then(() => {
                                    enqueueSnackbar("修改成功", { variant: "success" })
                                    userQuery.refetch()
                                }).catch(defaultCatch)
                        }}>确认</Button>
                    </CardActions>
                </Card>
            </Popover>
        </>

    }, [])

    return <>
        <Box display={"flex"} flexDirection={"row"}>
            <Box display={"flex"} flexDirection={"column"} sx={{
                width: "30%",
                maxWidth: (t) => t.breakpoints.values.sm,
            }}>
                <Box display={"flex"} flexDirection={"column"} margin={"1rem"}>
                    <Card>
                        <CardContent sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: ".5rem"
                        }}>
                            <Typography variant="h4" fontWeight={"bold"} marginBottom={"1rem"}>
                                欢迎，{userQuery.data?.name}
                            </Typography>
                            <ChangeNameButton />
                            {
                                providerQuery.data?.map((p) => {
                                    if (p === neteaseProviderName) {
                                        return <Button key={p} variant="contained" onClick={() => {

                                        }} fullWidth color="error">
                                            绑定网易云音乐账号
                                        </Button>
                                    } else if (p === bilibiliProviderName) {
                                        return <Button key={p} variant="contained" onClick={() => {

                                        }} fullWidth color="info">
                                            绑定Bilibili账号
                                        </Button>
                                    }
                                })
                            }
                        </CardContent>
                    </Card>
                </Box>
            </Box>
            <Box display={"flex"} flexDirection={"column"} flex={1}>
                <TabContext value={tab}>
                    <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                        <TabList onChange={(_, t) => { setTab(t) }}>
                            <Tab label="播放列表" value={0} />
                            <Tab label="从音乐ID点歌" value={1} />
                            <Tab label="从歌单点歌" value={2} />
                        </TabList>
                    </Box>
                    <TabPanel value={0}>Item One</TabPanel>
                    <TabPanel value={1}>Item Two</TabPanel>
                    <TabPanel value={2}>Item Three</TabPanel>
                </TabContext>
            </Box>
        </Box>
    </>
}