using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace SshAgent.Transport.UnixSocket
{
    public class UnixSocketSshAgentClient : StreamSshAgentClient
    {
        private readonly IOptions<UnixSocketSshAgentClientOptions> _optionsAccessor;

        public UnixSocketSshAgentClient(IOptions<UnixSocketSshAgentClientOptions> options)
        {
            _optionsAccessor = options;
        }

        protected override async ValueTask<TReply> WithConnection<TReply>(Func<StreamSshAgentHostConnection, ValueTask<TReply>> requestFunc, CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.SocketPath == null)
            {
                throw new InvalidOperationException("Configuration for UnixSocketSshAgentClient is missing");
            }

            var endpoint = new UnixDomainSocketEndPoint(options.SocketPath);

            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

            await socket.ConnectAsync(endpoint, token);
            await using var socketStream = new NetworkStream(socket);
            await using var socketStreamConnection = new StreamSshAgentHostConnection(socketStream);

            return await requestFunc(socketStreamConnection);
        }
    }
}
