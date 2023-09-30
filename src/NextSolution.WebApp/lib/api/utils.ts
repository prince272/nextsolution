import { isAxiosError } from "axios";
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
