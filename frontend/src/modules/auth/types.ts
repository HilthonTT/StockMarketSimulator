export type UserResponse = {
  id: string;
  username: string;
  email: string;
  profileImageId?: string | null | undefined;
  bannerImageId?: string | null | undefined;
  // JSON strings from the API; parse with `parseUtc`.
  createdOnUtc: string;
  modifiedOnUtc?: string | null | undefined;
};

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
};
