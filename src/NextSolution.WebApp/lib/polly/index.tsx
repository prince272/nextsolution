import { ComponentProps, ComponentPropsWithRef, ElementType, forwardRef, JSXElementConstructor, ReactElement } from "react";
import { Merge } from "type-fest";

// React Polly
// source: https://github.com/dgca/react-polly
/**
 * @desc Utility type for getting props type of React component.
 * It takes `defaultProps` into an account - making props with defaults optional.
 * @see {@link https://github.com/sindresorhus/type-fest/blob/79f6b6239b270abc1c1cd20812a00baeb7f9fb57/source/merge.d.ts}
 */
type PropsOf<T extends keyof JSX.IntrinsicElements | JSXElementConstructor<any>> = JSX.LibraryManagedAttributes<T, ComponentProps<T>>;

type RefTypeOf<T extends ElementType> = ComponentPropsWithRef<T>["ref"];

type PolymorphicProps<C extends ElementType, Props = {}> = Merge<PropsOf<C>, Props> & {
  as?: C;
  ref?: RefTypeOf<C>;
};

interface PollyComponentWithRef<DefaultType extends ElementType, Props = {}> {
  <C extends ElementType = DefaultType>(props: PolymorphicProps<C, Props>, ref: RefTypeOf<C>): ReactElement | null;
  displayName?: string | undefined;
}

export function polly<DefaultType extends ElementType, Props = {}>(baseComponent: PollyComponentWithRef<DefaultType, Props>) {
  // @ts-ignore: Unfortunately, we can't derive the right ref type to pass
  // to forwardRef at this time, so we'll ignore this warning and force the
  // type of `WrappedComponent`.
  const WrappedComponent: typeof baseComponent = forwardRef(baseComponent);

  WrappedComponent.displayName = baseComponent.name || "Polly";

  return WrappedComponent;
}
