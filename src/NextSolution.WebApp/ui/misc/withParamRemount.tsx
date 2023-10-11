import React, { useEffect, useState } from "react";
import { useParams } from "next/navigation";

export function withParamRemount<P>(Component: React.ComponentType<P>, paramName: string) {
  const WrapperComponent: React.FC<P> = (props) => {
    const [key, setKey] = useState(1);
    const params = useParams();
    const paramValue = params[paramName];

    useEffect(() => {
      setKey((currentKey) => currentKey + 1);
    }, [paramValue]);

    return <Component key={key} {...props} />;
  };
  return WrapperComponent;
}
