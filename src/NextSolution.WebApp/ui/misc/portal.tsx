"use client";

import { FC, ReactNode, useEffect, useMemo, useRef } from "react";
import { createPortal } from "react-dom";

// source: https://medium.com/trabe/reusable-react-portals-17dead20232b
export interface PortalProps {
  rootId: string;
  children: ReactNode;
}

export const Portal: FC<PortalProps> = ({ rootId, children }) => {
  const target = useRef<HTMLElement | null>(useMemo(() => document.getElementById(rootId), [rootId]));

  useEffect(() => {
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

  if (!target.current) {
    target.current = document.createElement("div");
    target.current.setAttribute("id", rootId);
    document.body.appendChild(target.current);
  }

  return createPortal(children, target.current!);
};
