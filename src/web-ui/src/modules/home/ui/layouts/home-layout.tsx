import { HomeNavbar } from "../components/home-navbar";

interface HomeLayoutProps {
  children: React.ReactNode;
}

export const HomeLayout = ({ children }: HomeLayoutProps) => {
  return (
    <div className="flex flex-col min-h-screen">
      <HomeNavbar />

      <div className="flex-1 bg-[#F4F4F0] dark:bg-[#272727]">{children}</div>
    </div>
  );
};
