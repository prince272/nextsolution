"use client";

import React from "react";

import * as accounts from "./accounts";

const dialogs: { id: string; Component: React.ComponentType<any> }[] = [];

// Load accounts
Object.entries(accounts).forEach(([id, Component]) => {
  id = id.replace(/Modal$/, "").replace(/[A-Z]/g, (char, index) => (index !== 0 ? "-" : "") + char.toLowerCase());
  dialogs.push({ id, Component });
});

export { dialogs };
