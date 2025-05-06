export const ProfileBanner = () => {
  return (
    <div className="relative">
      <div
        className="w-full max-h-[200px] h-[15vh] md:h-[25vh] bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl"
        style={{
          backgroundImage: "url(banner.jpg)",
        }}
      ></div>
    </div>
  );
};
