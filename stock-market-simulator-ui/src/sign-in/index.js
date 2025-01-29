import {
  DARK_MODE,
  DARK_MODE_STORAGE_KEY,
  checkThemePreference,
} from "../utils/theme.js";
import { isValidEmail } from "../utils/validation.js";

document.addEventListener("DOMContentLoaded", () => {
  const form = document.querySelector("form");
  const darkModeToggle = document.getElementById("dark-mode-toggle");

  darkModeToggle.addEventListener("click", () => {
    document.documentElement.classList.toggle(DARK_MODE);

    localStorage.setItem(
      DARK_MODE_STORAGE_KEY,
      document.documentElement.classList.contains(DARK_MODE)
    );
  });

  form.addEventListener("submit", handleSubmit);

  function handleSubmit(event) {
    event.preventDefault();

    const emailInput = document.getElementById("email");
    const emailError = emailInput.nextElementSibling;

    const passwordInput = document.getElementById("password");
    const passwordError = passwordInput.nextElementSibling;

    const email = emailInput.value.trim();
    const password = passwordInput.value.trim();

    emailError.classList.remove("hidden");
    passwordError.classList.remove("hidden");

    let hasError = false;

    if (!isValidEmail(email)) {
      emailError.textContent = "Invalid email address";
      emailError.classList.remove("hidden");
      hasError = true;
    } else {
      emailError.classList.add("hidden");
    }

    if (password.length < 6) {
      passwordError.textContent = "Password must be at least 6 characters";
      passwordError.classList.remove("hidden");
      hasError = true;
    } else {
      passwordError.classList.add("hidden");
    }

    if (!hasError) {
      window.location.href = "/index.html";
    }
  }

  checkThemePreference();
});
