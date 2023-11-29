
namespace SshAgent.Contract
{
    public static class AgentMessageType
    {
        // Requests from client to agent for protocol 1 key operations
        public const byte SSH_AGENTC_REQUEST_RSA_IDENTITIES = 1;
        public const byte SSH_AGENTC_RSA_CHALLENGE = 3;
        public const byte SSH_AGENTC_ADD_RSA_IDENTITY = 7;
        public const byte SSH_AGENTC_REMOVE_RSA_IDENTITY = 8;
        public const byte SSH_AGENTC_REMOVE_ALL_RSA_IDENTITIES = 9;
        public const byte SSH_AGENTC_ADD_RSA_ID_CONSTRAINED = 24;

        // Requests from client to agent for protocol 2 key operations
        public const byte SSH_AGENTC_REQUEST_IDENTITIES = 11;
        public const byte SSH_AGENTC_SIGN_REQUEST = 13;
        public const byte SSH_AGENTC_ADD_IDENTITY = 17;
        public const byte SSH_AGENTC_REMOVE_IDENTITY = 18;
        public const byte SSH_AGENTC_REMOVE_ALL_IDENTITIES = 19;
        public const byte SSH_AGENTC_ADD_ID_CONSTRAINED = 25;
        public const byte SSH_AGENTC_ADD_SMARTCARD_KEY = 20;
        public const byte SSH_AGENTC_REMOVE_SMARTCARD_KEY = 21;
        public const byte SSH_AGENTC_LOCK = 22;
        public const byte SSH_AGENTC_UNLOCK = 23;
        public const byte SSH_AGENTC_ADD_SMARTCARD_KEY_CONSTRAINED = 26;
        public const byte SSH_AGENTC_EXTENSION = 27;

        // Replies from agent to client for protocol 1 key operations
        public const byte SSH_AGENT_RSA_IDENTITIES_ANSWER = 2;
        public const byte SSH_AGENT_RSA_RESPONSE = 4;

        // Generic replies from agent to client
        public const byte SSH_AGENT_FAILURE = 5;
        public const byte SSH_AGENT_SUCCESS = 6;

        // Replies from agent to client for protocol 2 key operations
        public const byte SSH_AGENT_IDENTITIES_ANSWER = 12;
        public const byte SSH_AGENT_SIGN_RESPONSE = 14;
        public const byte SSH_AGENT_EXTENSION_FAILURE = 28;
    }
}