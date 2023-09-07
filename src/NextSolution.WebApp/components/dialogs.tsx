import * as usersDialogs from "./app/dialogs/users";

const dialogs = Object.entries(usersDialogs).map(([name, Component]) => {
  const id = name.replace(/Modal$/, "").replace(/[A-Z]/g, (char, index) => (index !== 0 ? "-" : "") + char.toLowerCase());
  return { id, name, Component };
});

export { dialogs };
