export type AppConfig = typeof appConfig;

export const appConfig = {
  name: "Next.js + NextUI",
  description: "Make beautiful websites regardless of your design experience.",
  navItems: [
    {
      label: "Home",
      href: "/"
    },
    {
      label: "ChatGPT",
      href: "/chatbot"
    },
    {
      label: "About",
      href: "/about"
    }
  ],
  navMenuItems: [
    {
      label: "Home",
      href: "/"
    },
    {
      label: "ChatGPT",
      href: "/chatbot"
    },
    {
      label: "About",
      href: "/about"
    }
  ],
  links: {
    github: "https://github.com/nextui-org/nextui",
    twitter: "https://twitter.com/getnextui",
    docs: "https://nextui.org",
    discord: "https://discord.gg/9b6yyZKmH4",
    sponsor: "https://patreon.com/jrgarciadev"
  }
};
