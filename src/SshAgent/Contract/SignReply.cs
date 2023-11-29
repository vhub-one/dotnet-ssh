using Common.Buffers;
using Ssh.Extensions;
using System.Buffers;

namespace SshAgent.Contract
{
    public class SignReply 
    {
        public ReadOnlyMemory<byte> SignatureBlob { get; set; }

        public void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteStringBytes(SignatureBlob.Span);
        }

        public static SignReply ReadFrom(IBufferReader<byte> reader)
        {
            return new SignReply
            {
                SignatureBlob = reader.ReadStringBytes()
            };
        }
    }
}