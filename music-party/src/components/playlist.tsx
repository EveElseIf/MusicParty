import { Text, AccordionItem, AccordionButton, Box, AccordionIcon, AccordionPanel, List, ListItem, Flex, Button, Skeleton, Stack } from "@chakra-ui/react";
import { useEffect, useState } from "react"
import { getMusicsByPlaylist, Music } from "../api/api";

export const Playlist = (props: { id: string, name: string, enqueue: (id: string) => void }) => {
    const [loaded, setLoaded] = useState(false);
    const [musics, setMusics] = useState<Music[]>([]);
    const [canshow, setCanshow] = useState(false);
    const [page, setPage] = useState(0);

    useEffect(() => {
        if (!loaded) return;
        getMusicsByPlaylist(props.id, page).then((res) => setMusics(res));
    }, [page]);

    return (<AccordionItem>
        <h2>
            <AccordionButton onClick={async () => {
                if (loaded) return;
                else setLoaded(true);
                const musics = await getMusicsByPlaylist(props.id, 0);
                setMusics(musics);
                setCanshow(true);
            }}>
                <Box as="span" flex='1' textAlign='left'>
                    {props.name}
                </Box>
                <AccordionIcon />
            </AccordionButton>
        </h2>
        <AccordionPanel pb={4}>
            <Skeleton isLoaded={canshow}>
                {musics.length > 0 || musics.length === 0 && page != 0 ?
                    <Stack>
                        <List>
                            {musics.map(m => (<ListItem key={m.id} pb={4}>
                                <Flex>
                                    <Text flex={1}>
                                        {`${m.name} - ${m.artist}`}
                                    </Text>
                                    <Button onClick={() => {
                                        props.enqueue(m.id);
                                    }}>Enqueue</Button>
                                </Flex>
                            </ListItem>))}
                        </List>
                        <Flex justifyContent={"flex-end"}>
                            <Flex alignItems={"center"}>
                                {`page ${page}`}
                            </Flex>
                            <Button colorScheme={"teal"} ml={4} onClick={() => setPage(o => o > 0 ? o - 1 : 0)}>
                                Prev
                            </Button>
                            <Button colorScheme={"teal"} ml={4} onClick={() => setPage(o => o + 1)}>
                                Next
                            </Button>
                        </Flex>
                    </Stack> : <Text>
                        Null Playlist.
                    </Text>
                }
            </Skeleton>
        </AccordionPanel>
    </AccordionItem>)
}