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

export const BilibiliBinder = (props: {}) => {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const btnRef = React.useRef<any>();

  const [users, setUsers] = useState<MusicServiceUser[]>([]);
  const [keyword, setKeyword] = useState('');

  const t = useToast();

  return (
    <>
      <Button ref={btnRef} colorScheme='blue' onClick={onOpen}>
        绑定哔哩哔哩账号
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
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
                placeholder='你的哔哩哔哩用户名'
              />
              <Button
                ml={2}
                onClick={async () => {
                  if (keyword === '') return;
                  const users = await searchUsers(keyword, 'Bilibili');
                  setUsers(users);
                }}
              >
                搜索
              </Button>
            </Flex>
            <List>
              {users.map((user) => {
                return (
                  <ListItem key={user.identifier}>
                    <Flex padding={4}>
                      <Text flex={1}>{user.name}</Text>
                      <Button
                        onClick={async () => {
                          try {
                            await bindAccount(user.identifier, 'Bilibili');
                            t({
                              title: '绑定成功！',
                              status: 'success',
                              duration: 5000,
                              position: 'top-right',
                            });
                            window.location.href = '/';
                          } catch (ex) {
                            console.error(ex);
                            t({
                              title: '绑定失败',
                              status: 'error',
                              duration: 5000,
                              position: 'top-right',
                              description: ex as any,
                            });
                          } finally {
                            onClose();
                          }
                        }}
                      >
                        绑定
                      </Button>
                    </Flex>
                  </ListItem>
                );
              })}
            </List>
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
