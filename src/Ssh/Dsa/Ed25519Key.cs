using Common.Buffers;
using Ssh.Extensions;
using System.Buffers;

namespace Ssh.Dsa
{
    public class Ed25519Key : SshKey
    {
        public const string ALGORITHM = "ssh-ed25519";

        public ReadOnlyMemory<byte> A { get; set; }

        public override string Algorithm
        {
            get { return ALGORITHM; }
        }

        public override void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteString(ALGORITHM);
            writer.WriteStringBytes(A.Span);
        }

        public static Ed25519Key ReadFrom(IBufferReader<byte> reader)
        {
            // Skip format
            var algorithm = reader.ReadString();

            if (algorithm != ALGORITHM)
            {
                throw new FormatException();
            }

            return new Ed25519Key
            {
                A = reader.ReadStringBytes()
            };
        }
    }
}