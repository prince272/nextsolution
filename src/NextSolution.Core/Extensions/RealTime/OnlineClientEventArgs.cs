using System;

namespace NextSolution.Core.Extensions.RealTime
{
    public class OnlineClientEventArgs : EventArgs
    {
        public IOnlineClient Client { get; }

        public OnlineClientEventArgs(IOnlineClient client)
        {
            Client = client;
        }
    }
}