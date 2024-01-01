using Common.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SshAgent.Contract;

namespace SshAgent.Proxy
{
    public class SshAgentProxy : ISshAgent
    {
        private readonly IServiceMap<ISshAgent> _services;
        private readonly IOptions<SshAgentProxyOptions> _optionsAccessor;
        private readonly ILogger<SshAgentProxy> _logger;

        public SshAgentProxy(IServiceMap<ISshAgent> services, IOptions<SshAgentProxyOptions> optionsAccessor, ILogger<SshAgentProxy> logger)
        {
            _services = services;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        public IEnumerable<ISshAgent> Agents
        {
            get
            {
                var options = _optionsAccessor.Value;

                if (options == null ||
                    options.SshAgentsOrder == null)
                {
                    throw new InvalidOperationException("Configuration is missing for [SshAgentAggregatorOptions]");
                }

                foreach (var agentName in options.SshAgentsOrder)
                {
                    yield return _services.Get(agentName);
                }
            }
        }

        public async ValueTask<IdentitiesReply> RequestIdentitiesAsync(CancellationToken token)
        {
            var replyKeys = new Dictionary<string, IdentityKey>();

            foreach (var agent in Agents)
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
            foreach (var agent in Agents)
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