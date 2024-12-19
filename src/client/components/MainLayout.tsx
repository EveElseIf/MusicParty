import { useState } from "react";
import { Box, Button, Card, CardContent, Tab, Typography } from "@mui/material";
import { TabContext, TabList, TabPanel } from "@mui/lab";
import { trpc } from "../utils/trpc";
import { neteaseProviderName } from "../../common/lib/netease";
import { bilibiliProviderName } from "../../common/lib/bilibili";

export default function () {
    const userQuery = trpc.getCurrentUser.useQuery();
    const providerQuery = trpc.getAvailableMusicProviders.useQuery();
    const [tab, setTab] = useState(0)

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
                            <Button variant="contained" fullWidth>
                                修改名字
                            </Button>
                            {
                                providerQuery.data?.map((p) => {
                                    if (p === neteaseProviderName) {
                                        return <Button variant="contained" onClick={() => {

                                        }} fullWidth color="error">
                                            绑定网易云音乐账号
                                        </Button>
                                    } else if (p === bilibiliProviderName) {
                                        return <Button variant="contained" onClick={() => {

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