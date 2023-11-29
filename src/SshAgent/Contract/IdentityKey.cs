using Common.Buffers;
using Ssh.Extensions;
using System.Buffers;

namespace SshAgent.Contract
{
    public class IdentityKey
    {
        public ReadOnlyMemory<byte> KeyBlob { get; set; }
        public string Comment { get; set; }

        public void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteStringBytes(KeyBlob.Span);
            writer.WriteString(Comment);
        }

        public static IdentityKey ReadFrom(IBufferReader<byte> reader)
        {
            return new IdentityKey
            {
                KeyBlob = reader.ReadStringBytes(),
                Comment = reader.ReadString()
            };
        }
    }
}