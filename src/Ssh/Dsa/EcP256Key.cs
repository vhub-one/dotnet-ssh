using Common.Buffers;
using Ssh.Extensions;
using System.Buffers;

namespace Ssh.Dsa
{
    public class EcP256Key : SshKey
    {
        public const string ALGORITHM = "ecdsa-sha2-nistp256";
        public const string ALGORITHM_CURVE = "nistp256";

        public ReadOnlyMemory<byte> Q { get; set; }

        public override string Algorithm
        {
            get { return ALGORITHM; }
        }

        public override void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteString(ALGORITHM);
            writer.WriteString(ALGORITHM_CURVE);
            writer.WriteStringBytes(Q.Span);
        }

        public static EcP256Key ReadFrom(IBufferReader<byte> reader)
        {
            // Skip format
            var algorithm = reader.ReadString();
            var algorithmHeader = reader.ReadString();

            if (algorithm != ALGORITHM ||
                algorithmHeader != ALGORITHM_CURVE)
            {
                throw new FormatException();
            }

            return new EcP256Key
            {
                Q = reader.ReadStringBytes()
            };
        }
    }
}