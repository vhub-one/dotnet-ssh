using Microsoft.Extensions.Options;

namespace SshAgent.Transport.Socket
{
    using System.Net;
    using System.Net.Sockets;

    public class SocketSshAgentClient : StreamSshAgentClient
    {
        private readonly IOptions<SocketSshAgentClientOptions> _optionsAccessor;

        public SocketSshAgentClient(IOptions<SocketSshAgentClientOptions> options)
        {
            _optionsAccessor = options;
        }

        protected override async ValueTask<TReply> WithConnection<TReply>(Func<StreamSshAgentHostConnection, ValueTask<TReply>> requestFunc, CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.Host == null || options.Port == 0)
            {
                throw new InvalidOperationException("Configuration for SocketSshAgentClient is missing");
            }

            var endpoint = new DnsEndPoint(options.Host, options.Port);

            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

            await socket.ConnectAsync(endpoint, token);
            await using var socketStream = new NetworkStream(socket);
            await using var socketStreamConnection = new StreamSshAgentHostConnection(socketStream);

            return await requestFunc(socketStreamConnection);
        }
    }
}