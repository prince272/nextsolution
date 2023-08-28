using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Clients
{
    public class ConnectClientForm
    {
        public string ConnectionId { get; set; } = default!;

        public class Validator : AbstractValidator<ConnectClientForm>
        {
            public Validator()
            {
                RuleFor(_ => _.ConnectionId).NotEmpty();
            }
        }
    }
}
