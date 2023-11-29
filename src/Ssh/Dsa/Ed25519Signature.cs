using Ssh.Extensions;
using System.Buffers;

namespace Ssh.Dsa
{
    public class Ed25519Signature : SshSignature
    {
        public override void WriteTo(IBufferWriter<byte> writer)
        {
            writer.WriteString(Ed25519Key.ALGORITHM);
            writer.WriteStringBytes(SignatureBlob.Span);
        }
    }
}