import {
  DARK_MODE,
  DARK_MODE_STORAGE_KEY,
  checkThemePreference,
} from "../utils/theme.js";
import { isValidEmail } from "../utils/validation.js";
import { config } from "../utils/config.js";

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

  async function handleSubmit(event) {
    event.preventDefault();

    const usernameInput = document.getElementById("username");
    const emailInput = document.getElementById("email");
    const passwordInput = document.getElementById("password");
    const confirmPasswordInput = document.getElementById("confirm-password");

    const usernameError = usernameInput.nextElementSibling;
    const emailError = emailInput.nextElementSibling;
    const passwordError = passwordInput.nextElementSibling;
    const confirmPasswordError = confirmPasswordInput.nextElementSibling;

    const username = usernameInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value.trim();
    const confirmPassword = confirmPasswordInput.value.trim();

    let hasError = false;

    // Username validation
    if (!username) {
      usernameError.textContent = "Username is required";
      usernameError.classList.remove("hidden");
      hasError = true;
    } else {
      usernameError.classList.add("hidden");
    }

    // Email validation
    if (!isValidEmail(email)) {
      emailError.textContent = "Invalid email address";
      emailError.classList.remove("hidden");
      hasError = true;
    } else {
      emailError.classList.add("hidden");
    }

    // Password validation
    if (password.length < 6) {
      passwordError.textContent = "Password must be at least 6 characters";
      passwordError.classList.remove("hidden");
      hasError = true;
    } else {
      passwordError.classList.add("hidden");
    }

    if (password !== confirmPassword) {
      confirmPasswordError.textContent = "Passwords do not match";
      confirmPasswordError.classList.remove("hidden");
      hasError = true;
    } else {
      confirmPasswordError.classList.add("hidden");
    }

    // If no errors, proceed with form submission
    if (!hasError) {
      const body = {
        email,
        username,
        password,
        confirmPassword,
      };

      const response = await fetch(
        new URL(`${config.baseApiUrl}/api/v1/users/register`),
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(body),
        }
      );

      if (response.ok) {
        alert(
          "You've registered successfully, you will be redirected to the login page."
        );
        window.location.href = "/sign-in/index.html";
      }
    }
  }

  checkThemePreference();
});
