import {
  Text,
  Skeleton,
  Stack,
  Accordion,
  useToast,
  Flex,
  Select,
} from "@chakra-ui/react";
import { useEffect, useState } from "react";
import * as api from "../api/api";
import { toastError } from "../utils/toast";
import { Playlist } from "./playlist";

export const MyPlaylist = (props: {
  apis: string[];
  enqueue: (id: string, apiName: string) => void;
}) => {
  const [canshow, setCanshow] = useState(false);
  const [playlists, setPlaylists] = useState<api.Playlist[]>([]);
  const [needBind, setNeedBind] = useState(false);
  const [apiName, setApiName] = useState("");
  const [playlistCache, setPlaylistCache] = useState<
    Map<string, api.Playlist[]>
  >(new Map<string, api.Playlist[]>());
  const [someHook, setSomeHook] = useState(0);
  const [apis, setApis] = useState<string[]>([]);
  const t = useToast();

  useEffect(() => {
    api.getBindInfo().then((info: { key: string; value: string }[]) => {
      setApis(info.map((x) => x.key));
      if (info.length > 0) {
        const defaultApi = info[0].key;
        setApiName(defaultApi);
      } else {
        setNeedBind(true);
        setCanshow(true);
      }
    });
  }, []);

  useEffect(() => {
    if (!apiName) return;
    if (playlistCache.has(apiName)) {
      setPlaylists(playlistCache.get(apiName)!);
      setSomeHook((n) => n + 1);
    } else {
      api
        .getMyPlaylist(apiName)
        .then((resp) => {
          setPlaylists(resp);
          setSomeHook((n) => n + 1);
          setCanshow(true);
          setPlaylistCache((c) => c.set(apiName, resp));
        })
        .catch((err) => {
          toastError(t, err);
        });
    }
  }, [apiName]);

  return (
    <Stack>
      {canshow ? (
        needBind ? (
          <Text>
            请绑定你的音乐平台账户后刷新页面
          </Text>
        ) : (
          <>
            <Flex flexDirection={"row"} alignItems={"center"} mb={4}>
              <Text>选择平台</Text>
              <Select
                ml={2}
                flex={1}
                onChange={(e) => {
                  setApiName(e.target.value);
                }}
                defaultValue={apiName}
              >
                {apis.map((a) => {
                  return <option key={a}>{a}</option>;
                })}
              </Select>
            </Flex>
            <Accordion allowMultiple key={someHook}>
              {playlists.map((p) => (
                <Playlist
                  key={p.id}
                  id={p.id}
                  name={p.name}
                  apiName={apiName}
                  enqueue={props.enqueue}
                />
              ))}
            </Accordion>
          </>
        )
      ) : (
        <>
          <Skeleton height="20px" />
          <Skeleton height="20px" />
          <Skeleton height="20px" />
          <Skeleton height="20px" />
          <Skeleton height="20px" />
          <Skeleton height="20px" />
        </>
      )}
    </Stack>
  );
};
