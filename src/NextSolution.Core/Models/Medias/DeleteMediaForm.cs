using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Medias
{
    public class DeleteMediaForm
    {
        public long Id { get; set; }
    }

    public class DeleteMediaByFileIdFormValidator : AbstractValidator<DeleteMediaForm>
    {
        public DeleteMediaByFileIdFormValidator()
        {
        }
    }
}
