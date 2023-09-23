"use client";

import { FC, Fragment, ReactNode, useEffect, useState } from "react";
import { AddIcon, CommentIcon, DeleteIcon, DockPanelLeftIcon, EditIcon, PersonIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Listbox, ListboxItem, ListboxProps, ListboxSection } from "@nextui-org/listbox";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, useDisclosure } from "@nextui-org/modal";
import { clone, startCase } from "lodash";
import { Controller, useForm } from "react-hook-form";

import { useConditionalState } from "@/lib/hooks";

import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader } from "../../ui/sheet";
import { UserButton } from "../user-button";
import { useChatGPT } from "./state";

export const ChatSidebar: FC = () => {
  const { sidebar } = useChatGPT();

  return (
    <Sheet
      placement="left"
      className="text-white dark"
      classNames={{
        base: "max-w-[280px]",
        header: "px-2",
        body: "px-2",
        footer: "px-2"
      }}
      hideCloseButton
      isSticky={sidebar.opened}
      isOpen={sidebar.opened}
      onOpenChange={(opened) => {
        (opened ? sidebar.open : sidebar.close)();
      }}
    >
      <SheetContent>
        <SheetHeader className="flex justify-between gap-2 pb-0">
          <Button className="h-11 justify-start px-4 !text-sm" variant="bordered" fullWidth startContent={<AddIcon className="h-4 w-4" />}>
            New Chat
          </Button>
          <Button className="h-11" variant="bordered" isIconOnly onPress={() => sidebar.close()}>
            <DockPanelLeftIcon className="h-6 w-6" type="outlined" />
          </Button>
        </SheetHeader>
        <SheetBody>
          <ChatListbox />
        </SheetBody>
        <SheetFooter>
          <UserButton />
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
};

const ChatListbox: FC<Omit<ListboxProps, "children">> = ({ ...props }) => {
  return (
    <Listbox
      variant="flat"
      itemClasses={{ base: "h-11 px-3 gap-3 data-[selected=true]:bg-default/40" }}
      disallowEmptySelection
      selectionMode="single"
      aria-label="Listbox menu with chats"
      {...props}
    >
      <ListboxSection title="Yesterday" classNames={{ group: "data-[has-title=true]:pt-2" }}>
        <ListboxItem
          key="new"
          startContent={<CommentIcon className="pointer-events-none h-5 w-5 flex-shrink-0 text-xl" type="outlined" />}
          selectedIcon={<Fragment />}
        >
          New chat
        </ListboxItem>
      </ListboxSection>
    </Listbox>
  );
};

interface ChatItem {
  id: string;
}

interface ChatEditInputs {
  title: string;
}

const ChatItemActions: FC<{ item: ChatItem }> = () => {
  const { isOpen, onOpen: openModel, onClose: closeModel } = useDisclosure();
  const [view, setView] = useState<"edit" | "delete">("edit");
  const [state, setState] = useState<{ action: "idle" | "loading" | "updating" | "deleting"; error?: any }>({
    action: "idle",
    error: null
  });
  const form = useForm<ChatEditInputs>();
  const formErrors = useConditionalState(clone(form.formState.errors), !form.formState.isSubmitting);

  const showEditView = () => {
    setView("edit");
    setState({ action: "idle" });
    openModel();
  };

  const showDeleteView = () => {
    setView("delete");
    setState({ action: "idle" });
    openModel();
  };

  const onEdit = () => {
    closeModel();
  };

  const onDelete = () => {
    closeModel();
  };

  const onCancel = () => {
    closeModel();
  };

  return (
    <div className="flex items-center">
      <Button size="sm" isIconOnly variant="light" color="primary" onPress={showEditView}>
        <EditIcon className="h-5 w-5" type="outlined" />
      </Button>
      <Button size="sm" isIconOnly variant="light" color="danger" onPress={showDeleteView}>
        <DeleteIcon className="h-5 w-5" type="outlined" />
      </Button>
      <Modal isOpen={isOpen} onOpenChange={onCancel}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">{startCase(view)} Chat</ModalHeader>
          {view == "edit" && (
            <ModalBody>
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
                  />
                )}
              />
            </ModalBody>
          )}
          {view == "delete" && (
            <ModalBody>
              <p>Are you sure you would want to delete this chat?</p>
            </ModalBody>
          )}
          <ModalFooter>
            <Button color="default" variant="light" onPress={onCancel}>
              Cancel
            </Button>
            {view == "edit" && (
              <Button color="primary" onPress={onEdit}>
                Save
              </Button>
            )}
            {view == "delete" && (
              <Button color="danger" onPress={onDelete}>
                Delete
              </Button>
            )}
          </ModalFooter>
        </ModalContent>
      </Modal>
    </div>
  );
};
