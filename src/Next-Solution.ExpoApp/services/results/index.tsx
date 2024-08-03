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

  get success() {
    return this.statusCode >= 200 && this.statusCode < 300;
  }

  static async handle<T = Result>(request: Promise<AxiosResponse<any>>): Promise<T> {
    try {
      const response = await request;
      return new Succeeded(HttpStatusCode.Ok, response.statusText, response.data) as T;
    } catch (error) {
      if (isAxiosError(error) && error.response) {
        const response = error.response;
        switch (response.status) {
          case HttpStatusCode.BadRequest: {
            if (response.data?.errors) {
              return new ValidationProblem(
                response.status,
                response.statusText,
                response.data
              ) as T;
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
        return new Problem(-1, "NetworkError", {
          title: "Unable to connect to the server.",
          detail: "Please check your internet connection and try again."
        }) as T;
      } else {
        return new Problem(-999, "UnknownError", {
          title: "An unknown error occurred.",
          detail: "Please try again later."
        }) as T;
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
      return "There was an issue with your request. Please check and try again.";
    } else if (statusCode >= 500 && statusCode < 600) {
      return "Oops! Something went wrong on our end. Please try again later.";
    } else {
      return "An unexpected error occurred. Please try again later.";
    }
  }
}

export class Problem extends Result {
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
  data: T;

  constructor(statusCode: number, status: string, data: T = undefined as any) {
    super(statusCode, status, Result.getMessage(statusCode));
    this.data = data;
  }
}
