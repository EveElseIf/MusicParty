import { CreateToastFnReturn } from "@chakra-ui/react";

export const toastEnqueueOk = (t: CreateToastFnReturn) => {
  t({
    title: "成功将歌曲加入队列",
    description: "回去看看播放队列吧",
    status: "success",
    duration: 5000,
    position: "top-right",
    isClosable: true,
  });
};

export const toastError = (t: CreateToastFnReturn, msg: string) => {
  t({
    title: "寄，出错了！",
    description: msg,
    status: "error",
    duration: 5000,
    position: "top-right",
    isClosable: true,
  });
};

export const toastInfo = (t: CreateToastFnReturn, msg: string) => {
  t({
    title: "信息",
    description: msg,
    status: "info",
    duration: 5000,
    position: "top-right",
    isClosable: true,
  });
};
