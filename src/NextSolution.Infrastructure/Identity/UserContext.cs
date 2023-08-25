using Microsoft.AspNetCore.Http;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string DeviceId
        {
            get
            {
                HttpContext? context = _httpContextAccessor.HttpContext;
                string deviceId;

                if (ValidationHelper.TryParseUserAgent(context?.Request.Headers.UserAgent, out var userAgent))
                {
                    var values = new object?[]
                    {
                        userAgent.Device,
                        userAgent.UA,
                        userAgent.OS,
                        context?.Connection?.RemoteIpAddress is IPAddress remoteIp ? remoteIp.Equals(IPAddress.IPv6Loopback) ? IPAddress.Loopback : remoteIp.MapToIPv4() : null
                    };

                    deviceId = string.Join(",", values.Where((_) => !string.IsNullOrEmpty(_?.ToString()))).ToLower();
                }
                else deviceId = "Unknown";


                deviceId = AlgorithmHelper.GenerateSHA256Hash(deviceId);
                return deviceId;
            }
        }

        public ClaimsPrincipal? User => _httpContextAccessor?.HttpContext?.User;
    }
}