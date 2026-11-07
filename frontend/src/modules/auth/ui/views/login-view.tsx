import { Suspense } from "react";

import { LoginFormSection } from "../sections/login-form-section";

export const LoginView = () => (
  // useSearchParams (callbackUrl) needs a Suspense boundary.
  <Suspense>
    <LoginFormSection />
  </Suspense>
);
