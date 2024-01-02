using Microsoft.Extensions.Hosting;
using SshAgent;

namespace SshAgentProxyService
{
    public class SshAgentProxyBackgroundService : BackgroundService
    {
        private readonly SshAgentService _sshAgentProxy;

        public SshAgentProxyBackgroundService(SshAgentService sshAgentProxy)
        {
            _sshAgentProxy = sshAgentProxy;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _sshAgentProxy.RunAsync(token);
        }
    }
}
