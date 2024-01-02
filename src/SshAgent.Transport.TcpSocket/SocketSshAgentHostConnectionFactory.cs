using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace SshAgent.Transport.TcpSocket
{
    public class SocketSshAgentHostConnectionFactory : ISshAgentHostConnectionFactory
    {
        private readonly IOptions<SocketSshAgentHostConnectionFactoryOptions> _optionsAccessor;

        public SocketSshAgentHostConnectionFactory(IOptions<SocketSshAgentHostConnectionFactoryOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public async IAsyncEnumerable<ISshAgentHostConnection> AcceptAsync([EnumeratorCancellation] CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.Host == null || options.Port == 0)
            {
                throw new InvalidOperationException("Configuration for SocketSshAgentHostConnectionFactory is missing");
            }

            var endpointHosts = await Dns.GetHostAddressesAsync(options.Host, token);
            var endpointHost = endpointHosts.FirstOrDefault();

            if (endpointHost == null)
            {
                throw new InvalidOperationException("Unable to resolve host address");
            }

            var endpoint = new IPEndPoint(endpointHost, options.Port);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(endpoint);
            socket.Listen();

            while (true)
            {
                var handler = await socket.AcceptAsync(token);
                var handlerStream = new NetworkStream(handler, true);

                yield return new StreamSshAgentHostConnection(handlerStream);
            }
        }
    }
}