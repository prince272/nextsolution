using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Medias
{
    public class DeleteMediaByFileIdForm
    {
        public string FileId { get; set; } = default!;

        public class Validator : AbstractValidator<DeleteMediaByFileIdForm>
        {
            public Validator()
            {
            }
        }
    }
}
