using System.IO;
using System.Text;

namespace Automate.CLI.Extensions
{
    public static class StreamExtensions
    {
        private const int DefaultBufferSize = 8 * 1024;

        public static string ReadToEnd(this MemoryStream ms)
        {
            return ReadToEnd(ms, Encoding.UTF8);
        }

        private static string ReadToEnd(this MemoryStream ms, Encoding encoding)
        {
            ms.Position = 0;

            if (ms.TryGetBuffer(out var buffer))
            {
                return encoding.GetString(buffer.Array!, buffer.Offset, buffer.Count);
            }

            using var reader = new StreamReader(ms, encoding, true, DefaultBufferSize, true);
            return reader.ReadToEnd();
        }
    }
}