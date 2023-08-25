namespace NextSolution.Core.Extensions.RealTime
{
    public class OnlineUserEventArgs : OnlineClientEventArgs
    {
        public long UserId { get; }

        public OnlineUserEventArgs(long userId, IOnlineClient client)
            : base(client)
        {
            UserId = userId;
        }
    }
}