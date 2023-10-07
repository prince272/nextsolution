"use client";

import { FC, ReactNode, useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";

// source: https://medium.com/trabe/reusable-react-portals-17dead20232b
export interface PortalProps {
  rootId: string;
  children: ReactNode;
}

export const Portal: FC<PortalProps> = ({ rootId, children }) => {
  const [mounted, setMounted] = useState(typeof window !== "undefined");
  const target = useRef<HTMLElement | null>(mounted ? document.getElementById(rootId) : null);

  useEffect(() => {
    setMounted(true);
    return () => {
      window.requestAnimationFrame(() => {
        if (target.current && target.current.childNodes.length === 0) {
          // remove all child nodes
          while (target.current.firstChild) {
            target.current.removeChild(target.current.firstChild);
          }
          target.current = null;
        }
      });
    };
  }, [rootId]);

  if (mounted && !target.current) {
    target.current = document.createElement("div");
    target.current.setAttribute("id", rootId);
    document.body.appendChild(target.current);
  }

  return mounted ? createPortal(children, target.current!) : children;
};
