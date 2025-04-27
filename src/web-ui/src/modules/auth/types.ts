export type UserResponse = {
  id: string;
  username: string;
  email: string;
  createdOnUtc: Date;
  modifiedOnUtc?: Date | null | undefined;
};

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
};
