## Getting Started

Follow these steps to get your development environment up and running.

### Prerequisites

Before you begin, ensure you have the following installed:

- [Visual Studio 2022 or later](https://visualstudio.microsoft.com/downloads/)
- [Visual Studio Code](https://code.visualstudio.com/) (optional)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (latest version)
- [Node.js v20](https://nodejs.org/en/) (latest version, only required if you are using Next.js or Expo)

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
    dotnet new nextsln -o YourProjectName --include-expo --include-next --configure-ngrok
   ```

   - `--include-expo`: Include Expo project in the solution (Ngrok is recommended for exposing APIs to your Expo app).
   - `--include-next`: Include Next.js project in the solution.
   - `--configure-ngrok`: Configure Ngrok tunneling (requires signing up at Ngrok to obtain your token and a custom domain).

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

3. **Set Up Your Mobile Development Environment (Expo Project Only):**

   Follow the detailed guide in the official Expo documentation to set up your mobile development environment:

   - [Set Up Your Expo Development Environment](https://docs.expo.dev/get-started/set-up-your-environment/)

   This guide will help you install all the necessary tools, configure your environment, and run your first Expo project.