import { Box, Button, Input } from "@mui/material"
import { trpc } from "../utils/trpc"
import { useNavigate } from "react-router"
import { useState } from "react"
import { enqueueSnackbar } from "notistack"

function CreateRoomModal({ onCreated }: { onCreated?: () => void }) {
    const createRoomMutation = trpc.createRoom.useMutation()
    const [name, setName] = useState("")
    return <>
        <Box display={"flex"}>
            <span>
                name: <Input onChange={(e) => setName(e.target.value)} />
            </span>
            <Button onClick={() => {
                if (name === "") return
                createRoomMutation.mutateAsync({ name }).then(res => {
                    onCreated?.()
                }).catch((err) => {
                    enqueueSnackbar(err, { variant: "error" })
                })
            }}>Create</Button>
        </Box>
    </>
}

export function Home() {
    const roomQuery = trpc.getRooms.useQuery()
    const nav = useNavigate()
    return <>
        <Box display={"flex"} flexDirection={"column"}>
            {
                roomQuery.data?.map(x => <Box key={x.id}>
                    <Button variant="text" onClick={() => {
                        nav(`/room/${x.id}`)
                    }}>
                        {x.name}
                    </Button>
                </Box>)
            }
        </Box>
        <CreateRoomModal onCreated={() => {
            roomQuery.refetch()
        }} />
    </>
}