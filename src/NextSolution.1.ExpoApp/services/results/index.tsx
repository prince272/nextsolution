import { AxiosResponse, HttpStatusCode, isAxiosError } from "axios";

type ProblemType = {
  detail?: string | null;
  instance?: string | null;
  title?: string | null;
  type?: string | null;
  [key: string]: any;
};

export class Result {
  constructor(
    public statusCode: number,
    public status: string,
    public reason: string
  ) {}

  get success() {
    return this.statusCode >= 200 && this.statusCode < 300;
  }

  static async handle<T = Result>(request: Promise<AxiosResponse<any>>): Promise<T> {
    try {
      const response = await request;
      return new Succeeded(HttpStatusCode.Ok, response.statusText, response.data) as T;
    } catch (error) {
      console.warn(error);

      if (isAxiosError(error) && error.response) {
        const response = error.response;
        switch (response.status) {
          case HttpStatusCode.BadRequest: {
            if (response.data?.errors) {
              return new ValidationProblem(response.status, response.statusText, response.data) as T;
            }
            return new BadRequest(response.status, response.statusText, response.data) as T;
          }
          case HttpStatusCode.NotFound: {
            return new NotFound(response.status, response.statusText, response.data) as T;
          }
          case HttpStatusCode.Unauthorized: {
            return new Unauthorized(response.status, response.statusText, response.data) as T;
          }
          case HttpStatusCode.Forbidden: {
            return new Forbidden(response.status, response.statusText, response.data) as T;
          }
          default: {
            return new Problem(response.status, response.statusText, response.data) as T;
          }
        }
      } else if (isAxiosError(error)) {
        return new Problem(-1, "NetworkError", { title: "Unable to connect to the server.", detail: "Please check your internet connection and try again." }) as T;
      } else {
        return new Problem(-999, "UnknownError", { title: "An unknown error occurred.", detail: "Please try again later." }) as T;
      }
    }
  }
}

export class Problem extends Result {
  detail?: string | null;
  instance?: string | null;
  title?: string | null;
  type?: string | null;
  [key: string]: any;

  constructor(statusCode: number, status: string, { detail, instance, title, type, ...extensions }: ProblemType) {
    super(statusCode, status, title || HttpPhrases.getReason(statusCode));
    this.detail = detail;
    this.instance = instance;
    this.title = title;
    this.type = type;
    Object.assign(this, extensions);
  }
}

export class BadRequest extends Problem {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}

export class ValidationProblem<T extends Record<string, any> = any> extends BadRequest {
  errors: Record<keyof T, string[]>;

  constructor(
    statusCode: number,
    status: string,
    {
      errors,
      ...problem
    }: {
      errors: Record<keyof T, string[]>;
    } & ProblemType
  ) {
    super(statusCode, status, problem);
    this.errors = errors;
  }
}

export class NotFound extends Problem {
  constructor(statusCode: number, status: string, data: ProblemType) {
    super(statusCode, status, data);
  }
}

export class Unauthorized extends Problem {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}

export class Forbidden extends Problem {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}

export class Succeeded<T extends unknown = any> extends Result {
  value: T;

  constructor(statusCode: number, status: string, value: T = undefined as any) {
    super(statusCode, status, HttpPhrases.getReason(statusCode));
    this.value = value;
  }
}

export class HttpPhrases {
  public static getReason(statusCode: number): string {
      switch (statusCode) {
          // Informational responses (1xx)
          case 100: return "We've received your request. Please continue sending the rest of the data.";
          case 101: return "We're switching protocols as requested.";
          case 103: return "We're resuming an interrupted request for data upload.";

          // Successful responses (2xx)
          case 200: return "Your request was successful!";
          case 201: return "We've successfully created a new resource for you.";
          case 202: return "Your request has been accepted and is being processed.";
          case 203: return "Your request was successful, but the data might be from a different source.";
          case 204: return "Your request was successful, but there's no content to show.";
          case 205: return "Your request was successful. Please reset your view to see the changes.";
          case 206: return "We're sending only part of the requested data.";

          // Redirection messages (3xx)
          case 300: return "There are multiple options available for your request.";
          case 301: return "The page you're looking for has permanently moved to a new location.";
          case 302: return "The page you're looking for has temporarily moved to a different location.";
          case 303: return "You can find the page under a new URL.";
          case 304: return "The page hasn't changed since your last visit.";
          case 307: return "The page has temporarily moved to a new location.";
          case 308: return "We're resuming an interrupted request for data upload.";

          // Client error responses (4xx)
          case 400: return "We couldn't understand your request due to a syntax error.";
          case 401: return "You need to log in to access this resource.";
          case 403: return "You don't have permission to access this resource.";
          case 404: return "We couldn't find the resource you were looking for.";
          case 405: return "This method is not allowed for the requested resource.";
          case 408: return "Your request took too long and timed out.";
          case 409: return "There's a conflict with your request.";
          case 410: return "The resource you're looking for is no longer available.";
          case 413: return "Your request is too large for us to process.";
          case 414: return "The URL you've requested is too long.";
          case 415: return "We don't support the media type of your request.";

          // Server error responses (5xx)
          case 500: return "There's a problem on our end. Please try again later.";
          case 501: return "We don't support the functionality required to fulfill your request.";
          case 502: return "We received an invalid response from another server.";
          case 503: return "We're currently overloaded or undergoing maintenance. Please try again later.";
          case 504: return "We didn't get a timely response from another server.";
          case 505: return "We don't support the HTTP protocol version used in your request.";
          case 511: return "You need to authenticate to gain network access.";

          default: return 'An unknown error occurred. Please try again later.';
      }
  }
}
