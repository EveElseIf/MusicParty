import {
  Text,
  Button,
  Drawer,
  DrawerBody,
  DrawerCloseButton,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerOverlay,
  Flex,
  Input,
  List,
  ListItem,
  useDisclosure,
  useToast,
} from '@chakra-ui/react';
import React, { useState } from 'react';
import { bindAccount, MusicServiceUser, searchUsers } from '../api/api';

export const QQMusicBinder = (props: {}) => {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const btnRef = React.useRef<any>();

  const [qqNo, setQQNo] = useState('');

  const t = useToast();

  return (
    <>
      <Button ref={btnRef} colorScheme='teal' onClick={onOpen}>
        绑定 QQ 音乐账号
      </Button>
      <Drawer
        isOpen={isOpen}
        placement='left'
        onClose={onClose}
        finalFocusRef={btnRef}
      >
        <DrawerOverlay />
        <DrawerContent>
          <DrawerCloseButton />
          <DrawerHeader>搜索并绑定你的账号</DrawerHeader>

          <DrawerBody>
            <Flex>
              <Input
                flex={1}
                value={qqNo}
                onChange={(e) => setQQNo(e.target.value)}
                placeholder='你的 QQ 号'
              />
              <Button
                ml={2}
                onClick={async () => {
                  if (qqNo === '') return;
                  await bindAccount(qqNo, 'QQMusic');
                  t({
                    title: '绑定成功！',
                    status: 'success',
                    duration: 5000,
                    position: 'top-right',
                  });
                  window.location.href = '/';
                }}
              >
                绑定
              </Button>
            </Flex>
          </DrawerBody>

          <DrawerFooter>
            <Button variant='outline' mr={3} onClick={onClose}>
              取消
            </Button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  );
};
