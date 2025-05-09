export type ProblemDetails = {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  traceId: string;
  requestId: string;
};

export type PagedList<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

export type CursorResponse<T> = {
  cursor?: string | null | undefined;
  data: T[];
};
