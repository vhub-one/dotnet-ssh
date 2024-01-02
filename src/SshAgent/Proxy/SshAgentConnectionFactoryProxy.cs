using Common.Service;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SshAgent.Proxy
{
    public class SshAgentConnectionFactoryProxy : ISshAgentHostConnectionFactory
    {
        private readonly IServiceMap<ISshAgentHostConnectionFactory> _services;
        private readonly ILogger<SshAgentConnectionFactoryProxy> _logger;

        public SshAgentConnectionFactoryProxy(IServiceMap<ISshAgentHostConnectionFactory> services, ILogger<SshAgentConnectionFactoryProxy> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async IAsyncEnumerable<ISshAgentHostConnection> AcceptAsync([EnumeratorCancellation] CancellationToken token)
        {
            using var tokenCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);

            var connections = Channel.CreateUnbounded<ISshAgentHostConnection>();
            var connectionReaderTasks = new Dictionary<string, Task>();

            try
            {
                foreach (var serviceEntry in _services)
                {
                    Task StartServiceAcceptTask()
                    {
                        return AcceptServiceConnectionAsync(serviceEntry, connections.Writer, tokenCancellation.Token);
                    }

                    connectionReaderTasks[serviceEntry.Name] = Task.Run(StartServiceAcceptTask, tokenCancellation.Token);
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

        private async Task AcceptServiceConnectionAsync(IServiceMapEntry<ISshAgentHostConnectionFactory> serviceEntry, ChannelWriter<ISshAgentHostConnection> connectionsChannel, CancellationToken token)
        {
            try
            {
                var connections = serviceEntry.Service.AcceptAsync(token);

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
                _logger.LogError(ex, "Unable to accept connection from [{service}] service", serviceEntry.Name);
            }
        }
    }
}