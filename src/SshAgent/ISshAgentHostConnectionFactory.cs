
namespace SshAgent
{
    public interface ISshAgentHostConnectionFactory
    {
        ValueTask<ISshAgentHostConnection> AcceptAsync(CancellationToken token);
    }
}