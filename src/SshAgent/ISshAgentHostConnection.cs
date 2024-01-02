using SshAgent.Contract;

namespace SshAgent
{
    public interface ISshAgentHostConnection : IAsyncDisposable
    {
        string ConnectionId { get; }

        ValueTask<AgentMessage> ReadMessageAsync(CancellationToken token);
        ValueTask WriteMessageAsync(AgentMessage message, CancellationToken token);
    }
}