
namespace SshAgent
{
    public interface ISshAgentConnectionFactory
    {
        ValueTask<ISshAgentConnection> AcceptAsync(CancellationToken token);
    }
}