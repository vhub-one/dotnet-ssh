
namespace SshAgent
{
    public interface ISshAgentHostConnectionFactory
    {
        IAsyncEnumerable<ISshAgentHostConnection> AcceptAsync(CancellationToken token);
    }
}