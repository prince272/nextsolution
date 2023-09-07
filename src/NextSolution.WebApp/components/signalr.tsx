"use client";

import { createSignalRContext } from "react-signalr";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext({ shareConnectionBetweenTab: false });
const useSignalR = () => signalR;

export { useSignalREffect, SignalRProvider, useSignalR };
