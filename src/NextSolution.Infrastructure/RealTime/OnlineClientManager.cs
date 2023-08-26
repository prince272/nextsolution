using NextSolution.Core.Extensions.RealTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextSolution.Infrastructure.RealTime
{
    public class OnlineClientManager<T> : OnlineClientManager, IOnlineClientManager<T>
    {
        public OnlineClientManager(IOnlineClientStore<T> store) : base(store)
        {

        }
    }

    /// <summary>
    /// Implements <see cref="IOnlineClientManager"/>.
    /// </summary>
    public class OnlineClientManager : IOnlineClientManager
    {
        public event EventHandler<OnlineClientEventArgs>? ClientConnected;
        public event EventHandler<OnlineClientEventArgs>? ClientDisconnected;
        public event EventHandler<OnlineUserEventArgs>? UserConnected;
        public event EventHandler<OnlineUserEventArgs>? UserDisconnected;

        /// <summary>
        /// Online clients Store.
        /// </summary>
        protected readonly IOnlineClientStore Store;

        protected readonly object SyncObj = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlineClientManager"/> class.
        /// </summary>
        public OnlineClientManager(IOnlineClientStore store)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public virtual void Add(IOnlineClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            lock (SyncObj)
            {
                var userWasAlreadyOnline = false;
                var userId = client.UserId;

                if (userId.HasValue)
                {
                    userWasAlreadyOnline = IsOnline(userId.Value);
                }

                Store.Add(client);

                ClientConnected?.Invoke(this, new OnlineClientEventArgs(client));

                if (userId.HasValue && !userWasAlreadyOnline)
                {
                    UserConnected?.Invoke(this, new OnlineUserEventArgs(userId.Value, client));
                }
            }
        }

        public virtual bool Remove(IOnlineClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return Remove(client.ConnectionId);
        }

        public virtual bool Remove(string connectionId)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            lock (SyncObj)
            {
                var result = Store.TryRemove(connectionId, out var client);

                if (result)
                {
                    if (UserDisconnected != null)
                    {
                        var userId = client.UserId;

                        if (userId.HasValue && !IsOnline(userId.Value))
                        {
                            UserDisconnected.Invoke(this, new OnlineUserEventArgs(userId.Value, client));
                        }
                    }

                    ClientDisconnected?.Invoke(this, new OnlineClientEventArgs(client));
                }

                return result;
            }
        }

        public virtual IOnlineClient? GetByConnectionId(string connectionId)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            lock (SyncObj)
            {
                if (Store.TryGet(connectionId, out var client))
                {
                    return client;
                }

                return null;
            }
        }

        public virtual IReadOnlyList<IOnlineClient> GetAllClients()
        {
            lock (SyncObj)
            {
                return Store.GetAll();
            }

        }

        public virtual IReadOnlyList<IOnlineClient> GetAllByUserId(long userId)
        {
            return GetAllClients()
                 .Where(c => c.UserId == userId)
                 .ToImmutableList();
        }

        public virtual bool IsOnline(long userId)
        {
            return GetAllByUserId(userId).Any();
        }
    }
}