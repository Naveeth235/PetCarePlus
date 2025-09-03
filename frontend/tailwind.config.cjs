/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./index.html", "./src/**/*.{ts,tsx,js,jsx}"],
  darkMode: "class",
  theme: {
    extend: {
      container: { center: true, padding: "1rem" },
    },
  },
  // plugins: [require("@tailwindcss/forms"), require("tailwindcss-animate")],
};
