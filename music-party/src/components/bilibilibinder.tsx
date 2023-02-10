import { Text, Button, Drawer, DrawerBody, DrawerCloseButton, DrawerContent, DrawerFooter, DrawerHeader, DrawerOverlay, Flex, Input, List, ListItem, useDisclosure, useToast } from "@chakra-ui/react"
import React, { useState } from "react"
import { bindAccount, MusicServiceUser, searchUsers } from "../api/api";

export const BilibiliBinder = (props: {}) => {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const btnRef = React.useRef<any>();

    const [users, setUsers] = useState<MusicServiceUser[]>([]);
    const [keyword, setKeyword] = useState("");

    const t = useToast();

    return (
        <>
            <Button ref={btnRef} colorScheme='blue' onClick={onOpen}>
                Bind Bilibili Account
            </Button>
            <Drawer
                isOpen={isOpen}
                placement='left'
                onClose={onClose}
                finalFocusRef={btnRef}>
                <DrawerOverlay />
                <DrawerContent>
                    <DrawerCloseButton />
                    <DrawerHeader>Search and bind your account</DrawerHeader>

                    <DrawerBody>
                        <Flex>
                            <Input flex={1} value={keyword} onChange={e => setKeyword(e.target.value)} placeholder='Your Bilibili name' />
                            <Button ml={2} onClick={async () => {
                                if (keyword === "") return;
                                const users = await searchUsers(keyword, "Bilibili");
                                setUsers(users);
                            }}>Search</Button>
                        </Flex>
                        <List>
                            {users.map((user => {
                                return (<ListItem key={user.identifier}>
                                    <Flex padding={4}>
                                        <Text flex={1} >{user.name}</Text>
                                        <Button onClick={async () => {
                                            try {
                                                await bindAccount(user.identifier, "Bilibili");
                                                t({
                                                    title: "Bind success!",
                                                    status: "success",
                                                    duration: 5000,
                                                    position: "top-right"
                                                });
                                                window.location.href = "/";
                                            } catch (ex) {
                                                console.error(ex);
                                                t({
                                                    title: "Bind failed",
                                                    status: "error",
                                                    duration: 5000,
                                                    position: "top-right",
                                                    description: ex as any
                                                })
                                            }
                                            finally {
                                                onClose();
                                            }
                                        }}>Bind</Button>
                                    </Flex>
                                </ListItem>);
                            }))}
                        </List>
                    </DrawerBody>

                    <DrawerFooter>
                        <Button variant='outline' mr={3} onClick={onClose}>
                            Cancel
                        </Button>
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
        </>
    )
}