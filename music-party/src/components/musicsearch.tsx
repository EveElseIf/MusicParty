import { Flex, Input, Button, useToast, Select, Text } from '@chakra-ui/react';
import { useEffect, useState } from 'react';
import { toastEnqueueOk, toastError } from '../utils/toast';
import {
  Card,
  List,
  ListItem,
  Stack,
  Divider,
} from '@chakra-ui/react';
import { Connection } from '../api/musichub';
import * as api from "../api/api";
const t = useToast();
export const MusicSearchor = (props: { apis: string[]; conn: Connection }) => {
  const [id, setId] = useState('');
  const [musics, setMusics] = useState<api.Music[]>([]);
  const [apiName, setApiName] = useState('');
  useEffect(() => {
    setApiName(props.apis[0]);
  }, [props.apis]);
  return (
    <>
      <Flex flexDirection={'row'} alignItems={'center'} mb={4}>
        <Text>选择平台</Text>
        <Select
          ml={2}
          flex={1}
          onChange={(e) => {
            setApiName(e.target.value);
          }}
        >
          {props.apis.map((a) => {
            return (
              <option key={a} value={a}>
                {a}
              </option>
            );
          })}
        </Select>
      </Flex>

      <Flex flexDirection={'row'}>
        <Input
          flex={1}
          type={'text'}
          value={id}
          placeholder={'输入音乐名 (此功能仍在开发中)'}
          onChange={(e) => {
            setId(e.target.value);
          }}
        />
        <Button
          ml={2}
          onClick={() => {
            if (id.length > 0){
              api.getMusicsByMusicName(id,apiName).then((res)=>setMusics(res));        
              console.log(musics);
            }   
          }}
        >
          搜索
        </Button>
        <Divider>
          
        </Divider>
        <Card >
          {musics.length > 0 || (musics.length === 0 ) ? (
            <Stack>
              <Divider />
              <List spacing={2}>
                {musics.map((m) => (
                  <ListItem key={m.id}>
                    <Flex>
                      <Text flex={1}>{`${m.name} - id : ${m.id}`}</Text>
                      <Button
                        onClick={() => {
                          props.conn.enqueueMusic(m.id.toString(), apiName).then(() => {
                            toastEnqueueOk(t);
                            setId('');
                          })
                          .catch((e) => {
                            toastError(t, `音乐 {id: ${id}} 加入队列失败`);
                            console.error(e);
                          });;
                        }}
                      >
                        点歌
                      </Button>
                    </Flex>
                  </ListItem>
                ))}
              </List>
              <Divider />
            </Stack>
          ) : (
            <Text>歌单为空</Text>
          )}
        </Card>
      </Flex>
    </>
  );
};
