"use client";

import { createSignalRContext } from "react-signalr/signalr";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext({ shareConnectionBetweenTab: true });
const useSignalR = () => signalR;

export { useSignalREffect, SignalRProvider, useSignalR };
