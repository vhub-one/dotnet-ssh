using SshAgent.Contract;

namespace SshAgent
{
    public interface ISshAgentConnection : IAsyncDisposable
    {
        ValueTask<AgentMessage> ReadMessageAsync(CancellationToken token);
        ValueTask WriteMessageAsync(AgentMessage message, CancellationToken token);
    }
}