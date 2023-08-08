export interface ExternalWindowFeatures {
  width?: number;
  height?: number;
  left?: number;
  top?: number;
  menubar?: boolean;
  toolbar?: boolean;
  location?: boolean;
  resizable?: boolean;
  scrollbars?: boolean;
  status?: boolean;
  center?: boolean;
}

export class ExternalWindow {
  private windowUrl: string | null = null;
  private windowRef: Window | null = null;
  private watcherId: any | null = null;

  private resolve: (value?: any) => void = () => {};
  private reject: (reason?: any) => void = () => {};

  public open(url: string, features: ExternalWindowFeatures): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      if (this.windowRef) this.close(new Error("Reopening another external window."));

      this.resolve = resolve;
      this.reject = reject;

      this.windowUrl = url;
      this.windowRef = window.open(this.windowUrl, "_blank", this.stringifyFeatures(features));

      if (this.windowRef) {
        window.addEventListener("message", this.onPostMessage, true);

        // Start tracking the window using an interval
        this.watcherId = setInterval(() => {
          if (this.windowRef && this.windowRef.closed) {
            // The window is closed, stop the interval
            this.close();
          }
        }, 1000);
      } else {
        this.close(new Error("Could not open the external window."));
      }
    });
  }

  public close(reason?: any): void {
    window.removeEventListener("message", this.onPostMessage);

    if (this.windowRef) {
      this.windowRef.close();
      this.windowRef = null;
    }

    if (this.watcherId !== null) {
      clearInterval(this.watcherId);
      this.watcherId = null;
    }

    if (reason instanceof Error) this.reject(reason);
    else this.resolve();
  }

  private stringifyFeatures(features: ExternalWindowFeatures): string {
    features = {
      width: 550,
      height: 650,
      menubar: false,
      toolbar: false,
      location: false,
      resizable: true,
      scrollbars: true,
      status: false,
      ...features
    };

    if (features.center) {
      const screenLeft = window.screenLeft || window.screenX || 0;
      const screenTop = window.screenTop || window.screenY || 0;
      const screenWidth = window.innerWidth || document.documentElement.clientWidth || screen.width;
      const screenHeight = window.innerHeight || document.documentElement.clientHeight || screen.height;
      const left = Math.round(screenLeft + (screenWidth - (features.width || 0)) / 2);
      const top = Math.round(screenTop + (screenHeight - (features.height || 0)) / 2);
      features.left = left;
      features.top = top;
    }

    return Object.entries(features)
      .filter(([key, value]) => typeof value === "boolean" || typeof value === "number")
      .map(([key, value]) => `${key}=${value}`)
      .join(",");
  }

  private onPostMessage = (event: MessageEvent<unknown>) => {
    const expectedOrigin = this.windowUrl ? new URL(this.windowUrl).searchParams.get("origin") || location.origin : location.origin;
    if (this.windowRef === event.source && event.origin === expectedOrigin) {
      if (typeof event.data === "object" && event.data !== null && "error" in event.data) {
        this.close(event.data.error);
      } else {
        this.close(event.data);
      }
    }
  };

  private static instance: ExternalWindow | null = null;

  private static getInstance(): ExternalWindow {
    if (!ExternalWindow.instance) {
      ExternalWindow.instance = new ExternalWindow();
    }
    return ExternalWindow.instance;
  }

  public static open(url: string, features: ExternalWindowFeatures): Promise<void> {
    return ExternalWindow.getInstance().open(url, features);
  }

  public static close(reason?: any): void {
     ExternalWindow.getInstance().close(reason);
  }
}