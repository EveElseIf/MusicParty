import {
  Text,
  AccordionItem,
  AccordionButton,
  Box,
  AccordionIcon,
  AccordionPanel,
  List,
  ListItem,
  Flex,
  Button,
  Skeleton,
  Stack,
  Divider,
} from '@chakra-ui/react';
import { useEffect, useState } from 'react';
import { getMusicsByPlaylist, Music } from '../api/api';

export const Playlist = (props: {
  id: string;
  name: string;
  apiName: string;
  enqueue: (id: string, apiName: string) => void;
}) => {
  const [loaded, setLoaded] = useState(false);
  const [musics, setMusics] = useState<Music[]>([]);
  const [canshow, setCanshow] = useState(false);
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (!loaded) return;
    getMusicsByPlaylist(props.id, page, props.apiName).then((res) =>
      setMusics(res)
    );
  }, [page]);

  return (
    <AccordionItem>
      <h2>
        <AccordionButton
          onClick={async () => {
            if (loaded) return;
            else setLoaded(true);
            const musics = await getMusicsByPlaylist(
              props.id,
              1,
              props.apiName
            );
            setMusics(musics);
            setCanshow(true);
          }}
        >
          <Box as='span' flex='1' textAlign='left'>
            {props.name}
          </Box>
          <AccordionIcon />
        </AccordionButton>
      </h2>
      <AccordionPanel pb={4}>
        <Skeleton isLoaded={canshow}>
          {musics.length > 0 || (musics.length === 0 && page != 1) ? (
            <Stack>
              <Divider />
              <List spacing={2}>
                {musics.map((m) => (
                  <ListItem key={m.id}>
                    <Flex>
                      <Text flex={1}>{`${m.name} - ${m.artists}`}</Text>
                      <Button
                        onClick={() => {
                          props.enqueue(m.id, props.apiName);
                        }}
                      >
                        点歌
                      </Button>
                    </Flex>
                  </ListItem>
                ))}
              </List>
              <Divider />
              <Flex justifyContent={'flex-end'}>
                <Flex alignItems={'center'}>{`第 ${page} 页`}</Flex>
                <Button
                  colorScheme={'teal'}
                  ml={4}
                  onClick={() => setPage((o) => (o > 1 ? o - 1 : 1))}
                >
                  上一页
                </Button>
                <Button
                  colorScheme={'teal'}
                  ml={4}
                  onClick={() => setPage((o) => o + 1)}
                >
                  下一页
                </Button>
              </Flex>
            </Stack>
          ) : (
            <Text>歌单为空</Text>
          )}
        </Skeleton>
      </AccordionPanel>
    </AccordionItem>
  );
};
