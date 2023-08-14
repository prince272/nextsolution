// source: https://github.com/react-restart/hooks/blob/master/src/useStateAsync.ts

import React, { Dispatch, SetStateAction, useCallback, useEffect, useRef, useState } from "react";

type Updater<TState> = (state: TState) => TState;

export type AsyncSetState<TState> = (stateUpdate: SetStateAction<TState>) => Promise<TState>;

export function useAsyncFromState<TState>([state, setState]: [TState, Dispatch<SetStateAction<TState>>]): [TState, AsyncSetState<TState>] {
  const resolvers = useRef<((state: TState) => void)[]>([]);

  useEffect(() => {
    resolvers.current.forEach((resolve) => resolve(state));
    resolvers.current.length = 0;
  }, [state]);

  const setStateAsync = useCallback(
    (update: Updater<TState> | TState) => {
      return new Promise<TState>((resolve, reject) => {
        setState((prevState) => {
          try {
            let nextState: TState;
            // ugly instanceof for typescript
            if (update instanceof Function) {
              nextState = update(prevState);
            } else {
              nextState = update;
            }

            // If state does not change, we must resolve the promise because
            // react won't re-render and effect will not resolve. If there are already
            // resolvers queued, then it should be safe to assume an update will happen
            if (!resolvers.current.length && Object.is(nextState, prevState)) {
              resolve(nextState);
            } else {
              resolvers.current.push(resolve);
            }
            return nextState;
          } catch (e) {
            reject(e);
            throw e;
          }
        });
      });
    },
    [setState]
  );
  return [state, setStateAsync];
}

export function useStateAsync<TState>(initialState: TState | (() => TState)): [TState, AsyncSetState<TState>] {
  return useAsyncFromState(useState(initialState));
}
