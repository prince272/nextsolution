using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Repositories;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Data.Repositories;
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
        private readonly UserManager<User> _userManager;

        public UserContext(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public string? UserAgent
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent;
            }
        }

        public long? UserId
        {
            get
            {
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user == null) return null;
                var userIdString = _userManager.GetUserId(user);
                return long.TryParse(userIdString, out long userId) ? userId : null;
            }
        }

        public string? DeviceId
        {
            get
            {
                if (ValidationHelper.TryParseUserAgent(UserAgent, out var userAgent))
                {
                    var values = new object?[]
                    {
                        userAgent.Device,
                        userAgent.UA,
                        userAgent.OS,
                        IpAddress
                    };

                    string deviceId;
                    deviceId = string.Join(",", values.Where((_) => !string.IsNullOrEmpty(_?.ToString()))).ToLower();
                    deviceId = AlgorithmHelper.GenerateMD5Hash(deviceId);
                    return deviceId;
                }
                else
                {
                    return null;
                }
            }
        }

        public string? IpAddress
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress is IPAddress remoteIp ? 
                    remoteIp.Equals(IPAddress.IPv6Loopback) ? IPAddress.Loopback.ToString() : remoteIp.MapToIPv4().ToString() : null;
            }
        }
    }
}