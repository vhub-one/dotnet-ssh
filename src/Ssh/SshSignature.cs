using System.Buffers;

namespace Ssh
{
    public abstract class SshSignature
    {
        public ReadOnlyMemory<byte> SignatureBlob { get; set; }

        public abstract void WriteTo(IBufferWriter<byte> writer);
    }
}
