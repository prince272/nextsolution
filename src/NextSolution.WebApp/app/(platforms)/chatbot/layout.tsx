import { ReactNode } from "react";

import { ChatLayout } from "@/components/chatbot/chat-layout";

export default function Layout({ children }: { children: ReactNode }) {
  return <ChatLayout>{children}</ChatLayout>;
}
