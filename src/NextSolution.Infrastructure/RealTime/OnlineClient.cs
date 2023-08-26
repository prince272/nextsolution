using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Extensions.RealTime;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NextSolution.Infrastructure.RealTime
{
    /// <summary>
    /// Implements <see cref="IOnlineClient"/>.
    /// </summary>
    [Serializable]
    public class OnlineClient : IOnlineClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineClient"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public OnlineClient(string connectionId, string? ipAddress, string? deviceId, long? userId, string? userAgent)
        {
            ConnectionId = connectionId;
            IpAddress = ipAddress;
            DeviceId = deviceId;
            UserId = userId;
            UserAgent = userAgent;
            ConnectionTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Unique connection Id for this client.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// IP address of this client.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Id.
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// User Id.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// User Agent.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Connection establishment time for this client.
        /// </summary>
        public DateTimeOffset ConnectionTime { get; set; }

        /// <summary>
        /// Shortcut to set/get <see cref="Properties"/>.
        /// </summary>
        public object this[string key]
        {
            get { return Properties[key]; }
            set { Properties[key] = value; }
        }

        /// <summary>
        /// Can be used to add custom properties for this client.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get => _properties;
            set => _properties = value ?? throw new ArgumentNullException(nameof(value));
        }
        private Dictionary<string, object> _properties = new Dictionary<string, object>();


        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}