import { Box, Button, FormControl, Input, InputLabel, MenuItem, Select, Typography } from "@mui/material";
import { trpc } from "../utils/trpc.js";
import { useState } from "react";
import type { Provider } from "../../common/lib/core.js";
import { enqueueSnackbar } from "notistack";

export function EnqueueMusicByIdView() {
    const providersQuery = trpc.getAvailableMusicProviders.useQuery()
    const enqueueMusicMutation = trpc.enqueueMusic.useMutation()
    const [provider, setProvider] = useState<"" | Provider>("")
    const [id, setId] = useState("")
    return <>
        <Box display={"flex"} flexDirection={"column"} gap={"1rem"}>
            <FormControl>
                <InputLabel id="label1">音乐提供者</InputLabel>
                <Select labelId="label1" label={"音乐提供者"} value={provider} onChange={(e) => {
                    setProvider(e.target.value as Provider)
                }}>
                    {
                        providersQuery.data?.map(x => <MenuItem key={x} value={x}>
                            {x}
                        </MenuItem>)
                    }
                </Select>
            </FormControl>
            <Box display={"flex"} gap={"0.5rem"}>
                <Input sx={{ flex: 1 }} placeholder="音乐ID" value={id} onChange={(e) => {
                    setId(e.target.value)
                }} />
                <Button variant="contained" onClick={() => {
                    if (provider === "") {
                        enqueueSnackbar("请先选择音乐提供者", { variant: "info" })
                        return
                    }
                    if (id === "") {
                        return
                    }
                    enqueueMusicMutation.mutateAsync({
                        targetId: id,
                        providerName: provider
                    }).then(() => {
                        enqueueSnackbar("点歌成功", { variant: "success" })
                    }).catch(err => {
                        enqueueSnackbar(err, { variant: "error" })
                    })
                }}>
                    点歌
                </Button>
            </Box>
        </Box>
    </>
}