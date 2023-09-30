import { ReactNode } from "react";

import { ChatBotLayout as Layout } from "@/components/chatbot";

export default function ChatBotLayout({ children }: { children: ReactNode }) {
  return <Layout>{children}</Layout>;
}
