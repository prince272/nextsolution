"use client";

import {
  createSignalRContext,
  LogLevel as SignalRLogLevel
} from "react-signalr";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext({ shareConnectionBetweenTab: true });
const useSignalR = () => signalR;

export { useSignalREffect, SignalRProvider, SignalRLogLevel, useSignalR };
