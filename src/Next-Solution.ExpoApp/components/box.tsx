import { ElementType, forwardRef } from "react";
import { StyleProp, TextStyle, View, ViewStyle } from "react-native";
import { cssInterop } from "nativewind";
import { PolymorphicProps } from "@/types/polymorphic-type";

export type BoxProps<T extends ElementType> = PolymorphicProps<
  { children?: React.ReactNode; style?: StyleProp<ViewStyle | TextStyle> },
  T
>;

const BaseBox = <T extends ElementType = typeof View>(
  { as: Component = View, children, style, ...props }: BoxProps<T>,
  ref: React.Ref<any>
) => {
  return (
    <Component ref={ref} {...props} style={style}>
      {children}
    </Component>
  );
};

cssInterop(BaseBox, { className: "style" });

const Box = forwardRef(BaseBox) as <T extends ElementType = typeof View>(
  props: BoxProps<T> & { ref?: React.Ref<any> }
) => React.ReactElement;

export { Box };
