"use client";

import { FC, FormEvent, Fragment, MutableRefObject, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { BotIcon, SendIcon } from "@/assets/icons";
import { Portal } from "@/ui/misc/portal";
import { Render } from "@/ui/misc/render";
import { Avatar } from "@nextui-org/avatar";
import { Button } from "@nextui-org/button";
import { Textarea } from "@nextui-org/input";
import { Spinner } from "@nextui-org/spinner";
import { trim } from "lodash";
import toast from "react-hot-toast";
import { animateScroll as scroller } from "react-scroll";
import { v4 as uuidv4 } from "uuid";

import { useApi, useUser } from "@/lib/api/client";
import { getErrorMessage, isApiError } from "@/lib/api/utils";
import { useScroll, useStep, useThrottledCallback } from "@/lib/hooks";
import { cn } from "@/lib/utils";

import { Chat, ChatMessage, ChatStream } from ".";
import { ChatMarkdown } from "./chat-markdown";

const ChatPage: FC = () => {
  const componentId = useRef(uuidv4()).current;
  const params = useParams();
  const chatId = params?.chatId?.[0];
  const router = useRouter();
  const api = useApi();
  const [chat, setChat] = useState<Chat | null>(null);
  const messagesRef = useRef<ChatMessage[]>([]);
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const windowScrollRef = useRef<{ scrollY: number; scrollX: number }>({ scrollY: 0, scrollX: 0 });

  const scrollToBottom = () => {
    if (windowScrollRef.current.scrollY > 0.8) {
      scroller.scrollToBottom({
        duration: 500,
        smooth: true
      });
    }
  };

  const scrollAlong = useThrottledCallback(scrollToBottom, [], 1000);

  const updateMessages = (newMessages: ChatMessage[]) => {
    setMessages((prevMessages) => {
      const updatedMessages = [...prevMessages];

      newMessages.forEach((newMessage) => {
        const existingIndex = updatedMessages.findIndex((item) => item.id === newMessage.id);

        if (existingIndex !== -1) {
          // If the object exists, update it
          updatedMessages[existingIndex] = newMessage;
        } else {
          // If the object doesn't exist, add it
          updatedMessages.push(newMessage);
        }
      });

      return updatedMessages;
    });
  };

  const [prompt, setPrompt] = useState<string>("");

  const [status, setStatus] = useState<{ action: "idle" | "loading" | "streaming"; error?: any }>({
    action: chatId ? "loading" : "idle"
  });

  const loadData = useCallback(async () => {
    try {
      setStatus({ action: "loading" });

      const [chatResponse, chatMessageResponse] = await Promise.all([
        api.get<Chat>(`/chats/${chatId}`),
        api.get<{ items: ChatMessage[] }>(`/chats/${chatId}/messages`, { params: { offset: 0, limit: -1 } })
      ]);

      setChat(chatResponse.data);
      setMessages(chatMessageResponse.data.items);
      setStatus({ action: "idle" });
    } catch (error) {
      if (isApiError(error) && error?.request?.status == 404) {
        router.replace("/chatbot");
      }
      setStatus({ action: "idle", error });
    }
  }, [api, chatId, router]);

  const streamData = async (inputs: unknown) => {
    try {
      scrollToBottom();
      setStatus({ action: "streaming" });

      const response = await api.stream({ url: "/chats", method: "POST", data: inputs });

      if (!response.body) {
        throw new Error("Response body is not a readable stream");
      }

      const reader = response.body.getReader();

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        const valueStr = trim(new TextDecoder().decode(value), "[] ,");
        if (!valueStr) continue;

        console.debug("Chat streaming: " + valueStr);
        const chatStream = (JSON.parse(`[${valueStr}]`) as ChatStream[]).slice(-1)[0];

        if (!chatId) router.replace(`/chatbot/${chatStream.chatId}`);
        updateMessages([chatStream.user, chatStream.assistant]);
        scrollAlong();
      }

      setStatus({ action: "idle" });
    } catch (error) {
      console.error(error);
      setStatus({ action: "idle" });
      toast.error(getErrorMessage(error), { id: componentId });
    }
  };

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    (e.target as HTMLTextAreaElement).focus();

    const messageId = messagesRef.current.filter((m) => m).slice(-1)[0]?.id;
    streamData({ chatId, messageId, prompt });
    setPrompt("");
  };

  useScroll(
    useMemo(() => window, []),
    (value) => (windowScrollRef.current = value)
  );

  useEffect(() => {
    if (chatId) loadData();
  }, [chatId, loadData]);

  return (
    <>
      <Render switch={status.action == "loading" ? "loading" : status.error ? "error" : "content"}>
        <div key="error" className="m-auto flex h-24 flex-col items-center justify-center space-y-2 text-center">
          <div className="p-2 text-sm text-foreground-500">{getErrorMessage(status.error)}</div>
          <Button
            variant="light"
            color="primary"
            onPress={() => {
              loadData();
            }}
          >
            Reload
          </Button>
        </div>
        <div key="loading" className="m-auto flex h-24 flex-col items-center justify-center space-y-2 text-center">
          <Spinner className="flex h-full flex-none items-center justify-center" size="lg" aria-label="Loading..." />
        </div>
        <Fragment key="content">
          <Portal rootId="chat-title">{chatId ? chat?.title : "New Chat"}</Portal>
          <MessageBody messagesRef={messagesRef} messages={getMessageTree(messages, null)} />
        </Fragment>
      </Render>
      <MessageCommand {...{ status, prompt, setPrompt, onSubmit }} />
    </>
  );
};

interface MessageCommandProps {
  status: { action: "idle" | "loading" | "streaming"; error?: any };
  prompt: string;
  setPrompt: (prompt: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

const MessageCommand: FC<MessageCommandProps> = ({ status, prompt, setPrompt, onSubmit }) => {
  return (
    <footer className="sticky bottom-0 z-20 mt-auto w-full bg-default-50 py-4">
      <div className="stretch mx-2 flex flex-row gap-3 last:mb-2 md:mx-4 md:last:mb-6 lg:mx-auto lg:max-w-2xl xl:max-w-3xl">
        <form className="relative w-full" onSubmit={onSubmit}>
          <Textarea
            isDisabled={status.action == "loading" || status.error}
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            onKeyPress={(e) => e.currentTarget.value && e.currentTarget.value.trim() && e.key === "Enter" && !e.shiftKey && onSubmit(e)}
            className="w-full"
            classNames={{ base: "w-full", inputWrapper: "py-2 pl-3 pr-14" }}
            minRows={1}
            maxRows={5}
            size="lg"
          />
          <Button
            isDisabled={!prompt || !prompt.trim()}
            isLoading={status.action == "streaming"}
            isIconOnly
            className="absolute bottom-0 right-0 z-10 mb-3 mr-2"
            color={!prompt || !prompt.trim() ? "default" : "primary"}
            type="submit"
          >
            <SendIcon className={cn("h-5 w-5", status.action == "streaming" && "hidden")} />
          </Button>
        </form>
      </div>
    </footer>
  );
};

interface MessageBodyProps {
  messagesRef: MutableRefObject<ChatMessage[]>;
  messages: ChatMessage[];
  index?: number;
}

const MessageBody: FC<MessageBodyProps> = ({ index = 0, messagesRef, messages }) => {
  const currentUser = useUser();
  const maxStep = messages.length;
  const [currentStep, { canGoToNextStep, canGoToPrevStep, goToNextStep, goToPrevStep, resetStep, setStep }] = useStep(maxStep);
  const currentMessage = messages[currentStep - 1];
  messagesRef.current[index] = currentMessage;
  messagesRef.current = messagesRef.current.filter((m) => m).slice(0, index + 1);

  return (
    <>
      {currentMessage && (
        <>
          <div className={cn("group w-full", currentMessage.role == "user" ? "bg-default-100" : "bg-default-50")}>
            <div className="m-auto justify-center p-4 text-base md:gap-6 md:py-6">
              <div className="mx-auto flex flex-1 gap-4 text-base md:max-w-2xl md:gap-6 lg:max-w-[38rem] xl:max-w-3xl">
                <div className="relative flex flex-shrink-0 flex-col items-end">
                  <Render switch={currentMessage.role}>
                    <Avatar
                      key="user"
                      name={`${currentUser?.firstName} ${currentUser?.lastName}`}
                      src={currentUser?.avatarUrl}
                      showFallback={!currentUser?.avatarUrl}
                      fallback={<span className="text-xl">{currentUser?.firstName?.[0]}</span>}
                      radius="sm"
                      className="bg-default-900 text-default-50"
                    />
                    <Avatar key="assistant" name={`Assistant`} className="bg-primary" radius="sm" fallback={<BotIcon type="filled" className="h-6 w-6" />} />
                  </Render>
                </div>
                <div className="relative flex flex-1 flex-col">
                  <Render as="div" className="prose prose-sm prose-light max-w-none dark:prose-dark" switch={currentMessage.role}>
                    <Fragment key="user">{currentMessage.content}</Fragment>
                    <ChatMarkdown key="assistant" source={currentMessage.content} />
                  </Render>
                  <div className="ml-auto">
                    <span onClick={() => canGoToPrevStep && goToPrevStep()}>&lt;</span>
                    {currentStep}/{maxStep}
                    <span onClick={() => canGoToNextStep && goToNextStep()}>&gt;</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <MessageBody index={index + 1} messagesRef={messagesRef} messages={currentMessage.childMessages} />
        </>
      )}
    </>
  );
};
MessageBody.displayName = "MessageBody";

const getMessageTree = (messages: ChatMessage[], parentId?: string | null) => {
  const result: ChatMessage[] = [];

  messages.forEach((message) => {
    if (message.parentId == parentId) {
      message.childMessages = getMessageTree(messages, message.id);
      result.push(message);
    }
  });

  return result;
};

export { ChatPage };
