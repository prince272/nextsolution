"use client";

import { Button } from "@nextui-org/button";
import { useTheme } from "next-themes";

export default function Page() {
  const { theme, setTheme } = useTheme();

  return (
    <div className="flex justify-center items-center h-screen">
      <Button color="primary" onClick={() => setTheme("light")}>
        Light Mode
      </Button>
      <Button color="primary" onClick={() => setTheme("dark")}>
        Dark Mode
      </Button>
    </div>
  );
}
