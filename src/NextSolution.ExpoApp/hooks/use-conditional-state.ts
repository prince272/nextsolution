import { useEffect, useState } from "react";

// Conditionally update state.
export function useConditionalState<T>(initialState: T, dependency: boolean): T {
  const [currentState, setCurrentState] = useState<T>(initialState);
  const [prevDependency, setPrevDependency] = useState<boolean>(dependency);

  useEffect(() => {
    if (dependency && prevDependency !== dependency) {
      // Update state when the dependency becomes true
      setCurrentState(initialState);
    }
    setPrevDependency(dependency);
  }, [dependency, initialState, prevDependency]);

  return currentState;
}
