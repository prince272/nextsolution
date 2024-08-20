import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState
} from "react";
import { cssInterop } from "nativewind";
import { Snackbar } from "react-native-paper";

cssInterop(Snackbar, { className: "style" });

interface SnackbarMessageOptions {
  duration?: number; // Duration in milliseconds
  key?: string; // Optional key
}

interface SnackbarMessage {
  key: string;
  content: string; // Changed from message to content
  options: SnackbarMessageOptions;
  visible: boolean; // Add visible property
  duration: number; // Duration property
}

interface SnackbarContextType {
  show: (content: string, options?: SnackbarMessageOptions) => string; // Return type is key
  hide: (key: string) => void; // Function to hide a specific snackbar
  messages: SnackbarMessage[];
}

const SnackbarContext = createContext<SnackbarContextType | undefined>(undefined);

const SnackbarProvider: React.FC<{ children: (context: SnackbarContextType) => ReactNode }> = ({
  children
}) => {
  const [snackbarMessages, setSnackbarMessages] = useState<SnackbarMessage[]>([]);
  const [queue, setQueue] = useState<SnackbarMessage[]>([]);
  const [isShowing, setIsShowing] = useState<boolean>(false);
  const timeoutsRef = useRef<Map<string, NodeJS.Timeout>>(new Map());

  const show = useCallback((content: string, options: SnackbarMessageOptions = {}): string => {
    const key = options.key || Date.now().toString(); // Generate unique key if not provided
    const duration = options.duration || 3000; // Default duration

    const newMessage = { key, content, options, visible: true, duration };

    setQueue((prevQueue) => [...prevQueue, newMessage]);

    return key; // Return the key
  }, []);

  const hide = useCallback((key: string) => {
    setSnackbarMessages((prevMessages) => {
      // Clear the timeout for the key if it exists
      if (timeoutsRef.current.has(key)) {
        clearTimeout(timeoutsRef.current.get(key)!);
        timeoutsRef.current.delete(key);
      }

      const updatedMessages = prevMessages.map((snack) =>
        snack.key === key ? { ...snack, visible: false } : snack
      );

      // Remove the snackbar message after 1 second
      setTimeout(() => {
        setSnackbarMessages((prevMessages) => prevMessages.filter((snack) => snack.key !== key));
      }, 1000); // Delay to allow fade-out effect

      return updatedMessages;
    });
  }, []);

  const processQueue = useCallback(() => {
    if (queue.length === 0 || isShowing) return;

    const [nextMessage, ...restQueue] = queue;
    setQueue(restQueue);
    setIsShowing(true);

    setSnackbarMessages((prevMessages) => [...prevMessages, nextMessage]);

    const timeoutId = setTimeout(() => {
      hide(nextMessage.key);
      setIsShowing(false);
    }, nextMessage.duration);

    timeoutsRef.current.set(nextMessage.key, timeoutId);
  }, [queue, isShowing, hide]);

  // Trigger processing the queue whenever it updates
  useEffect(() => {
    processQueue();
  }, [processQueue]);

  const contextValue = { show, hide, messages: snackbarMessages };

  return (
    <SnackbarContext.Provider value={contextValue}>
      {children(contextValue)}
    </SnackbarContext.Provider>
  );
};

const useSnackbar = () => {
  const context = useContext(SnackbarContext);
  if (!context) {
    throw new Error("useSnackbar must be used within a SnackbarProvider");
  }
  return context;
};

export { SnackbarProvider, useSnackbar, Snackbar };
