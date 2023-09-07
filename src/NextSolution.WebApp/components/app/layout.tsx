import { FC, ReactNode } from "react";
import { Link } from "@nextui-org/link";

export const AppLayout: FC<{ children: ReactNode }> = ({ children }) => {
  return (
    <div className="relative flex h-screen flex-col">
      <main className="container mx-auto max-w-7xl flex-grow px-6 pt-16">{children}</main>
      <footer className="flex w-full items-center justify-center py-3"></footer>
    </div>
  );
};
