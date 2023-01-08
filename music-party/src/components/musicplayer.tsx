import { Flex, Icon, IconButton, Progress, Text, Tooltip, useToast } from "@chakra-ui/react";
import { ArrowRightIcon } from "@chakra-ui/icons";
import React, { useEffect, useRef, useState } from "react";
import { toastError } from "../utils/toast";

export const MusicPlayer = (props: { src: string, playtime: number, nextClick: () => void, reset: () => void }) => {
    const audio = useRef<HTMLAudioElement>();
    const [length, setLength] = useState(100);
    const [time, setTime] = useState(0);
    const t = useToast();

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
        audio.current.play().catch((e: DOMException) => {
            if (e.message === "The play() request was interrupted because the media was removed from the document.")
                return;
            console.log(e);
            toastError(t, "It seems that your browser didn't allow auto play. Please try click the play button manually.");

        });
    }, [props.src, props.playtime]);

    return (
        <Flex flexDirection={"row"} alignItems={"center"}>
            <Progress flex={12} height={"32px"} max={length} value={time} />
            <Text flex={2} textAlign={"center"}>{`${Math.floor(time)} / ${Math.floor(length)}`}</Text>
            <Tooltip hasArrow label="If music can't play, try click this.">
                <IconButton flex={1} aria-label={"Play"}
                    icon={<Icon viewBox="0 0 1024 1024">
                        <path
                            d="M128 138.666667c0-47.232 33.322667-66.666667 74.176-43.562667l663.146667 374.954667c40.96 23.168 40.853333 60.8 0 83.882666L202.176 928.896C161.216 952.064 128 932.565333 128 885.333333v-746.666666z"
                            fill="#3D3D3D" p-id="2949"></path>
                    </Icon>} onClick={() => {
                        audio.current?.play();
                        audio.current?.pause();
                        props.reset();
                    }} />
            </Tooltip>
            <Tooltip hasArrow label={"Next song"}>
                <IconButton flex={1} icon={<ArrowRightIcon />} aria-label={"Next Song"}
                    onClick={props.nextClick} />
            </Tooltip>
        </Flex >
    );
}