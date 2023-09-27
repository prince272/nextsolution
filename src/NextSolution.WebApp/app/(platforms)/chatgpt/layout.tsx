import { ReactNode } from "react";

import { ChatGPTLayout as Layout } from "@/components/chatgpt";

export default function ChatGPTLayout({ children }: { children: ReactNode }) {
  return <Layout>{children}</Layout>;
}
