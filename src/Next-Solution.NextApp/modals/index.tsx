import {
  ComponentType,
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useState
} from "react";
import { useHashState, usePreviousValue, withStateAsync } from "@/hooks";
import { sleep } from "@/utils";

export interface ModalState {
  id: string;
  isOpen: boolean;
  isMounted: boolean;
}

export interface ModalControllerProviderProps {
  children?: ReactNode;
}

export interface ModalControllerContextType {
  modals: ModalState[];
  setModalId(id: string): void;
  clearModalId(): void;
  currentId: string;
}

export const ModalControllerContext = createContext<ModalControllerContextType>(null!);

export const ModalControllerProvider = ({ children }: ModalControllerProviderProps) => {
  const [currentId, setCurrentId] = useHashState();
  const clearModalId = useCallback(() => setCurrentId(""), [setCurrentId]);
  const previousId = usePreviousValue(currentId);

  const [modals, setModals] = withStateAsync(useState<ModalState[]>([]));

  const switchModal = useCallback(
    async (id: string) => {
      // add modal with id if it doesn't exist with isOpen = false, isMounted = false.
      await setModals((modals) => {
        if (modals.some((modal) => modal.id === id)) return modals;
        return [...modals, { id, isOpen: false, isMounted: true }];
      });

      // close previous modal
      if (previousId) {
        await setModals((modals) =>
          modals.map((modal) => (modal.id === previousId ? { ...modal, isOpen: false } : modal))
        );

        await sleep(300);

        await setModals((modals) =>
          modals.map((modal) => (modal.id === previousId ? { ...modal, isMounted: false } : modal))
        );
      }

      // open current modal
      await setModals((modals) =>
        modals.map((modal) => (modal.id === id ? { ...modal, isMounted: true } : modal))
      );

      await setModals((modals) =>
        modals.map((modal) => (modal.id === id ? { ...modal, isOpen: true } : modal))
      );
    },
    [previousId, setModals]
  );

  const closeModal = useCallback(
    async (id: string) => {
      await setModals((modals) =>
        modals.map((modal) => (modal.id === id ? { ...modal, isOpen: false } : modal))
      );

      await sleep(300);

      await setModals((modals) =>
        modals.map((modal) => (modal.id === id ? { ...modal, isMounted: false } : modal))
      );
    },
    [setModals]
  );

  useEffect(() => {
    if (currentId) switchModal(currentId);
    else if (previousId) closeModal(previousId);
  }, [closeModal, currentId, previousId, switchModal]);

  return (
    <ModalControllerContext.Provider value={{ modals, currentId, setModalId: setCurrentId, clearModalId  }}>
      {children}
    </ModalControllerContext.Provider>
  );
};

export interface ModalComponentProps {
  id: string;
  isOpen: boolean;
  onOpen?(): void;
  onClose?(result?: unknown): void;
}

export interface ModalControllerProps {
  id: string;
  onOpen?(): void;
  onClose?(result?: unknown): void;

  modal: ComponentType<any>;
  modalProps?: any;
}

export const ModalController = ({
  modal: ModalComponent,
  modalProps,
  id,
  onOpen,
  onClose
}: ModalControllerProps) => {
  const { modals, ...modalControllerProps } = useModalController();
  const { isMounted, isOpen } = modals.find((modal) => modal.id === id) || {
    id: null,
    isMounted: false,
    isOpen: false
  };

  useEffect(() => {
    if (isOpen) onOpen?.();
    else onClose?.();
  }, [isOpen, onClose, onOpen]);

  return isMounted ? (
    <ModalComponent isOpen={isOpen} {...modalProps} {...modalControllerProps} />
  ) : null;
};

export const useModalController = () => {
  const context = useContext(ModalControllerContext);
  if (!context) {
    throw new Error("useModalController must be used within a ModalControllerProvider");
  }

  return context;
};
