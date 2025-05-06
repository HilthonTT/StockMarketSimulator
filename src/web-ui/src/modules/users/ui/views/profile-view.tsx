import { ProfileUserSection } from "../sections/profile-user-section";

export const ProfileView = () => {
  return (
    <div className="flex flex-col max-w-[1300px] px-4 pt-2.5 mx-auto mb-10 gap-y-6">
      <ProfileUserSection />
    </div>
  );
};
