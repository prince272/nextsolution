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


        public async Task<ValidationResult> ValidateAsync<TModel>(TModel model) where TModel : class
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

                var modelValidationResult = await modelValidator.ValidateAsync(model);
                var errors = modelValidationResult.Errors
                                                  .GroupBy(e => e.PropertyName)
                                                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                var isValid = modelValidationResult.IsValid;

                return new ValidationResult { IsValid = isValid, Errors = errors };
            }
        }
    }

    public interface IModelValidator
    {
        Task<ValidationResult> ValidateAsync<TModel>(TModel model) where TModel : class;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }

        public string Message { get; set; } = string.Empty;

        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
