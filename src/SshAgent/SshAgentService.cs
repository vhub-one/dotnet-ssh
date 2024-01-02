using Common.Buffers;
using Microsoft.Extensions.Logging;
using SshAgent.Contract;
using System.Buffers;
using System.Collections.Concurrent;

namespace SshAgent
{
    public class SshAgentService
    {
        private readonly ISshAgent _agent;
        private readonly ISshAgentHostConnectionFactory _agentConnectionFactory;
        private readonly ILogger<SshAgentService> _logger;

        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
        private readonly ConcurrentDictionary<string, Task> _connections = new();

        public SshAgentService(ISshAgent agent, ISshAgentHostConnectionFactory agentConnectionFactory, ILogger<SshAgentService> logger)
        {
            _agent = agent;
            _agentConnectionFactory = agentConnectionFactory;

            _logger = logger;
        }

        public async ValueTask RunAsync(CancellationToken token = default)
        {
            try
            {
                var connections = _agentConnectionFactory.AcceptAsync(token);

                await foreach (var connection in connections)
                {
                    Task RunHandlerAsync()
                    {
                        return RunConnectionHandlerAsync(connection, token);
                    }

                    _connections[connection.ConnectionId] = Task.Run(RunHandlerAsync, token);
                }
            }
            finally
            {
                // Wait until all tasks are complete
                await Task.WhenAll(_connections.Values);
            }
        }

        private async Task RunConnectionHandlerAsync(ISshAgentHostConnection connection, CancellationToken token)
        {
            _logger.LogInformation("> client connected");

            try
            {
                await using (connection)
                {
                    while (true)
                    {
                        var message = await connection.ReadMessageAsync(token);
                        var messageReply = default(AgentMessage);

                        using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            // Limit with max handling time
                            tokenSource.CancelAfter(_timeout);

                            try
                            {
                                switch (message.MessageType)
                                {
                                    case AgentMessageType.SSH_AGENTC_REQUEST_IDENTITIES:

                                        messageReply = await HandleIdentitiesRequestAsync(message, tokenSource.Token);
                                        break;

                                    case AgentMessageType.SSH_AGENTC_SIGN_REQUEST:

                                        messageReply = await HandleSignRequestAsync(message, tokenSource.Token);
                                        break;
                                }
                            }
                            catch (SshAgentCancelledException)
                            {
                                _logger.LogInformation("< operation was cancelled");
                            }
                            catch (SshAgentNotReadyException)
                            {
                                _logger.LogInformation("< agent is not ready");
                            }
                            catch (SshAgentNotAvailableException)
                            {
                                _logger.LogInformation("< agent is not available");
                            }
                            catch (Exception ex)
                            {
                                if (ex is OperationCanceledException oce && oce.CancellationToken == tokenSource.Token)
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        _logger.LogInformation("< agent stopped");
                                    }
                                    else
                                    {
                                        _logger.LogInformation("< agent timeout");
                                    }
                                }
                                else
                                {
                                    _logger.LogError(ex, "Unable to handle request");
                                }
                            }
                        }

                        if (messageReply == null)
                        {
                            messageReply = new AgentMessage
                            {
                                MessageType = AgentMessageType.SSH_AGENT_FAILURE
                            };
                        }

                        _logger.LogDebug("< sending response to client");

                        // Send result
                        await connection.WriteMessageAsync(messageReply, token);
                    }
                }
            }
            catch (SshAgentConnectionClosedException)
            {
                // Connection has been closed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to proceed request");
            }
            finally
            {
                _connections.TryRemove(connection.ConnectionId, out _);
            }

            _logger.LogInformation("< client disconnected");
        }

        private async ValueTask<AgentMessage> HandleIdentitiesRequestAsync(AgentMessage _, CancellationToken token)
        {
            _logger.LogDebug("> identities request received");

            var identitiesReply = await _agent.RequestIdentitiesAsync(token);
            var identitiesReplyWriter = new ArrayBufferWriter<byte>();

            identitiesReply.WriteTo(identitiesReplyWriter);

            _logger.LogDebug("< identities request handled");

            return new AgentMessage
            {
                MessageType = AgentMessageType.SSH_AGENT_IDENTITIES_ANSWER,
                Message = identitiesReplyWriter.WrittenMemory
            };
        }

        private async ValueTask<AgentMessage> HandleSignRequestAsync(AgentMessage message, CancellationToken token)
        {
            _logger.LogDebug("> sign request received");

            var singRequestReader = MemoryBufferReader.Create(message.Message);
            var signRequest = SignRequest.ReadFrom(singRequestReader);

            var challengeDataReader = MemoryBufferReader.Create(signRequest.DataBlob);
            var challengeData = ChallengeData.ReadFrom(challengeDataReader);

            if (challengeData != null)
            {
                _logger.LogInformation("> sign request from [{service}] service for [{username}] user", challengeData.Service, challengeData.ServerUser);
            }

            var signReply = await _agent.SignAsync(signRequest, token);
            var signReplyWriter = new ArrayBufferWriter<byte>();

            signReply.WriteTo(signReplyWriter);

            _logger.LogDebug("< sign request handled");

            return new AgentMessage
            {
                MessageType = AgentMessageType.SSH_AGENT_SIGN_RESPONSE,
                Message = signReplyWriter.WrittenMemory
            };
        }
    }
}