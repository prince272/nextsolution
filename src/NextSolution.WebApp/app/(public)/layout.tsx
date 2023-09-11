import { ReactNode } from "react";

import { PublicLayout as Layout } from "@/components/public-layout";

export default function PublicLayout({ children }: { children: ReactNode }) {
  return <Layout>{children}</Layout>;
}
