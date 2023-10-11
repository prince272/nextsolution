import { AxiosResponse, isAxiosError } from "axios";
import { isIdempotentRequestError, isNetworkError } from "axios-retry";

export const isApiError = isAxiosError;

export const getErrorMessage = (error: any) => {
  let message = "";

  if (isAxiosError(error)) {
    if (error.response) {
      message = error.response.data?.title;
    } else if (isNetworkError(error)) {
      message = "Check your internet connection.";
    } else if (isIdempotentRequestError(error)) {
      message = "Something went wrong, Please try again.";
    }
  } else if (typeof error === "object" && error !== null) {
    message = error.title;
  } else {
    return (message = error);
  }

  message = message ? message : "Something went wrong.\nPlease try again later.";
  return message;
};

export function convertAxiosResponseToFetchResponse<T>(axiosResponse: AxiosResponse<T>): Response {
  const { data, status, statusText, headers } = axiosResponse;

  // Convert Axios headers into an array of key-value pairs
  const headersArray: [string, string][] = [];
  for (const key in headers) {
    if (headers.hasOwnProperty(key)) {
      headersArray.push([key, headers[key]]);
    }
  }

  // Convert the response data to a JSON string
  const body = JSON.stringify(data);

  // Create a new Response object with the converted headers
  const response = new Response(body, {
    status,
    statusText,
    headers: new Headers(headersArray)
  });

  return response;
}
