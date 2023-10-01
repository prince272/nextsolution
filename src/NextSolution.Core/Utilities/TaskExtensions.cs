namespace NextSolution.Core.Utilities
{
    // 5 useful extensions for Task<T> in .NET
    // source: https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e
    public static class TaskExtensions
    {
        // Use case: Fire and forget a task without waiting for its completion.
        // Example: SendEmailAsync().Forget(errorHandler => Console.WriteLine(errorHandler.Message));
        public static void Forget(this Task task, Action<Exception?>? errorHandler = null)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && errorHandler != null)
                    errorHandler(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        // Use case: Retry a task a specific number of times with a delay between retries.
        // Example: var result = await (() => GetResultAsync()).Retry(3, TimeSpan.FromSeconds(1));
        public static async Task<TResult> Retry<TResult>(this Func<Task<TResult>> taskFactory, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await taskFactory().ConfigureAwait(false);
                }
                catch
                {
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(delay).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // This line should never be reached, so it's better to throw an exception here
            throw new InvalidOperationException("Retry loop reached an unexpected state.");
        }

        // Use case: Execute a callback when a Task encounters an exception.
        // Example: await GetResultAsync().OnFailure(ex => Console.WriteLine(ex.Message));
        public static async Task OnFailure(this Task task, Action<Exception> onFailure)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                onFailure(ex);
            }
        }

        // Use case: Set a timeout for a task and cancel it if it exceeds the timeout.
        // Example: await GetResultAsync().WithTimeout(TimeSpan.FromSeconds(1));
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var delayTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
            if (completedTask == delayTask)
                throw new TimeoutException();

            await task;
        }

        // Use case: Use a fallback value when a task fails.
        // Example: var result = await GetResultAsync().Fallback("fallback");
        public static async Task<TResult> Fallback<TResult>(this Task<TResult> task, TResult fallbackValue)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return fallbackValue;
            }
        }
    }
}
