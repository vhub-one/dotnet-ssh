using Ssh.Extensions;
using System.Buffers;

namespace Ssh.Dsa
{
    public class EcP256Signature : SshSignature
    {
        public override void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteString(EcP256Key.ALGORITHM);
            writer.WriteStringBytes(SignatureBlob.Span);
        }
    }
}