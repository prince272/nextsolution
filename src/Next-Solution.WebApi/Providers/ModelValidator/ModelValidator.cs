using FluentValidation;

namespace Next_Solution.WebApi.Providers.ModelValidator
{
    public class ModelValidator : IModelValidator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ModelValidator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }


        public async Task<ValidationResult> ValidateAsync<TModel>(TModel model, Func<Dictionary<string, string[]>, Task>? custom = null) where TModel : class
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;

                var modelValidator = scopedServiceProvider.GetService<IValidator<TModel>>();

                if (modelValidator == null)
                {
                    throw new InvalidOperationException($"No validator found for type {typeof(TModel).FullName}");
                }

                var errors = (await modelValidator.ValidateAsync(model)).Errors
                                                  .GroupBy(e => e.PropertyName)
                                                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                if (custom != null) await custom.Invoke(errors);

                var isValid = errors.Count == 0;

                return new ValidationResult
                {
                    IsValid = isValid,
                    Errors = errors,
                    Message = isValid switch
                    {
                        true => "Validation successful.",
                        false => errors.Count == 1 ? errors.First().Value.FirstOrDefault() ?? "One or more errors occurred." : "One or more errors occurred."
                    }
                };
            }
        }
    }

    public interface IModelValidator
    {
        Task<ValidationResult> ValidateAsync<TModel>(TModel model, Func<Dictionary<string, string[]>, Task>? custom = null) where TModel : class;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }

        public string Message { get; set; } = string.Empty;

        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
