using Common.Buffers;
using SshAgent.Contract;
using System.Buffers;

namespace SshAgent.Transport
{
    public abstract class StreamSshAgentClient : ISshAgent
    {
        public async ValueTask<IdentitiesReply> RequestIdentitiesAsync(CancellationToken token)
        {
            var message = new AgentMessage
            {
                MessageType = AgentMessageType.SSH_AGENTC_REQUEST_IDENTITIES,
                Message = ReadOnlyMemory<byte>.Empty
            };

            async ValueTask<IdentitiesReply> RequestIdentitiesInternalAsync(StreamSshAgentHostConnection connection)
            {
                await connection.WriteMessageAsync(message, token);

                var messageReply = await connection.ReadMessageAsync(token);
                var messageReplyReader = MemoryBufferReader.Create(messageReply.Message);

                return IdentitiesReply.ReadFrom(messageReplyReader);
            }

            return await WithConnection(RequestIdentitiesInternalAsync, token);
        }

        public async ValueTask<SignReply> SignAsync(SignRequest signRequest, CancellationToken token)
        {
            var messageWriter = new ArrayBufferWriter<byte>();

            signRequest.WriteTo(messageWriter);

            var message = new AgentMessage
            {
                MessageType = AgentMessageType.SSH_AGENTC_SIGN_REQUEST,
                Message = messageWriter.WrittenMemory
            };

            async ValueTask<SignReply> SignInternalAsync(StreamSshAgentHostConnection connection)
            {
                await connection.WriteMessageAsync(message, token);

                var messageReply = await connection.ReadMessageAsync(token);
                var messageReplyReader = MemoryBufferReader.Create(messageReply.Message);

                return SignReply.ReadFrom(messageReplyReader);
            }

            return await WithConnection(SignInternalAsync, token);
        }

        protected abstract ValueTask<TReply> WithConnection<TReply>(Func<StreamSshAgentHostConnection, ValueTask<TReply>> requestFunc, CancellationToken token);
        //{
        //    var options = _optionsAccessor.Value;

        //    if (options == null ||
        //        options.PipeName == null)
        //    {
        //        throw new InvalidOperationException("Configuration for PipeSshAgent is missing");
        //    }

        //    await using var stream = new NamedPipeClientStream(options.PipeName);
        //    await stream.ConnectAsync(1000, token);
        //    await using var streamConnection = new StreamSshAgentHostConnection(stream);

        //    return await requestFunc(streamConnection);
        //}
    }
}
