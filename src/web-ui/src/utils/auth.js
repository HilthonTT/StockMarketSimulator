import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { config } from "./config.js";

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

export function extractJwt() {
  const jwt = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (!jwt) {
    return null;
  }

  const decodedJwt = jwtDecode(jwt);

  const currentTime = Math.floor(Date.now() / 1000); // in seconds
  const isExpired = decodedJwt.exp < currentTime;

  if (isExpired) {
    return null;
  }

  return jwt;
}

export function extractUserInfo() {
  const jwt = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (!jwt) {
    return null;
  }

  const decodedJwt = jwtDecode(jwt);

  const currentTime = Math.floor(Date.now() / 1000); // in seconds
  const isExpired = decodedJwt.exp < currentTime;

  return {
    id: decodedJwt.sub,
    email: decodedJwt.email,
    username: decodedJwt.preferred_username,
    isExpired,
    ...decodedJwt,
  };
}

export async function getCurrentUser() {
  try {
    const user = extractUserInfo();

    if (!user || user?.isExpired) {
      const apiUrl = `${config.baseApiUrl}/api/v1/users/refresh-tokens`;
      const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

      if (!storedRefreshToken) {
        return null;
      }

      const response = await axios.post(apiUrl, {
        refreshToken: storedRefreshToken,
      });

      if (response.status != 200) {
        return null;
      }

      const { accessToken, refreshToken: newRefreshToken } = response.data;

      saveTokensToLocalStorage(accessToken, newRefreshToken);

      const user = extractUserInfo();

      return user;
    }

    return user;
  } catch (error) {
    return null;
  }
}
