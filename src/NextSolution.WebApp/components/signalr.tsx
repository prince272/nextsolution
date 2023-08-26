import {
  createSignalRContext // SignalR
} from "react-signalr";
import { ProviderProps } from "react-signalr/lib/signalr/provider";

const { useSignalREffect: useSignalREffectInternal, Provider } = createSignalRContext();

export const SignalRProvider: React.FC<ProviderProps> = (props) => {
  return <Provider {...props} />;
};

export const useSignalREffect = useSignalREffectInternal;