import {
  createSignalRContext // SignalR
} from "react-signalr";
import { ProviderProps } from "react-signalr/lib/signalr/provider";

const { useSignalREffect, Provider, ...signalR } = createSignalRContext();

export const SignalRProvider: React.FC<ProviderProps> = (props) => {
  return <Provider {...props} />;
};

const useSignalR = () => signalR;
export { useSignalR, useSignalREffect };
