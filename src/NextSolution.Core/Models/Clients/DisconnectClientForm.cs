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

        public class Validator : AbstractValidator<DisconnectClientForm>
        {
            public Validator()
            {
                RuleFor(_ => _.ConnectionId).NotEmpty();
            }
        }
    }
}
