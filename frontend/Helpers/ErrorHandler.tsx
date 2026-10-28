import axios from "axios";

interface ApiErrorBody {
  errors?: { msg: string }[];
  message?: string;
}

export const handleError = (error: unknown) => {
  if (axios.isAxiosError<ApiErrorBody>(error)) {
    const err = error.response;
    if (Array.isArray(err?.data?.errors)) {
      return err.data.errors.map((e) => e.msg).join(", ");
    }
    return err?.data?.message || err?.statusText || "An error occurred";
  }
  return "An unexpected error occurred";
};
