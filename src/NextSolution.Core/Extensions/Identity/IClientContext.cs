using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IClientContext
    {
        string? DeviceId { get; }

        string? IpAddress { get; }

        long? UserId { get; }

        string? UserAgent { get; }

        string? Issuer { get; }

        string? Audience { get; }
    }
}
