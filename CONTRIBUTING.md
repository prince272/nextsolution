# Contributing to NextSolution

We welcome contributions to the NextSolution project and appreciate your interest in improving the codebase. To ensure a smooth contribution process, please follow the steps below:

## Contribution Process

1. **Fork the Repository** 🍴  
   Start by creating a personal copy of the main repository on your GitHub account. This allows you to work on your own version of the project without affecting the original code.

2. **Create a New Feature Branch** 🌿  
   In your forked repository, create a separate branch specifically for your changes. This helps isolate your work and makes it easier to manage and review contributions. Use a descriptive name for your branch that reflects the feature or fix you're working on.

3. **Implement Your Changes** ✍️  
   Develop and test your feature or bug fix in your feature branch. Make sure your code follows the project's coding standards and best practices. Be sure to write tests to validate your changes and ensure that everything works as expected.

4. **Submit a Pull Request** 🔄  
   Once your changes are complete and fully tested, submit a pull request (PR) from your feature branch to the `master` branch of the main repository. Include a clear and concise description of the changes you made, why they are necessary, and any relevant context or documentation that might be helpful to reviewers.

5. **Code Review and Approval** 👩‍🏫  
   After submitting your pull request, the maintainers will review your code. They may request changes or ask for clarification. Engage in the review process and make any necessary updates. Once your PR is approved, it will be merged into the `master` branch.

## Additional Considerations

- **Stay Up-to-Date** 🔄  
  Regularly sync your fork with the `master` branch of the main repository to avoid conflicts and ensure you are working with the latest version of the codebase.

- **Write Clean Commits** 📝  
  Make sure your commits are clear and concise, following the repository's guidelines on commit messages. Avoid bundling unrelated changes into a single commit or pull request.

- **Follow the Project's Guidelines** 📜  
  Ensure you adhere to any contribution guidelines or code of conduct that the project may have.

## Commit Structure

**type(scope): subject**

- **type**: The type of change, such as `feat` for new features, `fix` for bug fixes, `refactor` for code improvements, `test` for test cases, `docs` for documentation, `chore` for other miscellaneous changes.
- **scope**: The area of the codebase affected by the change, such as `component`, `model`, `utils`, or `api`.
- **subject**: A concise and descriptive summary of the change, preferably 50 characters or less.

### Example
**type(scope): subject**

- **type**: The type of change, such as `feat` for new features, `fix` for bug fixes, `refactor` for code improvements, `test` for test cases, `docs` for documentation, `chore` for other miscellaneous changes.
- **scope**: The area of the codebase affected by the change, such as `component`, `model`, `utils`, or `api`.
- **subject**: A concise and descriptive summary of the change, preferably 50 characters or less.

### Example with Detailed Description (feat)

✨ feat(sign-in): Add Facebook sign-in and update theme toggle buttons

- Added Facebook sign-in button with icon and handling in `SignInMethodModal`.
- Updated Google sign-in button to use the new icon import from `@iconify-icons/logos`.
- Commented out theme toggle buttons in `Page` component.
- Improved logging and error handling in `SignInMethodModal`.
- Added `@iconify-icons/logos` dependency in `package.json`.

### Example without Detailed Description (docs)

📝 docs: Update API documentation for user authentication


