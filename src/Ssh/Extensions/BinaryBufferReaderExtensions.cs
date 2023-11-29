using Common.Buffers;
using Common.Buffers.Extensions;
using System.Text;

namespace Ssh.Extensions
{
    public static class BinaryBufferReaderExtensions
    {
        public static ReadOnlyMemory<byte> ReadStringBytes(this IBufferReader<byte> reader)
        {
            var bytesLength = (int)reader.ReadUInt32BigEndian();
            var bytes = reader.Memory[..bytesLength];

            reader.Advance(bytesLength);

            return bytes;
        }

        public static string ReadString(this IBufferReader<byte> reader, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var bytesLength = (int)reader.ReadUInt32BigEndian();
            var bytes = reader.Memory.Span[..bytesLength];

            reader.Advance(bytesLength);

            return encoding.GetString(bytes);
        }

        public static ReadOnlyMemory<byte> ReadMpInt(this IBufferReader<byte> reader)
        {
            var bytes = reader.ReadStringBytes();

            if (bytes.Length == 0)
            {
                return new byte[] { 0 };
            }

            if (bytes.Span[0] == 0)
            {
                bytes = bytes[1..];
            }

            return bytes;
        }
    }
}
