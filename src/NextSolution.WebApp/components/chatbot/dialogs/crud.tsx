"use client";

import { FC, Fragment, useEffect, useRef, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Render } from "@/ui/misc/render";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/modal";
import { Skeleton } from "@nextui-org/skeleton";
import { clone, startCase } from "lodash";
import queryString from "query-string";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { v4 as uuidv4 } from "uuid";

import { useApi, useUnauthenticated } from "@/lib/api/client";
import { getErrorMessage, isApiError } from "@/lib/api/utils";
import { useConditionalState } from "@/lib/hooks";

import { Chat, useChatBotStore } from "..";

export interface CrudProps {
  opened: boolean;
  onClose: (force?: boolean) => void;
}

export type CrudInputs = {
  title: string;
};

export type CrudType = "edit" | "delete" | "deleteAll";

const createCrudModal = (crudType: CrudType) => {
  const CrudModal: FC<CrudProps> = ({ opened, onClose }) => {
    const router = useRouter();
    const searchParams = useSearchParams();
    const chatId = searchParams.get("chatId");
    const [chat, setChat] = useState<Chat | null>(null);
    const { dispatchChats } = useChatBotStore();

    useUnauthenticated(() => {
      router.replace(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
    });

    const api = useApi();
    const form = useForm<CrudInputs>({
      defaultValues: {
        title: ""
      }
    });
    const formErrors = useConditionalState(clone(form.formState.errors), !form.formState.isSubmitting);
    const componentId = useRef(uuidv4()).current;

    const [status, setStatus] = useState<{ action: "idle" | "submitting" | "loading"; error?: any }>({
      action: crudType == "edit" || crudType == "delete" ? "loading" : "idle"
    });

    const onSubmit: SubmitHandler<CrudInputs> = async (inputs) => {
      try {
        setStatus({ action: "submitting" });

        switch (crudType) {
          case "edit":
            const editResponse = await api.put<Chat>(`/chats/${chatId}`, inputs);
            setChat(editResponse.data);
            dispatchChats("update", editResponse.data);
            router.refresh();
            toast.success(`Chat updated.`, { id: componentId });
            break;

          case "delete":
            await api.delete(`/chats/${chatId}`);
            dispatchChats("remove", { id: chatId } as Chat);
            router.refresh();
            toast.success(`Chat deleted.`, { id: componentId });
            break;

          case "deleteAll":
            await api.delete(`/chats`);
            dispatchChats("removeAll");
            router.refresh();
            toast.success(`All chats deleted.`, { id: componentId });
            break;

          default:
            throw new Error("Unknown crudType: " + crudType);
        }

        onClose();
      } catch (error) {
        setStatus({ action: "idle", error });

        if (isApiError(error)) {
          if (error.response) {
            const fields = Object.entries<string[]>(error.response.data.errors || []);
            fields.forEach(([name, message]) => {
              form.setError(name as any, { message: message?.join("\n") });
            });
          }
        }

        toast.error(getErrorMessage(error), { id: componentId });
      }
    };

    const onLoad = async () => {
      try {
        setStatus({ action: "loading" });
        const response = await api.get<Chat>(`/chats/${chatId}`);
        form.reset({ title: response.data.title });
        setChat(response.data);
        setStatus({ action: "idle" });
      } catch (error) {
        setStatus({ action: "idle", error });
      }
    };

    useEffect(() => {
      if (crudType == "edit" || crudType == "delete") {
        onLoad();
      }
    }, []);

    return (
      <>
        <Modal isKeyboardDismissDisabled={true} isOpen={opened} onClose={() => onClose()} as={"form"} onSubmit={form.handleSubmit(onSubmit)}>
          <ModalContent>
            <ModalHeader className="flex flex-col gap-1">
              {startCase(crudType)} {crudType == "deleteAll" ? "Chats" : "Chat"}
            </ModalHeader>
            <Render switch={(crudType == "edit" || crudType == "delete") && !chat ? (status.error ? "error" : crudType) : crudType}>
              <Fragment key="new|edit">
                <ModalBody>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Controller
                      control={form.control}
                      name="title"
                      render={({ field: { onChange, onBlur, value, ref } }) => (
                        <Input
                          ref={ref}
                          onChange={(e) => onChange(e.target.value)}
                          onBlur={onBlur}
                          value={value}
                          isInvalid={!!formErrors.title}
                          errorMessage={formErrors.title?.message}
                          autoComplete="off"
                          label="Title"
                        />
                      )}
                    />
                  </Skeleton>
                </ModalBody>
                <ModalFooter>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button isDisabled={status.action == "loading"} onPress={() => onClose()}>
                      Cancel
                    </Button>
                  </Skeleton>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button onPress={() => form.handleSubmit(onSubmit)()} color="primary" isDisabled={status.action == "loading"} isLoading={status.action == "submitting"}>
                      Save
                    </Button>
                  </Skeleton>
                </ModalFooter>
              </Fragment>
              <Fragment key="delete">
                <ModalBody>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    Are you sure you went to delete &quot;{chat?.title}&quot; chat?
                  </Skeleton>
                </ModalBody>
                <ModalFooter>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button isDisabled={status.action == "loading"} onPress={() => onClose()}>
                      Cancel
                    </Button>
                  </Skeleton>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button isDisabled={status.action == "loading"} onPress={() => form.handleSubmit(onSubmit)()} color="danger" isLoading={status.action == "submitting"}>
                      Delete
                    </Button>
                  </Skeleton>
                </ModalFooter>
              </Fragment>
              <Fragment key="deleteAll">
                <ModalBody>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    Are you sure you went to delete all chats?
                  </Skeleton>
                </ModalBody>
                <ModalFooter>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button isDisabled={status.action == "loading"} onPress={() => onClose()}>
                      Cancel
                    </Button>
                  </Skeleton>
                  <Skeleton className="rounded-xl" isLoaded={status.action != "loading"}>
                    <Button isDisabled={status.action == "loading"} onPress={() => form.handleSubmit(onSubmit)()} color="danger" isLoading={status.action == "submitting"}>
                      Delete All
                    </Button>
                  </Skeleton>
                </ModalFooter>
              </Fragment>
              <Fragment key="error">
                <ModalBody className="pb-8 pt-4">
                  <div className="flex h-20 flex-col items-center justify-center space-y-2 text-center">
                    <div className="p-2 text-sm text-foreground-500">{getErrorMessage(status.error)}</div>
                    <Button
                      variant="light"
                      color="primary"
                      onPress={() => {
                        onLoad();
                      }}
                    >
                      Reload
                    </Button>
                  </div>
                </ModalBody>
              </Fragment>
            </Render>
          </ModalContent>
        </Modal>
      </>
    );
  };

  return CrudModal;
};

const EditChatModal = createCrudModal("edit");
EditChatModal.displayName = "EditChatModal";

const DeleteChatModal = createCrudModal("delete");
DeleteChatModal.displayName = "DeleteChatModal";

const DeleteAllChatsModal = createCrudModal("deleteAll");
DeleteAllChatsModal.displayName = "DeleteAllChatsModal";

export { DeleteAllChatsModal, DeleteChatModal, EditChatModal };
