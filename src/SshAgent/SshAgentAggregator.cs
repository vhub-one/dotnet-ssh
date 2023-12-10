using Microsoft.Extensions.Logging;
using SshAgent.Contract;

namespace SshAgent
{
    public class SshAgentAggregator : ISshAgent
    {
        private readonly ILogger<SshAgentAggregator> _logger;
        private readonly ISshAgent[] _agents;

        public SshAgentAggregator(ILogger<SshAgentAggregator> logger, params ISshAgent[] agents)
        {
            _logger = logger;
            _agents = agents;
        }

        public async ValueTask<IdentitiesReply> RequestIdentitiesAsync(CancellationToken token)
        {
            var replyKeys = new Dictionary<string, IdentityKey>();

            foreach (var agent in _agents)
            {
                var agentIdentities = default(IdentitiesReply);

                try
                {
                    agentIdentities = await agent.RequestIdentitiesAsync(token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to request indentities from agent");
                }

                if (agentIdentities != null)
                {
                    foreach (var identityKey in agentIdentities.Keys)
                    {
                        var replyKeyB64 = Convert.ToBase64String(identityKey.KeyBlob.Span);

                        if (replyKeyB64 != null)
                        {
                            replyKeys.TryAdd(replyKeyB64, identityKey);
                        }
                    }
                }
            }

            return new IdentitiesReply
            {
                Keys = replyKeys.Values
            };
        }

        public async ValueTask<SignReply> SignAsync(SignRequest signRequest, CancellationToken token)
        {
            foreach (var agent in _agents)
            {

                var agentIdentities = default(IdentitiesReply);

                try
                {
                    agentIdentities = await agent.RequestIdentitiesAsync(token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to request indentities from agent");
                }

                if (agentIdentities != null)
                {
                    foreach (var identityKey in agentIdentities.Keys)
                    {
                        if (identityKey.KeyBlob.Span.SequenceEqual(signRequest.KeyBlob.Span))
                        {
                            // Forward sign request to the first supported agent
                            return await agent.SignAsync(signRequest, token);
                        }
                    }
                }
            }

            throw new SshAgentNotAvailableException();
        }
    }
}
