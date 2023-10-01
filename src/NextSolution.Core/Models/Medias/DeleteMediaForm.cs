using FluentValidation;

namespace NextSolution.Core.Models.Medias
{
    public class DeleteMediaForm
    {
        public long Id { get; set; }
    }

    public class DeleteMediaFormValidator : AbstractValidator<DeleteMediaForm>
    {
        public DeleteMediaFormValidator()
        {
        }
    }
}
