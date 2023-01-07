import { CreateToastFnReturn } from "@chakra-ui/react"

export const toastEnqueueOk = (t: CreateToastFnReturn) => {
    t({
        title: "Enqueue Ok!",
        description: "Go back and see the queue again.",
        status: "success",
        duration: 5000,
        position: "top-right",
        isClosable: true
    });
}

export const toastError = (t: CreateToastFnReturn, msg: string) => {
    t({
        title: "Error occured!",
        description: msg,
        status: "error",
        duration: 5000,
        position: "top-right",
        isClosable: true
    });
}