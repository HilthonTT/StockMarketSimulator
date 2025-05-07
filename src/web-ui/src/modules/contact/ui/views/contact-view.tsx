import { ContactSection } from "../sections/contact-section";

export const ContactView = () => {
  return (
    <div className="relative antialiased">
      <div className="relative flex flex-col items-center justify-center px-4 pt-2.5 pb-24 min-h-screen z-50">
        <div className="relative max-w-[1200px] w-full mb-10">
          <ContactSection />
        </div>
      </div>
    </div>
  );
};
