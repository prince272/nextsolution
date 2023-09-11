import { ReactNode } from "react";

import { PlatformLayout as Layout } from "@/components/platform-layout";

export default function PlatformLayout({ children }: { children: ReactNode }) {
  return <Layout>{children}</Layout>;
}
