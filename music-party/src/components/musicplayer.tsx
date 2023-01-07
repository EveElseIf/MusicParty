import { Flex, IconButton, Progress, Text } from "@chakra-ui/react";
import { ArrowRightIcon } from "@chakra-ui/icons";
import React, { useEffect, useRef, useState } from "react";

export const MusicPlayer = (props: { src: string, playtime: number, nextClick: () => void }) => {
    const audio = useRef<HTMLAudioElement>();
    const [length, setLength] = useState(100);
    const [time, setTime] = useState(0);

    useEffect(() => {
        if (!audio.current) {
            audio.current = new Audio();
            audio.current.addEventListener("durationchange", () => {
                setLength(audio.current!.duration);
            })
            audio.current.addEventListener("timeupdate", () => {
                setTime(audio.current!.currentTime);
            });
        }
        if (props.src === "") return;
        audio.current.src = props.src;
        if (props.playtime !== 0) audio.current.currentTime = props.playtime;
        audio.current.play();
    }, [props.src]);

    return (
        <Flex flexDirection={"row"}>
            <Progress flex={12} height={"32px"} max={length} value={time} />
            <Text flex={2}>{`${Math.floor(time)} / ${Math.floor(length)}`}</Text>
            <IconButton flex={1} icon={<ArrowRightIcon />} aria-label={"Next Song"}
                onClick={props.nextClick} title={"Next Song"} />
        </Flex>
    );
}