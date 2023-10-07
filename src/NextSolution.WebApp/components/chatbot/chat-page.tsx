"use client";

import { FC, FormEvent, Fragment, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { BotIcon, SendIcon } from "@/assets/icons";
import { Portal } from "@/ui/misc/portal";
import { Render } from "@/ui/misc/render";
import { Avatar } from "@nextui-org/avatar";
import { Button } from "@nextui-org/button";
import { Textarea } from "@nextui-org/input";
import { Spinner } from "@nextui-org/spinner";

import { useApi, useUser } from "@/lib/api/client";
import { getErrorMessage } from "@/lib/api/utils";
import { useStep } from "@/lib/hooks";
import { cn } from "@/lib/utils";

import { Chat, ChatMessage, ChatStream } from ".";

export const ChatPage: FC = () => {
  const params = useParams();
  const chatId = params?.chatId?.toString();
  const api = useApi();
  const [chat, setChat] = useState<Chat | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [prompt, setPrompt] = useState<string>("");

  const buildMessagesTree = (messages: ChatMessage[], parentId?: string | null) => {
    const result: ChatMessage[] = [];

    messages.forEach((message) => {
      if (message.parentId == parentId) {
        message.childMessages = buildMessagesTree(messages, message.id);
        result.push(message);
      }
    });

    return result;
  };

  const [status, setStatus] = useState<{ action: "idle" | "loading" | "streaming"; error?: any }>({
    action: "loading"
  });

  const loadData = async () => {
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
      setStatus({ action: "idle", error });
    }
  };

  const streamData = async (inputs: unknown) => {
    try {
      setStatus({ action: "streaming" });

      const response = await api.post<ReadableStream<ChatStream>>(`/chats`, inputs, {
        responseType: "stream"
      });

      // Handle the streamed data as you need.
      const stream = response.data;

      // For example, you can read the stream and process the data.
      const reader = stream.getReader();

      try {
        while (true) {
          const { done, value } = await reader.read();
          if (done) break;

          console.log(value);
        }
      } finally {
        // Don't forget to close the stream when done.
        reader.releaseLock();
      }
    } catch (error) {
      setStatus({ action: "idle" });
    }
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    streamData({ chatId, prompt });
    setPrompt("");
    (e.target as HTMLTextAreaElement).focus();
  };

  useEffect(() => {
    loadData();
  }, []);

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
          <Portal rootId="chat-title">{chat?.title}</Portal>
          <ChatMessageTree messages={buildMessagesTree(messages, null)} />
        </Fragment>
      </Render>
      <footer className="mt-auto w-full bg-content1 py-4">
        <div className="stretch mx-2 flex flex-row gap-3 last:mb-2 md:mx-4 md:last:mb-6 lg:mx-auto lg:max-w-2xl xl:max-w-3xl">
          <form className="relative w-full" onSubmit={handleSubmit}>
            <Textarea
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              onKeyPress={(e) => e.currentTarget.value && e.currentTarget.value.trim() && e.key === "Enter" && !e.shiftKey && handleSubmit(e)}
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
    </>
  );
};

const ChatMessageTree: FC<{ messages: ChatMessage[] }> = ({ messages }) => {
  const currentUser = useUser();
  const maxStep = messages.length;
  const [currentStep, { canGoToNextStep, canGoToPrevStep, goToNextStep, goToPrevStep, resetStep, setStep }] = useStep(maxStep);
  const currentMessage = messages[currentStep - 1];

  return (
    <>
      {currentMessage && (
        <>
          <div className={cn("group w-full", currentMessage.role == "user" ? "bg-default-100" : "")}>
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
                    <Avatar key="assistant" name={`Assistant`} className="bg-primary-200" radius="sm" fallback={<BotIcon type="filled" className="h-6 w-6" />} />
                  </Render>
                </div>
                <div className="relative flex flex-1 flex-col">
                  <div>{currentMessage.content}</div>
                  <div className="ml-auto">
                    <span onClick={() => canGoToPrevStep && goToPrevStep()}>&lt;</span>
                    {currentStep}/{maxStep}
                    <span onClick={() => canGoToNextStep && goToNextStep()}>&gt;</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <ChatMessageTree messages={currentMessage.childMessages} />
        </>
      )}
    </>
  );
};
