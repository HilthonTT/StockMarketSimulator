import axios from "axios";
import { z } from "zod";
import { Notyf } from "notyf";

import {
  DARK_MODE,
  DARK_MODE_STORAGE_KEY,
  checkThemePreference,
} from "../utils/theme.js";
import { config } from "../utils/config.js";

const DELAY_MS = 1000; // 1s

const registrationSchema = z
  .object({
    username: z.string().min(1, "Username is required"),
    email: z.string().email("Invalid email address"),
    password: z.string().min(6, "Password must at least be 6 characters"),
    confirmPassword: z.string().min(1, "Confirm your password"),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

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

  const confirmPasswordInput = document.getElementById("confirm-password");
  const toggleConfirmPassword = document.getElementById(
    "toggleConfirmPassword"
  );
  toggleConfirmPassword.addEventListener("click", () => {
    if (confirmPasswordInput.type === "password") {
      confirmPasswordInput.type = "text";
      toggleConfirmPassword.textContent = "🙈";
    } else {
      confirmPasswordInput.type = "password";
      toggleConfirmPassword.textContent = "👁";
    }
  });

  form.addEventListener("submit", handleSubmit);

  async function handleSubmit(event) {
    event.preventDefault();
    disableLoginButton();

    const usernameInput = document.getElementById("username");
    const emailInput = document.getElementById("email");
    const passwordInput = document.getElementById("password");
    const confirmPasswordInput = document.getElementById("confirm-password");

    const usernameError = usernameInput.nextElementSibling;
    const emailError = emailInput.nextElementSibling;
    const passwordError = passwordInput.nextElementSibling;
    const confirmPasswordError = confirmPasswordInput.nextElementSibling;

    const formData = {
      username: usernameInput.value.trim(),
      email: emailInput.value.trim(),
      password: passwordInput.value.trim(),
      confirmPassword: confirmPasswordInput.value.trim(),
    };

    // Clear previous errors
    [usernameError, emailError, passwordError, confirmPasswordError].forEach(
      (el) => el.classList.add("hidden")
    );

    const result = await registrationSchema.safeParseAsync(formData);

    if (!result.success) {
      const formattedErrors = result.error.format();

      if (formattedErrors.username?._errors) {
        usernameError.textContent = formattedErrors.username._errors[0];
        usernameError.classList.remove("hidden");
      }

      if (formattedErrors.email?._errors) {
        emailError.textContent = formattedErrors.email._errors[0];
        emailError.classList.remove("hidden");
      }

      if (formattedErrors.password?._errors) {
        passwordError.textContent = formattedErrors.password._errors[0];
        passwordError.classList.remove("hidden");
      }

      if (formattedErrors.confirmPassword?._errors) {
        confirmPasswordError.textContent =
          formattedErrors.confirmPassword._errors[0];
        confirmPasswordError.classList.remove("hidden");
      }

      enableLoginButton();
      return;
    }

    // If validation passes
    const { confirmPassword, ...requestBody } = result.data;

    try {
      const notyf = new Notyf();

      await axios.post(
        `${config.baseApiUrl}/api/v1/users/register`,
        requestBody
      );

      notyf.success("You've registered!");

      setTimeout(() => {
        saveTokensToLocalStorage(accessToken, refreshToken);
        window.location.href = "/verify/index.html";
      }, DELAY_MS);
    } catch (error) {
      alert("Something went wrong!");
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
});
