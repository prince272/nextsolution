using NextSolution.Core.Extensions.SmsSender;

namespace NextSolution.Infrastructure.SmsSender.Fake
{
    public class FakeSmsSender : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
