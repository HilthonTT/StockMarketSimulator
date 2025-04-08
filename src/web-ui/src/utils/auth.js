import { jwtDecode } from "jwt-decode";

const ACCESS_TOKEN_KEY = "access-token";
const REFRESH_TOKEN_KEY = "refresh-token";

export function saveTokensToLocalStorage(accessToken, refreshToken) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function extractUserInfo() {
  const jwt = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (!jwt) {
    return null;
  }

  const decodedJwt = jwtDecode(jwt);

  return {
    id: decodedJwt.sub,
    email: decodedJwt.email,
    username: decodedJwt.preferred_username,
    ...decodedJwt,
  };
}
