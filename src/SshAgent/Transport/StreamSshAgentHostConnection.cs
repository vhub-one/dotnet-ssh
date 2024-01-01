using Common.Buffers;
using SshAgent.Contract;
using System.Buffers;
using System.Buffers.Binary;

namespace SshAgent.Transport
{
    public class StreamSshAgentHostConnection : ISshAgentHostConnection
    {
        private readonly Stream _stream;

        public StreamSshAgentHostConnection(Stream stream)
        {
            _stream = stream;
        }

        public async ValueTask<AgentMessage> ReadMessageAsync(CancellationToken token)
        {
            try
            {
                var messageBytesLengthSpan = new byte[4];

                // Read message length
                await _stream.ReadExactlyAsync(messageBytesLengthSpan, token);

                var messageBytesLength = BinaryPrimitives.ReadUInt32BigEndian(messageBytesLengthSpan);
                var messageBytes = new byte[messageBytesLength];

                // Read message bytes
                await _stream.ReadExactlyAsync(messageBytes, token);

                // Deserialize message
                var messageReader = MemoryBufferReader.Create(messageBytes);
                var message = AgentMessage.ReadFrom(messageReader);

                return message;
            }
            catch (EndOfStreamException ex)
            {
                throw new SshAgentConnectionClosedException(ex);
            }
        }

        public async ValueTask WriteMessageAsync(AgentMessage message, CancellationToken token)
        {
            var messageWriter = new ArrayBufferWriter<byte>();
            var messageWriterLengthSpan = new byte[4];

            // Serialize message
            message.WriteTo(messageWriter);

            BinaryPrimitives.WriteUInt32BigEndian(messageWriterLengthSpan, (uint) messageWriter.WrittenCount);

            // Write message length
            await _stream.WriteAsync(messageWriterLengthSpan, token);

            // Write message
            await _stream.WriteAsync(messageWriter.WrittenMemory, token);
        }

        public async ValueTask DisposeAsync()
        {
            await _stream.DisposeAsync();
        }
    }
}