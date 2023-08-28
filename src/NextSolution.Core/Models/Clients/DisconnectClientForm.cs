using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Clients
{
    public class DisconnectClientForm
    {
        public string ConnectionId { get; set; } = default!;
    }

    public class DisconnectClientFormValidator : AbstractValidator<DisconnectClientForm>
    {
        public DisconnectClientFormValidator()
        {
            RuleFor(_ => _.ConnectionId).NotEmpty();
        }
    }
}
