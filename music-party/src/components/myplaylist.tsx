import { Text, Skeleton, Stack, Accordion, useToast } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import * as api from "../api/api";
import { toastError } from "../utils/toast";
import { Playlist } from "./playlist";

export const MyPlaylist = (props: { enqueue: (id: string) => void }) => {
    const [canshow, setCanshow] = useState(false);
    const [playlists, setPlaylists] = useState<api.Playlist[]>([]);
    const [needBind, setNeedBind] = useState(false);
    const t = useToast();

    useEffect(() => {
        api.getMyPlaylist().then(resp => {
            setPlaylists(resp);
            setCanshow(true);
        }).catch(err => {
            if (err === "400") {
                setNeedBind(true);
                setCanshow(true);
            }
            else
                toastError(t, err);
        });
    }, []);

    return (<Stack>
        {canshow ?
            (
                needBind ? <Text>
                    Please bind your Netease account first!
                    After that, refresh this page.
                </Text>
                    :
                    <Accordion allowMultiple>
                        {playlists.map(p =>
                            <Playlist key={p.id} id={p.id} name={p.name} enqueue={props.enqueue} />
                        )}
                    </Accordion>
            )
            : <>
                <Skeleton height='20px' />
                <Skeleton height='20px' />
                <Skeleton height='20px' />
                <Skeleton height='20px' />
                <Skeleton height='20px' />
                <Skeleton height='20px' />
            </>}
    </Stack >)
}