
namespace SshAgent
{
    public abstract class SshAgentException : Exception
    {
        protected SshAgentException(string message, Exception ex = null)
            : base(message, ex)
        {
        }
    }

    public class SshAgentCancelledException : SshAgentException
    {
        public SshAgentCancelledException(Exception ex)
            : base("Operation cancelled by user", ex)
        {
        }
    }

    public class SshAgentNotAvailableException : SshAgentException
    {
        public SshAgentNotAvailableException(Exception ex = null)
            : base("Agent is not available", ex)
        {
        }
    }

    public class SshAgentNotReadyException : SshAgentException
    {
        public SshAgentNotReadyException(Exception ex)
            : base("Agent is not ready", ex)
        {
        }
    }

    public class SshAgentConnectionClosedException : SshAgentException
    {
        public SshAgentConnectionClosedException(Exception ex = null)
            : base("Connection is closed", ex)
        {
        }
    }
}