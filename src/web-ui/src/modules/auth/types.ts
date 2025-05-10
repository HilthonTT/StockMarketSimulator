export type UserResponse = {
  id: string;
  username: string;
  email: string;
  profileImageId?: string | null | undefined;
  bannerImageId?: string | null | undefined;
  createdOnUtc: Date;
  modifiedOnUtc?: Date | null | undefined;
};

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
};
