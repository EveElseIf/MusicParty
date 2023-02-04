import { Text, Skeleton, Stack, Accordion, useToast, Flex, Select } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import * as api from "../api/api";
import { toastError } from "../utils/toast";
import { Playlist } from "./playlist";

export const MyPlaylist = (props: { apis: string[], enqueue: (id: string, apiName: string) => void }) => {
    const [canshow, setCanshow] = useState(false);
    const [playlists, setPlaylists] = useState<api.Playlist[]>([]);
    const [needBind, setNeedBind] = useState(false);
    const [apiName, setApiName] = useState("NeteaseCloudMusic");
    const t = useToast();

    useEffect(() => {
        setApiName(props.apis[0]);
    }, [props.apis])

    useEffect(() => {
        api.getMyPlaylist(apiName).then(resp => {
            setPlaylists(resp);
            setCanshow(true);
        }).catch(err => {
            if (err === "NeedBind") {
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
                    : <>
                        <Flex flexDirection={"row"} alignItems={"center"} mb={4}>
                            <Text>
                                Api Provider
                            </Text>
                            <Select ml={2} flex={1} onChange={e => {
                                setApiName(e.target.value);
                            }} >
                                {props.apis.map(a => {
                                    return <option key={a} value={a}>
                                        {a}
                                    </option>;
                                })}
                            </Select>
                        </Flex>
                        <Accordion allowMultiple>
                            {playlists.map(p =>
                                <Playlist key={p.id} id={p.id} name={p.name} apiName={apiName} enqueue={props.enqueue} />
                            )}
                        </Accordion>
                    </>
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