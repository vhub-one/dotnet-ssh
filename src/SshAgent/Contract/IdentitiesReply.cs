using Common.Buffers;
using Common.Buffers.Extensions;
using System.Buffers;

namespace SshAgent.Contract
{
    public class IdentitiesReply
    {
        public IEnumerable<IdentityKey> Keys { get; set; }

        public void WriteTo(IBufferWriter<byte> writer)
        {
            var keys = Keys.ToList();
            var keysCount = (uint)keys.Count;

            // Write keys count
            writer.WriteUInt32BigEndian(keysCount);

            foreach (var key in keys)
            {
                key.WriteTo(writer);
            }
        }

        public static IdentitiesReply ReadFrom(IBufferReader<byte> reader)
        {
            var keysCount = reader.ReadUInt32BigEndian();
            var keys = new IdentityKey[keysCount];

            for (var index = 0; index < keysCount; ++index)
            {
                keys[index] = IdentityKey.ReadFrom(reader);
            }

            return new IdentitiesReply
            {
                Keys = keys
            };
        }
    }
}