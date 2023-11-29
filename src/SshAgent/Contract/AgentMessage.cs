using Common.Buffers;
using Common.Buffers.Extensions;
using System.Buffers;

namespace SshAgent.Contract
{
    public class AgentMessage
    {
        public byte MessageType { get; set; }
        public ReadOnlyMemory<byte> Message { get; set; }

        public void WriteTo(IBufferWriter<byte> writer)
        {
            writer.Write(MessageType);
            writer.Write(Message.Span);
        }

        public static AgentMessage ReadFrom(IBufferReader<byte> reader)
        {
            return new AgentMessage
            {
                MessageType = reader.Read(),
                Message = reader.ReadAll()
            };
        }
    }
}