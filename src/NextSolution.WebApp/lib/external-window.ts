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
  private windowUrl: URL | null = null;
  private windowRef: Window | null = null;
  private watcherId: any | null = null;

  private resolve: (value?: any) => void = () => {};
  private reject: (reason?: any) => void = () => {};

  public open(url: URL, features: ExternalWindowFeatures): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      if (this.windowRef) this.close(new ExternalWindowError("Reopening another external window.", "WINDOW_REOPENING"));

      this.resolve = resolve;
      this.reject = reject;

      this.windowUrl = url;
      this.windowRef = window.open(this.windowUrl.href, "_blank", this.stringifyFeatures(features));

      if (this.windowRef) {
        (window as any)["external-window"] = this;

        // Start tracking the window using an interval
        this.watcherId = setInterval(() => {
          if (this.windowRef && this.windowRef.closed) {
            // The window is closed, stop the interval
            this.close(new ExternalWindowError("External window closed unexpectedly.", "WINDOW_CLOSE_UNEXPECTEDLY"));
          }
        }, 1000);
      } else {
        this.close(new ExternalWindowError("Could not open the external window.", "WINDOW_OPEN_FAILED"));
      }
    });
  }

  public close(reason?: any): void {
    (window as any)["external-window"] = null;

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

  public static notify(reason?: any): void {
    const origin: string = window.location.origin;

    // Check if the external window is defined
    if (window.opener && window.opener["external-window"]) {
      const externalWindow = window.opener["external-window"] as ExternalWindow;
      const expectedOrigin: string = externalWindow.windowUrl ? externalWindow.windowUrl.searchParams.get("origin") || origin : origin;

      if (expectedOrigin.startsWith(origin)) {
        externalWindow.close(reason);
      }
    }
  }

  private static instance: ExternalWindow = new ExternalWindow();

  public static open(url: URL, features: ExternalWindowFeatures): Promise<void> {
    return ExternalWindow.instance.open(url, features);
  }

  public static close(reason?: any): void {
    ExternalWindow.instance.close(reason);
  }
}

export type ExternalWindowErrorReasons = "WINDOW_REOPENING" | "WINDOW_CLOSE_UNEXPECTEDLY" | "WINDOW_OPEN_FAILED";

export class ExternalWindowError extends Error {
  constructor(
    message: string,
    public reason?: ExternalWindowErrorReasons
  ) {
    super(message);
    this.name = "ExternalWindowError";
  }
}
