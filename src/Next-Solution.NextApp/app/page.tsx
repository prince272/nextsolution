"use client";

import { useModalController } from "@/modals";
import { useAppearance, useAuthentication } from "@/states";
import { Button } from "@nextui-org/button";
import { AppearanceState } from "@/states/appearance";

export default function Page() {
  const { user: currentUser, clearUser } = useAuthentication();
  const { setModalId } = useModalController();
  const { setTheme } = useAppearance();

  const handleLogout = () => {
    clearUser();
  };

  const handleSignIn = () => {
    setModalId("sign-in-method");
  };

  const toggleTheme = (newTheme: AppearanceState["theme"]) => {
    setTheme(newTheme);
  };

  return (
    <div className={`flex-1 flex flex-col items-center justify-center px-4 py-8 h-full`}>
      <div className="flex space-x-4 mb-4">
        <Button onPress={() => toggleTheme("light")} className="rounded-full shadow-lg">
          Light Theme
        </Button>
        <Button onPress={() => toggleTheme("dark")} className="rounded-full shadow-lg">
          Dark Theme
        </Button>
      </div>
      {currentUser ? (
        <div className={`rounded-lg shadow-lg p-6 w-full max-w-md`}>
          <p className="text-2xl font-semibold mb-6 text-center">
            User Profile
          </p>
          <div className="space-y-4">
            <p className="text-lg">
              <span className="font-bold">Name:</span> {currentUser.firstName} {currentUser.lastName}
            </p>
            <p className="text-lg">
              <span className="font-bold">Username:</span> {currentUser.userName}
            </p>
            <p className="text-lg">
              <span className="font-bold">Email:</span> {currentUser.email || "No email provided"}
            </p>
            <p className="text-lg">
              <span className="font-bold">Phone:</span> {currentUser.phoneNumber || "No phone number provided"}
            </p>
            <p className="text-lg">
              <span className="font-bold">Email Confirmed:</span> {currentUser.emailConfirmed ? "Yes" : "No"}
            </p>
            <p className="text-lg">
              <span className="font-bold">Phone Confirmed:</span> {currentUser.phoneNumberConfirmed ? "Yes" : "No"}
            </p>
            <p className="text-lg">
              <span className="font-bold">Roles:</span> {currentUser.roles.join(", ")}
            </p>
            <p className="text-lg">
              <span className="font-bold">Password Configured:</span> {currentUser.passwordConfigured ? "Yes" : "No"}
            </p>
          </div>
        </div>
      ) : (
        <p className="text-center text-lg mb-4 text-default-800">No user logged in</p>
      )}
      {currentUser ? (
        <Button
          onPress={handleLogout}
          className="mt-6 rounded-full shadow-lg"
          color="danger"
        >
          Sign Out
        </Button>
      ) : (
        <Button
          onPress={handleSignIn}
          className="mt-6 rounded-full shadow-lg"
          color="primary"
        >
          Sign In
        </Button>
      )}
    </div>
  );
}
