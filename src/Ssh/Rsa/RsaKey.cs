using Common.Buffers;
using Ssh.Extensions;
using System.Buffers;

namespace Ssh.Rsa
{
    public class RsaKey : SshKey
    {
        public const string ALGORITHM = "ssh-rsa";

        public ReadOnlyMemory<byte> E { get; set; }
        public ReadOnlyMemory<byte> N { get; set; }

        public override string Algorithm
        {
            get { return ALGORITHM; }
        }

        public override void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteString(ALGORITHM);
            writer.WriteMpInt(E.Span);
            writer.WriteMpInt(N.Span);
        }

        public static RsaKey ReadFrom(IBufferReader<byte> reader)
        {
            // Skip format
            var format = reader.ReadString();

            if (format != ALGORITHM)
            {
                throw new FormatException();
            }

            return new RsaKey
            {
                E = reader.ReadMpInt(),
                N = reader.ReadMpInt()
            };
        }
    }
}