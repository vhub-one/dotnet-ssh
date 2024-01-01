using Microsoft.Extensions.Options;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SshAgent.Transport.Pipe
{
    public class PipeSshAgentHostConnectionFactory : ISshAgentHostConnectionFactory
    {
        private readonly IOptions<PipeSshAgentHostConnectionFactoryOptions> _optionsAccessor;

        public PipeSshAgentHostConnectionFactory(IOptions<PipeSshAgentHostConnectionFactoryOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public async ValueTask<ISshAgentHostConnection> AcceptAsync(CancellationToken token)
        {
            var pipeOptions = _optionsAccessor.Value;

            if (pipeOptions == null ||
                pipeOptions.PipeName == null)
            {
                throw new InvalidOperationException("Configuration for PipeSshAgentHostConnectionFactory is missing");
            }

            var pipeUser = WindowsIdentity.GetCurrent();

            // Limit access to the current user. This also has the effect
            // of allowing non-elevated processes to access the agent when
            // it is running as an elevated process.

            var pipeSecurity = new PipeSecurity();
            var pipeSecurityRule = new PipeAccessRule(
                pipeUser.User,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow
            );

            // Initialize security
            pipeSecurity.AddAccessRule(
                pipeSecurityRule
            );

            // Create new pipe
            var pipe = NamedPipeServerStreamAcl.Create(
                pipeOptions.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough | PipeOptions.Asynchronous,
                0,
                0,
                pipeSecurity
            );

            try
            {
                await pipe.WaitForConnectionAsync(token);
            }
            catch
            {
                // Unable to accept new connection => release pipe stream
                await pipe.DisposeAsync();

                // Propagate it up
                throw;
            }

            return new StreamSshAgentHostConnection(pipe);
        }
    }
}