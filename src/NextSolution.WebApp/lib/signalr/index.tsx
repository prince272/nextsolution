import {
  createSignalRContext, // SignalR
  LogLevel as SignalRLogLevel
} from "react-signalr";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext();

const useSignalR = () => signalR;

export { useSignalR, useSignalREffect, SignalRProvider, SignalRLogLevel };
