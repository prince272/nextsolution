export interface WebBrowserFeatures {
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

export class WebBrowser {
    private browserUrl: URL | null = null;
    private browserRef: Window | null = null;
    private watcherId: any | null = null;

    private resolve: (value: WebBrowserData) => void = () => { };
    private reject: (reason?: any) => void = () => { };

    public open(url: URL | string, features?: WebBrowserFeatures) {
        return new Promise<WebBrowserData>((resolve, reject) => {
            if (this.browserRef)
                this.close(new WebBrowserError("Reopening another web browser.", "BROWSER_REOPENING"));

            this.resolve = resolve;
            this.reject = reject;

            this.browserUrl = new URL(url.toString());
            this.browserRef = window.open(
                this.browserUrl.href,
                "_blank",
                this.stringifyFeatures(features)
            );

            if (this.browserRef) {
                (window as any)["web-browser"] = this;

                // Start tracking the browser using an interval
                this.watcherId = setInterval(() => {
                    if (this.browserRef && this.browserRef.closed) {
                        // The browser is closed, stop the interval
                        this.close(
                            new WebBrowserError("Web browser closed unexpectedly.", "BROWSER_CLOSE_UNEXPECTEDLY")
                        );
                    }
                }, 1000);
            } else {
                this.close(new WebBrowserError("Could not open the web browser.", "BROWSER_OPEN_FAILED"));
            }
        });
    }

    public close(result?: any) {
        (window as any)["web-browser"] = null;

        if (this.browserRef) {
            this.browserRef.close();
            this.browserRef = null;
        }

        if (this.watcherId !== null) {
            clearInterval(this.watcherId);
            this.watcherId = null;
        }

        if (result instanceof Error) this.reject(result);
        else this.resolve({ ...result } as WebBrowserData);
    }

    private static WebBrowser = new WebBrowser();

    public static open(url: URL | string, features?: WebBrowserFeatures) {
        return this.WebBrowser.open(url, features);
    }

    public static close(result?: any) {
        return this.WebBrowser.close(result);
    }

    public static notify(): boolean {
        // Check if the web browser is defined
        if (window.opener && window.opener["web-browser"]) {
            const webBrowser = window.opener["web-browser"] as WebBrowser;

            const actualOrigin = window.location.origin;
            const expectedOrigin = webBrowser.browserRef?.location.origin;
            const linkingUrl = window.location.href;

            if (expectedOrigin && expectedOrigin.startsWith(actualOrigin)) {
                webBrowser.close({ linkingUrl });
                return true;
            }
        }

        return false;
    }

    private stringifyFeatures(features?: WebBrowserFeatures): string {
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
            const screenHeight =
                window.innerHeight || document.documentElement.clientHeight || screen.height;
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
}

export type WebBrowserErrorReasons =
    | "BROWSER_REOPENING"
    | "BROWSER_CLOSE_UNEXPECTEDLY"
    | "BROWSER_OPEN_FAILED";

export class WebBrowserError extends Error {
    constructor(
        message: string,
        public reason?: WebBrowserErrorReasons
    ) {
        super(message);
        this.name = "WebBrowserError";
    }
}

export interface WebBrowserData {
    linkingUrl: string;
}