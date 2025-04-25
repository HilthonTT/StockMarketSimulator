import axios from "axios";
import { Notyf } from "notyf";
import { z } from "zod";

import {
  DARK_MODE,
  DARK_MODE_STORAGE_KEY,
  checkThemePreference,
} from "../utils/theme.js";
import { config } from "../utils/config.js";
import { getCurrentUser, saveTokensToLocalStorage } from "../utils/auth.js";

const DELAY_MS = 1000; // 1s

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

document.addEventListener("DOMContentLoaded", async () => {
  const form = document.querySelector("form");

  const darkModeToggle = document.getElementById("dark-mode-toggle");
  darkModeToggle.addEventListener("click", () => {
    document.documentElement.classList.toggle(DARK_MODE);

    localStorage.setItem(
      DARK_MODE_STORAGE_KEY,
      document.documentElement.classList.contains(DARK_MODE)
    );
  });

  const togglePassword = document.getElementById("togglePassword");
  const passwordInput = document.getElementById("password");
  const submitButton = document.getElementById("submit");

  togglePassword.addEventListener("click", () => {
    if (passwordInput.type === "password") {
      passwordInput.type = "text";
      togglePassword.textContent = "🙈";
    } else {
      passwordInput.type = "password";
      togglePassword.textContent = "👁";
    }
  });

  form.addEventListener("submit", handleSubmit);

  async function handleSubmit(event) {
    event.preventDefault();
    disableLoginButton();

    const emailInput = document.getElementById("email");
    const emailError = emailInput.nextElementSibling;

    const passwordInput = document.getElementById("password");
    const passwordError = passwordInput.nextElementSibling;

    const formData = {
      email: emailInput.value.trim(),
      password: passwordInput.value.trim(),
    };

    // Clear previous errors
    emailError.classList.add("hidden");
    passwordError.classList.add("hidden");

    const result = loginSchema.safeParse(formData);

    if (!result.success) {
      const errors = result.error.format();

      if (errors.email?._errors) {
        emailError.textContent = errors.email._errors[0];
        emailError.classList.remove("hidden");
      }

      if (errors.password?._errors) {
        passwordError.textContent = errors.password._errors[0];
        passwordError.classList.remove("hidden");
      }

      enableLoginButton();
      return;
    }

    // Valid data
    const body = result.data;

    try {
      const response = await axios.post(
        `${config.baseApiUrl}/api/v1/users/login`,
        body
      );

      const { accessToken, refreshToken } = response.data;

      if (accessToken && refreshToken) {
        const notyf = new Notyf();

        saveTokensToLocalStorage(accessToken, refreshToken);

        notyf.success("You've logged in!");

        setTimeout(() => {
          saveTokensToLocalStorage(accessToken, refreshToken);
          window.location.href = "/index.html";
        }, DELAY_MS);
      } else {
        alert("Missing tokens in response!");
      }
    } catch (error) {
      if (axios.isAxiosError(error)) {
        // You can customize this based on `error.response?.status`
        alert(
          "Login failed: " + (error.response?.data?.message || "Unknown error")
        );
      } else {
        alert("An unexpected error occurred.");
      }
    } finally {
      enableLoginButton();
    }
  }

  function enableLoginButton() {
    submitButton.disabled = false;
    submitButton.textContent = "Login";
  }

  function disableLoginButton() {
    submitButton.disabled = true;
    submitButton.textContent = "Loading...";
  }

  checkThemePreference();

  const currentUser = await getCurrentUser();
  if (currentUser) {
    window.location.href = "/";
  }
});
