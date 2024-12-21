import React, { useCallback, useEffect, useRef, useState } from "react";
import { Alert, Box, Button, Card, CardActions, CardContent, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Input, Popover, Tab, Typography, useTheme } from "@mui/material";
import { TabContext, TabList, TabPanel } from "@mui/lab";
import { trpc } from "../utils/trpc";
import { useNavigate, useParams } from "react-router";
import { enqueueSnackbar } from "notistack";
import { neteaseProviderName, bilibiliProviderName } from "../../common/lib/core";

const defaultCatch = (err: any) => {
    enqueueSnackbar(err, { variant: "error" })
}

const heartbeatIntervalms = 40 * 1000

export function Room() {
    const theme = useTheme()
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
                    }, heartbeatIntervalms)
                })
                .catch(err => enqueueSnackbar(err, { variant: "error" }))
        }
        return () => {
            if (intervalId)
                clearInterval(intervalId)
        }
    }, [roomQuery.isSuccess])
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

    const BindProfile = useCallback(({ type, button }: { type: string, button: (onClick: () => void) => React.ReactNode }) => {
        const [open, setOpen] = useState(false)
        const providerName = (() => {
            if (type === neteaseProviderName) return "网易云"
            else if (type === bilibiliProviderName) return "哔哩哔哩"
        })()
        const [keyword, setKeyword] = useState("")
        const [offset, setOffset] = useState(0)
        const query = trpc.searchUserWithProvider.useQuery({
            keyword,
            offset,
            provider: type
        }, {
            enabled: keyword !== ""
        })
        const [input, setInput] = useState("")
        const bindMutation = trpc.bindCurrentUserWithProfile.useMutation()
        return <>
            {button(() => { setOpen(true) })}
            <Dialog open={open} onClose={() => setOpen(false)}>
                <DialogTitle>搜索并绑定你的账号</DialogTitle>
                <DialogContent>
                    <Box>
                        <Input placeholder={`你的${providerName}用户名`} value={input} onChange={(e) => setInput(e.target.value)} />
                        <Button onClick={() => {
                            setKeyword(input)
                        }}>搜索</Button>
                    </Box>
                    <Box display={"flex"} flexDirection={"column"}>
                        {
                            query.data?.map((x) => <Box key={x.id} display={"flex"} flexDirection={"row"} width={"100%"}>
                                <Box alignContent={"center"} flex={1}>{x.name}</Box>
                                <Button onClick={() => {
                                    bindMutation.mutateAsync(x)
                                        .then(res => {
                                            setOpen(false)
                                            enqueueSnackbar("绑定成功", { variant: "success" })
                                        }).catch(defaultCatch)
                                }}>绑定</Button>
                            </Box>)
                        }
                        {
                            query.data && <>
                                <Box display={"flex"} flexDirection={"row"} justifyContent={"space-around"}>
                                    <Button onClick={() => {
                                        if (offset >= 1) setOffset(o => o - 1)
                                    }}>
                                        上一页
                                    </Button>
                                    <Button onClick={() => {
                                        setOffset(o => o + 1)
                                    }}>
                                        下一页
                                    </Button>
                                </Box>
                            </>
                        }
                        {
                            query.isLoading && <Box display={"flex"} justifyContent={"center"} paddingY={".5rem"}>
                                <CircularProgress />
                            </Box>
                        }
                        {
                            query.isError && <Alert color="error">
                                错误：{query.error.message}
                            </Alert>
                        }
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => { setOpen(false) }}>
                        关闭
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    }, [])

    return <>
        <Box sx={{
            display: "flex",
            [theme.breakpoints.down("md")]: {
                flexDirection: "column",
            },
            flexDirection: "row"
        }}>
            <Box sx={{
                [theme.breakpoints.down("md")]: {
                    width: "100%",
                    maxWidth: "100%"
                },
                width: "30%",
                maxWidth: theme.breakpoints.values.sm,
            }}>
                <Box sx={{
                    display: "flex",
                    flexDirection: "column",
                    gap: ".5rem",
                    margin: ".5rem"
                }}>
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
                                        return <BindProfile key={p} type={p}
                                            button={(onSubmit) =>
                                                <Button onClick={onSubmit} variant="contained" fullWidth color="error">
                                                    绑定网易云音乐账号
                                                </Button>
                                            }
                                        />
                                    } else if (p === bilibiliProviderName) {
                                        return <BindProfile key={p} type={p}
                                            button={(onSubmit) =>
                                                <Button onClick={onSubmit} variant="contained" fullWidth color="info">
                                                    绑定Bilibili账号
                                                </Button>}
                                        />
                                    }
                                })
                            }
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: ".5rem"
                        }}>
                            <Typography variant="h4" fontWeight={"bold"} marginBottom={"1rem"}>
                                在线
                            </Typography>
                            <ul>
                                {
                                    usersQuery.data?.map(x => <li key={x.id}>
                                        {x.name}
                                    </li>)
                                }
                            </ul>
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