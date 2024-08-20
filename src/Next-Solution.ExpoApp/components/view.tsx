import { ComponentProps, ComponentRef, forwardRef, useMemo } from "react";
import { View as BaseView, ViewStyle } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

export interface ViewProps extends ComponentProps<typeof BaseView> {
  safeArea?:
    | boolean
    | {
        top?: boolean;
        bottom?: boolean;
        left?: boolean;
        right?: boolean;
      };
}

type ViewRef = ComponentRef<typeof BaseView>;

const View = forwardRef<ViewRef, ViewProps>(({ safeArea, style, ...props }, ref) => {
  const insets = useSafeAreaInsets();
  safeArea ??= false;

  const paddingStyles: ViewStyle | undefined = useMemo(() => {
    if (typeof safeArea === "boolean") {
      if (!safeArea) return undefined;
      return {
        paddingTop: insets.top,
        paddingBottom: insets.bottom,
        paddingLeft: insets.left,
        paddingRight: insets.right
      };
    }

    const padding: ViewStyle = {};
    if (safeArea.top) padding.paddingTop = insets.top;
    if (safeArea.bottom) padding.paddingBottom = insets.bottom;
    if (safeArea.left) padding.paddingLeft = insets.left;
    if (safeArea.right) padding.paddingRight = insets.right;

    return Object.keys(padding).length > 0 ? padding : undefined;
  }, [safeArea, insets]);

  return <BaseView style={[paddingStyles, style]} {...props} ref={ref} />;
});

export { View };
