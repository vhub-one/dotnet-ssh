using SshAgent.Contract;

namespace SshAgent
{
    public interface ISshAgent
    {
        public ValueTask<IdentitiesReply> RequestIdentitiesAsync(CancellationToken token);
        public ValueTask<SignReply> SignAsync(SignRequest signRequest, CancellationToken token);
    }
}