import {
  Attributes,
  ComponentPropsWithoutRef,
  ComponentPropsWithRef,
  ElementType,
  ExoticComponent,
  FC,
  JSX,
  PropsWithoutRef
} from "react";

// Utility type to merge two types
type Merge<T, U> = Omit<T, keyof U> & U;

// Props type with an "as" prop that allows specifying the element type
export type PropsWithAs<
  P,
  T extends ElementType,
  S extends keyof JSX.IntrinsicElements = keyof JSX.IntrinsicElements
> = P & {
  as?: T extends keyof JSX.IntrinsicElements ? (T extends S ? T : never) : T;
};

// Helper to get the polymorphic component props without the as attribute
// it will allow the composition of polymorphic components
export type PropsWithoutAs<T> = Omit<T, "as">;

// Polymorphic props type that merges the component-specific props with the "as" prop
export type PolymorphicProps<
  P,
  T extends ElementType,
  S extends keyof JSX.IntrinsicElements = keyof JSX.IntrinsicElements
> = Merge<
  T extends keyof JSX.IntrinsicElements
    ? PropsWithoutRef<JSX.IntrinsicElements[T]>
    : ComponentPropsWithoutRef<T>,
  // cover cases where the allowed ElementType and allowed DOM nodes overlap and are the same
  T extends S ? PropsWithAs<P, T, T> : PropsWithAs<P, T, S>
> &
  Attributes;

// Polymorphic props type for exotic components
export type PolymorphicExoticProps<
  P,
  T extends ElementType,
  S extends keyof JSX.IntrinsicElements = keyof JSX.IntrinsicElements
> =
  T extends ExoticComponent<infer U>
    ? PolymorphicProps<Merge<P, PropsWithoutAs<PropsWithoutRef<U>>>, T, S>
    : never;

// Polymorphic props type for functional components
export type PolymorphicFunctionalProps<
  P,
  T extends ElementType,
  S extends keyof JSX.IntrinsicElements = keyof JSX.IntrinsicElements
> =
  T extends FC<infer U>
    ? PolymorphicProps<Merge<P, PropsWithoutAs<PropsWithoutRef<U>>>, T, S>
    : never;

// Type for the forwarded ref of a component
export type PolymorphicForwardedRef<C extends ElementType> = ComponentPropsWithRef<C>["ref"];
