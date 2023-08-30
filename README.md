# NextSolution - ASP.NET API & Next.js Template 🚀

Welcome to NextSolution, a starter template that combines an ASP.NET API backend with a Next.js frontend. This template provides a foundation for building modern web applications with a powerful backend and a dynamic frontend. Below you'll find details about the template's structure and its key components.

## Table of Contents 📑

- [Introduction](#introduction)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
- [Backend Installation](#backend-installation)
- [Frontend Installation](#frontend-installation)
- [Backend Setup](#backend-setup)
  - [Configuration](#configuration)
  - [Authentication](#authentication)
  - [Database](#database)
  - [Email and SMS](#email-and-sms)
- [Frontend Setup](#frontend-setup)
  - [Scripts](#scripts)
  - [Dependencies](#dependencies)
  - [Dev Dependencies](#dev-dependencies)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [Support](#support)
  - [Sponsorship](#sponsorship)
- [Contact](#contact)
- [License](#license)

## Introduction 🌟

NextSolution is a template that brings together an ASP.NET API backend and a Next.js frontend. This combination offers the benefits of a robust backend with ASP.NET and a responsive, interactive frontend using Next.js.

## Getting Started 🚀

### Prerequisites 🛠️

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Node.js](https://nodejs.org/)
- [npm](https://www.npmjs.com/get-npm)

## Backend Installation ⚙️

1. Clone this repository: `git clone https://github.com/prince272/nextsolution.git`
2. Navigate to the backend directory: `cd NextSolution.WebApi`
3. Install backend dependencies: `dotnet restore`
4. Build and run the backend application: `dotnet run`

## Frontend Installation ⚙️

1. Return to the main directory: `cd nextsolution`
2. Navigate to the frontend directory: `cd NextSolution.WebApp`
3. Install frontend dependencies: `npm install`

## Backend Setup 🔧

The backend is organized into different projects for core functionality and infrastructure.

### Configuration ⚙️

The backend's configuration is managed through the `appsettings.json` file. Update settings such as database connection strings, authentication providers, and email settings based on your project's requirements.

### Authentication 🔑

The template supports user authentication and authorization using ASP.NET Identity. You can configure authentication settings in the `Startup.cs` file.

### Database 🗄️

The template uses Entity Framework Core for database operations. Database context is configured in the `AppDbContext.cs` file, and migrations are managed using Entity Framework's tools.

### Email and SMS 📧📱

Email sending is implemented using MailKit, and the template includes a fake SMS sender for testing.

## Frontend Setup 🔨

The frontend is built with Next.js, providing a fast and dynamic user experience.

### Scripts 📜

In the `NextSolution.WebApp` directory, you can use the following npm scripts:

- `dev`: Start the development server
- `dev:ssl`: Start the development server with SSL (useful for testing)
- `start`: Start the production server
- `lint`: Run ESLint
- `format:check`: Check code formatting using Prettier
- `format`: Format code using Prettier

### Dependencies 📦

The frontend relies on several dependencies, including:

- Next.js
- React
- Axios
- React Hook Form
- Tailwind CSS
- FilePond
- nextui
- libphonenumber-js
- lodash
- ....

### Dev Dependencies 🔧

Dev dependencies for the frontend include tools like Prettier, ESLint, and others. Refer to the `package.json` file for the complete list.

## Deployment 🚀

For deployment, follow standard procedures for deploying an ASP.NET application and a Next.js application. Configure environment-specific settings in the `appsettings.json` file and the frontend's environment variables.

## Contributing 👥

Contributions to this template are welcome. If you encounter issues or have suggestions, please open an issue on the [GitHub repository](https://github.com/prince272/nextsolution).

## Support 🙌

If you find this template helpful, consider supporting the project by contributing, giving it a star ⭐️ on GitHub, or sharing it with others who might benefit from it.

### Sponsorship 💖

If you're interested in sponsoring this project, please reach out to us via email or other communication channels. Your sponsorship helps ensure the continued development and maintenance of this template.

## Contact 📞

Feel free to reach out to me:

- ☎️ Mobile: +233 (55) 036 2337
- ✉️ Email: princeowusu.272@gmail.com
- 🌐 GitHub: [prince272](https://github.com/prince272)
- 💼 LinkedIn: [Prince Owusu](https://www.linkedin.com/in/prince-owusu-799438108)

## License 📄

This template is licensed under the [MIT License](https://github.com/prince272/nextsolution/blob/master/LICENSE.txt).
