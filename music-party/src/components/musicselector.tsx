import { Flex, Input, Button, useToast, Select, Text } from '@chakra-ui/react';
import { useEffect, useState } from 'react';
import { Connection } from '../api/musichub';
import { toastEnqueueOk, toastError } from '../utils/toast';

export const MusicSelector = (props: { apis: string[]; conn: Connection }) => {
  const [id, setId] = useState('');
  const [apiName, setApiName] = useState('');
  const t = useToast();
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
          placeholder={'输入音乐 ID'}
          onChange={(e) => {
            setId(e.target.value);
          }}
        />
        <Button
          ml={2}
          onClick={() => {
            if (id.length > 0)
              props.conn
                .enqueueMusic(id, apiName)
                .then(() => {
                  toastEnqueueOk(t);
                  setId('');
                })
                .catch((e) => {
                  toastError(t, `音乐 {id: ${id}} 加入队列失败`);
                  console.error(e);
                });
          }}
        >
          点歌
        </Button>
      </Flex>
    </>
  );
};
