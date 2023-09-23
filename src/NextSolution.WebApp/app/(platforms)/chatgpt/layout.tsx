import { ReactNode } from "react";

import { ChatLayout as Layout } from "@/components/chatgpt/layout";

export default function ChatGPTLayout({ children }: { children: ReactNode }) {
  return <Layout>{children}</Layout>;
}
