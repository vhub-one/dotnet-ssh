using Common.Buffers;
using Common.Buffers.Extensions;
using Ssh.Extensions;

namespace SshAgent.Contract
{
    public class ChallengeData
    {
        public ReadOnlyMemory<byte> SessionBlob { get; set; }

        public byte MessageType { get; set; }

        public string ServerUser { get; set; }
        public string Service { get; set; }
        public string AuthMethod { get; set; }

        public byte Flag { get; set; }

        public string Algorithm { get; set; }
        public ReadOnlyMemory<byte> KeyBlob { get; set; }

        public static ChallengeData ReadFrom(IBufferReader<byte> reader)
        {
            return new ChallengeData
            {
                SessionBlob = reader.ReadStringBytes(),
                MessageType = reader.Read(),
                ServerUser = reader.ReadString(),
                Service = reader.ReadString(),
                AuthMethod = reader.ReadString(),
                Flag = reader.Read(),
                Algorithm = reader.ReadString(),
                KeyBlob = reader.ReadStringBytes()
            };
        }
    }
}