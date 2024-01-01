using SshAgent.Contract;

namespace SshAgent
{
    public interface ISshAgentHostConnection : IAsyncDisposable
    {
        ValueTask<AgentMessage> ReadMessageAsync(CancellationToken token);
        ValueTask WriteMessageAsync(AgentMessage message, CancellationToken token);
    }
}