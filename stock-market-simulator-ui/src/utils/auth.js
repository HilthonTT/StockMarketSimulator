import { config } from "./config.js";

const ACCESS_TOKEN_KEY = "access-token";
const REFRESH_TOKEN_KEY = "refresh-token";

function decodeJwt(token) {
  try {
    // Using the jwt-decode library
    const decodedJwt = jwt_decode(token);

    return {
      id: decodedJwt.sub,
      email: decodedJwt.email,
      username: decodedJwt.preferred_username,
      exp: decodedJwt.exp,
    };
  } catch (error) {
    return null;
  }
}

async function renewAccessToken(refreshToken) {
  try {
    const response = await fetch(
      new URL(`${config.baseApiUrl}/api/v1/users/refresh-token`),
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ refreshToken }),
      }
    );

    if (!response.ok) {
      return null;
    }

    const data = await response.json();
    saveTokensToLocalStorage(data.accessToken, data.refreshToken);
    return data.accessToken;
  } catch (error) {
    return null;
  }
}

export function saveTokensToLocalStorage(accessToken, refreshToken) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export async function getCurrentUser() {
  let accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (!accessToken) {
    return null;
  }

  const decodedToken = decodeJwt(accessToken);

  if (!decodedToken || decodedToken.exp * 1000 < Date.now()) {
    console.log("IS EXPIRED");

    // Token is expired
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (!refreshToken) {
      return null;
    }

    accessToken = await renewAccessToken(refreshToken);

    if (!accessToken) {
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      return null;
    }
  }

  // Using the jwt-decode library
  const user = decodeJwt(accessToken);

  return { ...user, accessToken };
}

export async function ensureAuthenticated() {
  const user = await getCurrentUser();

  if (!user) {
    window.location.href = "/sign-in/index.html";
    return null;
  }

  return user;
}

export function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}
