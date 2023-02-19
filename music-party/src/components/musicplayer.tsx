import {
  Button,
  Flex,
  Icon,
  IconButton,
  Modal,
  ModalBody,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Progress,
  Text,
  Tooltip,
  useDisclosure,
  useToast,
} from "@chakra-ui/react";
import { ArrowRightIcon } from "@chakra-ui/icons";
import React, { useEffect, useRef, useState } from "react";

export const MusicPlayer = (props: {
  src: string;
  playtime: number;
  nextClick: () => void;
  reset: () => void;
}) => {
  const audio = useRef<HTMLAudioElement>();
  const [length, setLength] = useState(100);
  const [time, setTime] = useState(0);
  const t = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();

  useEffect(() => {
    if (!audio.current) {
      audio.current = new Audio();
      audio.current.addEventListener("durationchange", () => {
        setLength(audio.current!.duration);
      });
      audio.current.addEventListener("timeupdate", () => {
        setTime(audio.current!.currentTime);
      });
    }
    if (props.src === "") return;
    audio.current.src = props.src;
    if (props.playtime !== 0) audio.current.currentTime = props.playtime;
    audio.current.play().catch((e: DOMException) => {
      if (
        e.message ===
        "The play() request was interrupted because the media was removed from the document."
      )
        return;
      console.log(e);
      onOpen();
    });
  }, [props.src, props.playtime]);

  return (
    <>
      <Flex flexDirection={"row"} alignItems={"center"}>
        <Progress flex={12} height={"32px"} max={length} value={time} />
        <Text flex={2} textAlign={"center"}>{`${Math.floor(
          time
        )} / ${Math.floor(length)}`}</Text>
        <Tooltip hasArrow label="当音乐没有自动播放时，点我试试">
          <IconButton
            flex={1}
            aria-label={"Play"}
            mr={2}
            icon={
              <Icon viewBox="0 0 1024 1024">
                <path
                  d="M128 138.666667c0-47.232 33.322667-66.666667 74.176-43.562667l663.146667 374.954667c40.96 23.168 40.853333 60.8 0 83.882666L202.176 928.896C161.216 952.064 128 932.565333 128 885.333333v-746.666666z"
                  fill="#3D3D3D"
                  p-id="2949"
                ></path>
              </Icon>
            }
            onClick={() => {
              audio.current?.play();
              audio.current?.pause();
              props.reset();
            }}
          />
        </Tooltip>
        <Tooltip hasArrow label={"切歌"}>
          <IconButton
            flex={1}
            icon={<ArrowRightIcon />}
            aria-label={"切歌"}
            onClick={props.nextClick}
          />
        </Tooltip>
      </Flex>
      <Modal isOpen={isOpen} onClose={onClose}>
        <ModalOverlay>
          <ModalContent>
            <ModalHeader fontSize={"lg"} fontWeight={"bold"}>
              Error
            </ModalHeader>
            <ModalBody>
              <Text>
                看起来你的浏览器不允许音频自动播放，
                请点击下方的按钮来启用自动播放~
              </Text>
            </ModalBody>
            <ModalFooter>
              <Button
                colorScheme={"blue"}
                onClick={() => {
                  audio.current?.play();
                  props.reset();
                  onClose();
                }}
              >
                启用自动播放
              </Button>
            </ModalFooter>
          </ModalContent>
        </ModalOverlay>
      </Modal>
    </>
  );
};
