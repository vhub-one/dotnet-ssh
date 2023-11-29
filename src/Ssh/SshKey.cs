using System.Buffers;

namespace Ssh
{
    public abstract class SshKey
    {
        public abstract string Algorithm { get; }

        public abstract void WriteTo(IBufferWriter<byte> writer);
    }
}
