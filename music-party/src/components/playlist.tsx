import { Text, AccordionItem, AccordionButton, Box, AccordionIcon, AccordionPanel, List, ListItem, Flex, Button, Skeleton } from "@chakra-ui/react";
import { useEffect, useState } from "react"
import { getMusicByPlaylist, Music } from "../api/api";

export const Playlist = (props: { id: string, name: string, enqueue: (id: string) => void }) => {
    const [loaded, setLoaded] = useState(false);
    const [musics, setMusics] = useState<Music[]>([]);
    const [canshow, setCanshow] = useState(false);

    return (<AccordionItem>
        <h2>
            <AccordionButton onClick={async () => {
                if (loaded) return;
                else setLoaded(true);
                const musics = await getMusicByPlaylist(props.id);
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
                {musics.length > 0 ?
                    <List>
                        {musics.map(m => (<ListItem key={m.id}>
                            <Flex>
                                <Text flex={1}>
                                    {`${m.name} - ${m.artist}`}
                                </Text>
                                <Button onClick={() => {
                                    props.enqueue(m.id);
                                }}>Enqueue</Button>
                            </Flex>
                        </ListItem>))}
                    </List> : <Text>
                        Null Playlist.
                    </Text>
                }
            </Skeleton>
        </AccordionPanel>
    </AccordionItem>)
}