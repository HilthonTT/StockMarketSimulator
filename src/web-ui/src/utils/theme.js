export const DARK_MODE_STORAGE_KEY = "dark-mode";
export const DARK_MODE = "dark";

export function checkThemePreference() {
  if (localStorage.getItem(DARK_MODE_STORAGE_KEY) === "true") {
    document.documentElement.classList.add(DARK_MODE);
  } else {
    document.documentElement.classList.remove(DARK_MODE);
  }
}
