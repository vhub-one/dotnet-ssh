using Common.Buffers;
using Common.Buffers.Extensions;
using Ssh.Extensions;
using System.Buffers;

namespace SshAgent.Contract
{
    public class SignRequest
    {
        public ReadOnlyMemory<byte> KeyBlob { get; set; }
        public ReadOnlyMemory<byte> DataBlob { get; set; }
        public uint Flags { get; set; }

        public void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteStringBytes(KeyBlob.Span);
            writer.WriteStringBytes(DataBlob.Span);
            writer.WriteUInt32BigEndian(Flags);
        }

        public static SignRequest ReadFrom(IBufferReader<byte> reader)
        {
            return new SignRequest
            {
                KeyBlob = reader.ReadStringBytes(),
                DataBlob = reader.ReadStringBytes(),
                Flags = reader.ReadUInt32BigEndian()
            };
        }
    }
}