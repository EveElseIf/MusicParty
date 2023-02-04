import { Text, Button, Drawer, DrawerBody, DrawerCloseButton, DrawerContent, DrawerFooter, DrawerHeader, DrawerOverlay, Flex, Input, List, ListItem, useDisclosure, useToast } from "@chakra-ui/react"
import React, { useState } from "react"
import { bindAccount, MusicServiceUser, searchUsers } from "../api/api";

export const QQMusicBinder = (props: {}) => {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const btnRef = React.useRef<any>();

    const [qqNo, setQQNo] = useState("");

    const t = useToast();

    return (
        <>
            <Button ref={btnRef} colorScheme='teal' onClick={onOpen}>
                Bind QQ Music Account
            </Button>
            <Drawer
                isOpen={isOpen}
                placement='left'
                onClose={onClose}
                finalFocusRef={btnRef}>
                <DrawerOverlay />
                <DrawerContent>
                    <DrawerCloseButton />
                    <DrawerHeader>Bind your account</DrawerHeader>

                    <DrawerBody>
                        <Flex>
                            <Input flex={1} value={qqNo} onChange={e => setQQNo(e.target.value)} placeholder='Your QQ number' />
                            <Button ml={2} onClick={async () => {
                                if (qqNo === "") return;
                                await bindAccount(qqNo, "QQMusic");
                                t({
                                    title: "Bind success!",
                                    status: "success",
                                    duration: 5000,
                                    position: "top-right"
                                });
                                window.location.href = "/";
                            }}>Bind</Button>
                        </Flex>
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