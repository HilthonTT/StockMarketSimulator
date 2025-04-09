import axios from "axios";
import { Notyf } from "notyf";
import { z } from "zod";

import { config } from "../utils/config.js";
import { checkThemePreference } from "../utils/theme.js";

const formSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
});

document.addEventListener("DOMContentLoaded", () => {
  const notyf = new Notyf();

  const proceedButton = document.getElementById("proceed-button");
  const resendForm = document.getElementById("resend-form");
  const emailInput = document.getElementById("email-input");

  resendForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = emailInput.value.trim();
    const result = await formSchema.safeParseAsync({ email });

    if (!result.success) {
      const errMsg = result.error.errors[0].message;
      notyf.error(errMsg);
      return;
    }

    notyf.success("Sending verification...");
    resendForm.classList.add("opacity-50", "pointer-events-none");

    await axios
      .post(`${config.baseApiUrl}/api/v1/users/resend-email-verification`, {
        email,
      })
      .then(() => {
        console.log("Mock: Verification email sent to:", email);
        notyf.success(`Verification sent to ${email}`);
      })
      .catch(() => {
        notyf.error("Something went wrong!");
      })
      .finally(() => {
        resendForm.classList.remove("opacity-50", "pointer-events-none");
      });
  });

  proceedButton.addEventListener("click", () => {
    notyf.success("Proceeding...");
    console.log("Mock: Proceed button clicked");
  });

  checkThemePreference();
});
