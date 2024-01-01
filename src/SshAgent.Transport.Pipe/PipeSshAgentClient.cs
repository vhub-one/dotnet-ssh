using Microsoft.Extensions.Options;
using System.IO.Pipes;

namespace SshAgent.Transport.Pipe
{
    public class PipeSshAgentClient : StreamSshAgentClient
    {
        private readonly IOptions<PipeSshAgentClientOptions> _optionsAccessor;

        public PipeSshAgentClient(IOptions<PipeSshAgentClientOptions> options)
        {
            _optionsAccessor = options;
        }

        protected override async ValueTask<TReply> WithConnection<TReply>(Func<StreamSshAgentHostConnection, ValueTask<TReply>> requestFunc, CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.PipeName == null)
            {
                throw new InvalidOperationException("Configuration for PipeSshAgentClient is missing");
            }

            await using var stream = new NamedPipeClientStream(options.PipeName);
            await stream.ConnectAsync(1000, token);
            await using var streamConnection = new StreamSshAgentHostConnection(stream);

            return await requestFunc(streamConnection);
        }
    }
}
