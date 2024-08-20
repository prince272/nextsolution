import { ComponentProps } from "react";
import { View } from "@/components";

export type HomeScreenProps = ComponentProps<typeof View> & {};

export const HomeScreen = (props: HomeScreenProps) => {
  return <View {...props}></View>;
};
