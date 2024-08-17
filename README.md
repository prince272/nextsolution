<div align="center">

[![Next Solution Template](./docs/images/banner.jpg)](https://github.com/prince272/nextsolution)

[![NuGet Downloads](https://img.shields.io/nuget/dt/NextSolution.Template?color=%2317c964)](https://www.nuget.org/packages/NextSolution.Template)
[![GitHub License](https://img.shields.io/github/license/prince272/nextsolution?color=%2317c964)](https://github.com/prince272/nextsolution/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/NextSolution.Template?color=%237828c8)](https://www.nuget.org/packages/NextSolution.Template)

</div>


# NextSolution V2 (ASP.NET API with Next.js, and Expo) 🚀

This starter template combines an [ASP.NET API](https://dotnet.microsoft.com/apps/aspnet) 🖥️ with a [Next.js (React)](https://nextjs.org/) web application 🌐 and an [Expo (React Native)](https://expo.dev/) mobile app 📱 to provide a solid foundation for building full-stack applications with powerful APIs and responsive web and mobile interfaces.

## Motivation

🚀 After releasing the initial version of the [NextSolution template](https://github.com/prince272/nextsolution) on [NuGet](https://www.nuget.org/packages/NextSolution.Template), I observed a gradual increase in both NuGet downloads and GitHub stars over a few months. This positive feedback was a significant motivator for me to develop V2, which features an improved codebase and more organized patterns.

🤔 If you find the next solution is useful, please give it a star ⭐ and consider sponsoring. Your support helps me keep improving it. Thanks!

## Demo

![Next Solution Template Demo](./docs/demo.png)

## Getting Started

Follow these steps to get your development environment up and running.

### Prerequisites

Before you begin, ensure you have the following installed:

- [Visual Studio 2022 or later](https://visualstudio.microsoft.com/downloads/)
- [Visual Studio Code](https://code.visualstudio.com/) (optional)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (latest version)
- [Node.js v20](https://nodejs.org/en/) (latest version,  only required if you are using Next.js or Expo)
  
### Installation

To set up your project using the NuGet template, follow these steps:

1. **Install the NuGet template:**

   Open your terminal or command prompt and run the following command to install the template:

   ```bash
   dotnet new --install NextSolution.Template::2.0.0
   ```
   

2. **Create a New Project Using the Template:**

   After installing the template, you can either continue using the terminal or command prompt, or switch to Visual Studio to create the new project:

   - **Using the terminal or command prompt:**

     Generate a new project by running the following command. Replace `YourProjectName` with your desired project name:

     ```bash
     dotnet new nextsln -o YourProjectName
     ```

     Move into the newly created project directory:

     ```bash
     cd YourProjectName
     ```

     Restore the project dependencies:

     ```bash
     dotnet restore
     ```

     Open the solution file in Visual Studio:

     ```bash
     start YourProjectName.sln
     ```

   - **Using Visual Studio:**

     Open Visual Studio, select "Create a new project," search for "Next Solution," select it, and follow the prompts to create your project.

## Tools, Frameworks & Libraries

This template was built using a variety of powerful frameworks and tools, including those listed below and many others:

[![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/) [![Ngrok](https://img.shields.io/badge/ngrok-003F5C?style=for-the-badge&logo=ngrok&logoColor=white)](https://ngrok.com/) [![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white)](https://jwt.io/) [![Entity Framework](https://img.shields.io/badge/Entity_Framework-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/ef/) [![AutoMapper](https://img.shields.io/badge/AutoMapper-000000?style=for-the-badge&logo=automapper&logoColor=white)](https://automapper.org/) [![FluentValidation](https://img.shields.io/badge/FluentValidation-000000?style=for-the-badge&logo=fluentvalidation&logoColor=white)](https://fluentvalidation.net/) [![Flurl](https://img.shields.io/badge/Flurl-000000?style=for-the-badge&logo=flurl&logoColor=white)](https://flurl.dev/) [![Humanizer](https://img.shields.io/badge/Humanizer-000000?style=for-the-badge&logo=humanizer&logoColor=white)](https://github.com/Humanizr/Humanizer) [![libphonenumber-csharp](https://img.shields.io/badge/libphonenumber--csharp-000000?style=for-the-badge&logo=libphonenumber&logoColor=white)](https://github.com/libphonenumber/libphonenumber-csharp) [![MailKit](https://img.shields.io/badge/MailKit-00B9F2?style=for-the-badge&logo=mailkit&logoColor=white)](https://github.com/jstedfast/MailKit) [![OAuth](https://img.shields.io/badge/OAuth-000000?style=for-the-badge&logo=oauth&logoColor=white)](https://oauth.net/) [![Serilog](https://img.shields.io/badge/Serilog-2F2F2F?style=for-the-badge&logo=serilog&logoColor=white)](https://serilog.net/) [![Twilio](https://img.shields.io/badge/Twilio-000000?style=for-the-badge&logo=twilio&logoColor=white)](https://www.twilio.com/) [![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/) [![React.js](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://reactjs.org/) [![React Native](https://img.shields.io/badge/React_Native-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://reactnative.dev/) [![React Navigation](https://img.shields.io/badge/React_Navigation-000000?style=for-the-badge&logo=reactnavigation&logoColor=white)](https://reactnavigation.org/) [![Axios](https://img.shields.io/badge/Axios-5A29E3?style=for-the-badge&logo=axios&logoColor=white)](https://axios-http.com/) [![Expo Dev](https://img.shields.io/badge/Expo_Dev-000020?style=for-the-badge&logo=expo&logoColor=white)](https://expo.dev/) [![lodash](https://img.shields.io/badge/Lodash-3492F2?style=for-the-badge&logo=lodash&logoColor=white)](https://lodash.com/) [![nativewind](https://img.shields.io/badge/NativeWind-000000?style=for-the-badge&logo=nativewind&logoColor=white)](https://nativewind.dev/) [![react-hook-form](https://img.shields.io/badge/React_Hook_Form-ECF5F6?style=for-the-badge&logo=reacthookform&logoColor=000000)](https://react-hook-form.com/) [![zustand](https://img.shields.io/badge/Zustand-FF4C60?style=for-the-badge&logo=zustand&logoColor=white)](https://github.com/pmndrs/zustand) [![Visual Studio Code](https://img.shields.io/badge/Visual_Studio_Code-007ACC?style=for-the-badge&logo=visualstudiocode&logoColor=white)](https://code.visualstudio.com/) [![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/) [![Android Studio](https://img.shields.io/badge/Android_Studio-3DDC84?style=for-the-badge&logo=androidstudio&logoColor=white)](https://developer.android.com/studio) [![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)](https://git-scm.com/) [![GitHub Copilot](https://img.shields.io/badge/GitHub_Copilot-2D5D7F?style=for-the-badge&logo=github&logoColor=white)](https://github.com/features/copilot) [![Node.js](https://img.shields.io/badge/Node.js-339933?style=for-the-badge&logo=node.js&logoColor=white)](https://nodejs.org/) [![React Native Paper](https://img.shields.io/badge/React_Native_Paper-000000?style=for-the-badge&logo=react&logoColor=white)](https://reactnativepaper.com/) [![NextUI](https://img.shields.io/badge/NextUI-000000?style=for-the-badge&logo=next&logoColor=white)](https://nextui.org/)


## License

This template is distributed under the MIT License. Please refer to the [LICENSE](./LICENSE.txt) for further details.

## Acknowledgments

With gratitude, I acknowledge these libraries, tools, and documentation which played a crucial role in the creation of this template.

**Documentation:**
- [React Official Site](https://react.dev/)
- [React Native Official Site](https://reactnative.dev/)
- [YouTube: React Native for Beginners](https://www.youtube.com/watch?v=0-S5a0eXPoc&t=1918s)
- [YouTube: Understanding React Native Performance](https://www.youtube.com/watch?v=lA_73_-n-V4)
- [W3Schools: TypeScript](https://www.w3schools.com/typescript/)

**Tools, Frameworks & Libraries:**
- [React Hook Form](https://react-hook-form.com/)
- [Tailwind CSS](https://tailwindcss.com)
- [React Native Paper](https://reactnativepaper.com/)
- [Expo with React Native](https://expo.dev/)
- [GitHub: FluffySpoon.Ngrok](https://github.com/ffMathy/FluffySpoon.Ngrok)
- [GitHub: ASP.NET Core JWT Authentication](https://github.com/VahidN/ASPNETCore2JwtAuthentication)

**Architecture & Design:**
- [GitHub: Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture)
- [UXWing: Free Icons](https://uxwing.com/)
- [Best-README-Template](https://github.com/othneildrew/Best-README-Template)
- [Gist: .NET Project Structure](https://gist.github.com/davidfowl/ed7564297c61fe9ab814)
- [GitHub: .NET Template Reference](https://github.com/dotnet/templating/wiki/Reference-for-template.json)
- [Gist: Conventional Commit Messages](https://gist.github.com/qoomon/5dfcdf8eec66a051ecd85625518cfd13?permalink_comment_id=4892033)
- [Gist: Git Commit Message Emoji](https://gist.github.com/parmentf/035de27d6ed1dce0b36a)