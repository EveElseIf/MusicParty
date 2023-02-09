import { TriangleUpIcon } from "@chakra-ui/icons";
import { Text, Card, CardHeader, Heading, CardBody, OrderedList, ListItem, Box, Highlight, Flex, Tooltip, IconButton } from "@chakra-ui/react";
import { MusicOrderAction } from "../api/musichub";

export const MusicQueue = (props: { queue: MusicOrderAction[], top: (actionId: string) => void }) => {
    return <Card mt={4}>
        <CardHeader>
            <Heading size={"lg"}>Queue</Heading>
        </CardHeader>
        <CardBody>
            <OrderedList>
                {props.queue.length > 0 ? props.queue.map((v) => (
                    <ListItem key={v.actionId} fontSize={"lg"}>
                        <Flex>
                            <Box flex={1}>
                                {v.music.name} - {v.music.artists}
                                <Text fontSize={"sm"} fontStyle={"italic"}>
                                    enqueued by {v.enqueuerName}
                                </Text>
                            </Box>
                            {props.queue.findIndex(x => x.actionId === v.actionId) !== 0 &&
                                <Tooltip hasArrow label={"Set this song to top"}>
                                    <IconButton onClick={() => props.top(v.actionId)} aria-label={"Top Music"}
                                        icon={<TriangleUpIcon />}>
                                        Top
                                    </IconButton>
                                </Tooltip>}
                        </Flex>
                    </ListItem>)) : <Text size={"md"}>
                    <Highlight query={"enqueue"} styles={{ px: '2', py: '1', rounded: 'full', bg: 'teal.100' }}>
                        The queue is null currently, feel free to enqueue some music.
                    </Highlight>
                </Text>}
            </OrderedList>
        </CardBody>
    </Card>;
}