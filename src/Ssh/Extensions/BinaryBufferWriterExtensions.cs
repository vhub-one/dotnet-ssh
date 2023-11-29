using Common.Buffers.Extensions;
using System.Buffers;
using System.Text;

namespace Ssh.Extensions
{
    public static class BinaryBufferWriterExtensions
    {
        public static void WriteStringBytes(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
        {
            writer.WriteUInt32BigEndian((uint)value.Length);
            writer.Write(value);
        }

        public static void WriteString(this IBufferWriter<byte> writer, string value, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var valueBytes = encoding.GetBytes(value);

            writer.WriteStringBytes(valueBytes);
        }

        public static void WriteMpInt(this IBufferWriter<byte> writer, ReadOnlySpan<byte> bytes)
        {
            var bytesLength = (uint)bytes.Length;

            if (bytesLength == 1 && bytes[0] == 0)
            {
                writer.WriteUInt32BigEndian(0);
            }
            else
            {
                var high = (bytes[0] & 0x80) != 0;

                if (high)
                {
                    writer.WriteUInt32BigEndian(bytesLength + 1);
                    writer.Write((byte)0);
                }
                else
                {
                    writer.WriteUInt32BigEndian(bytesLength);
                }

                writer.Write(bytes);
            }
        }
    }
}