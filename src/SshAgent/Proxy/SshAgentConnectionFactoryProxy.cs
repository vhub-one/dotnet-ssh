using Common.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SshAgent.Proxy
{
    public class SshAgentConnectionFactoryProxy : ISshAgentHostConnectionFactory
    {
        private readonly IServiceMap<ISshAgentHostConnectionFactory> _services;
        private readonly ILogger<SshAgentConnectionFactoryProxy> _logger;
        private readonly IOptions<SshAgentConnectionFactoryProxyOptions> _optionsAccessor;

        public SshAgentConnectionFactoryProxy(IServiceMap<ISshAgentHostConnectionFactory> services, ILogger<SshAgentConnectionFactoryProxy> logger, IOptions<SshAgentConnectionFactoryProxyOptions> optionsAccessor)
        {
            _services = services;
            _logger = logger;
            _optionsAccessor = optionsAccessor;
        }

        public async IAsyncEnumerable<ISshAgentHostConnection> AcceptAsync([EnumeratorCancellation] CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.SshAgents == null)
            {
                throw new InvalidOperationException("Configuration is missing for [SshAgentConnectionFactoryProxy]");
            }

            using var tokenCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);

            var connections = Channel.CreateUnbounded<ISshAgentHostConnection>();
            var connectionReaderTasks = new Dictionary<string, Task>();

            try
            {
                foreach (var serviceName in options.SshAgents)
                {
                    var service = _services.Get(serviceName);

                    Task StartServiceAcceptTask()
                    {
                        return AcceptServiceConnectionAsync(serviceName, service, connections.Writer, tokenCancellation.Token);
                    }

                    connectionReaderTasks[serviceName] = Task.Run(StartServiceAcceptTask, tokenCancellation.Token);
                }

                while (true)
                {
                    var connection = await connections.Reader.ReadAsync(tokenCancellation.Token);

                    if (connection != null)
                    {
                        yield return connection;
                    }
                }
            }
            finally
            {
                tokenCancellation.Cancel();

                // Await all tasks are complete
                await Task.WhenAll(connectionReaderTasks.Values);
            }
        }

        private async Task AcceptServiceConnectionAsync(string serviceName, ISshAgentHostConnectionFactory service, ChannelWriter<ISshAgentHostConnection> connectionsChannel, CancellationToken token)
        {
            try
            {
                var connections = service.AcceptAsync(token);

                await foreach (var connection in connections)
                {
                    var success = connectionsChannel.TryWrite(connection);

                    if (success == false)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to accept connection from [{service}] service", serviceName);
            }
        }
    }
}