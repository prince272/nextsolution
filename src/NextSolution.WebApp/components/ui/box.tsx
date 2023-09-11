import { ComponentType, Fragment, ReactNode } from "react";

import { polly } from "@/lib/polly";

const Box = polly<ComponentType, { children: ReactNode }>(function Component({ as: Component = Fragment, children, ...rest }, ref?) {
  return (
    <Component {...rest} ref={ref}>
      {children}
    </Component>
  );
});

export { Box };
