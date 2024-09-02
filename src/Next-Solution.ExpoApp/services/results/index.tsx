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
    public message: string
  ) {}

  get success(): boolean {
    return this instanceof Succeeded;
  }

  static async handle<TResult>(request: Promise<AxiosResponse<any>>): Promise<TResult> {
    try {
      const response = await request;
      return new Succeeded(response.status, response.statusText, response.data) as TResult;
    } catch (error) {
      if (isAxiosError(error) && error.response) {
        const response = error.response;
        switch (response.status) {
          case HttpStatusCode.BadRequest: {
            if (response.data?.errors) {
              return new ValidationFailed(
                response.status,
                response.statusText,
                response.data
              ) as TResult;
            }
            return new BadRequest(response.status, response.statusText, response.data) as TResult;
          }
          case HttpStatusCode.NotFound: {
            return new NotFound(response.status, response.statusText, response.data) as TResult;
          }
          case HttpStatusCode.Unauthorized: {
            return new Unauthorized(response.status, response.statusText, response.data) as TResult;
          }
          case HttpStatusCode.Forbidden: {
            return new Forbidden(response.status, response.statusText, response.data) as TResult;
          }
          default: {
            return new Failed(response.status, response.statusText, response.data) as TResult;
          }
        }
      } else if (isAxiosError(error)) {
        return new Failed(-1, "NetworkError", {
          title: "Unable to connect to the server.",
          detail: "Please check your internet connection and try again."
        }) as TResult;
      } else {
        return new Failed(-999, "UnknownError", {
          title: "An unknown error occurred.",
          detail: "Please try again later."
        }) as TResult;
      }
    }
  }

  static getMessage(statusCode: number): string {
    if (statusCode >= 100 && statusCode < 200) {
      return "We've received your request and are processing it.";
    } else if (statusCode >= 200 && statusCode < 300) {
      return "Your request was successful.";
    } else if (statusCode >= 300 && statusCode < 400) {
      return "You'll be redirected to a new location shortly.";
    } else if (statusCode >= 400 && statusCode < 500) {
      return "There was an issue with your request.";
    } else if (statusCode >= 500 && statusCode < 600) {
      return "Oops! Something went wrong on our end.";
    } else {
      return "An unexpected client error occurred.";
    }
  }
}

export class Failed extends Result {
  detail?: string | null;
  instance?: string | null;
  title?: string | null;
  type?: string | null;
  [key: string]: any;

  constructor(
    statusCode: number,
    status: string,
    { detail, instance, title, type, ...extensions }: ProblemType
  ) {
    super(statusCode, status, title || Result.getMessage(statusCode));
    this.detail = detail;
    this.instance = instance;
    this.title = title;
    this.type = type;
    Object.assign(this, extensions);
  }
}

export class Succeeded<T extends unknown = any> extends Result {
  data: T;

  constructor(statusCode: number, status: string, data: T = undefined as any) {
    super(statusCode, status, Result.getMessage(statusCode));
    this.data = data;
  }
}

export class BadRequest extends Failed {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}

export class ValidationFailed<T extends Record<string, any> = any> extends BadRequest {
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

export class NotFound extends Failed {
  constructor(statusCode: number, status: string, data: ProblemType) {
    super(statusCode, status, data);
  }
}

export class Unauthorized extends Failed {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}

export class Forbidden extends Failed {
  constructor(statusCode: number, status: string, problem: ProblemType) {
    super(statusCode, status, problem);
  }
}
