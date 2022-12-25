namespace ProcessCommunication;

using System;
using System.Text;

/// <summary>
/// Provides tools to encode and decode strings transmitted over channels.
/// </summary>
public static class Converter
{
    /// <summary>
    /// Encodes a string.
    /// </summary>
    /// <param name="text">TRhe string to encode.</param>
    public static byte[] EncodeString(string text)
    {
        byte[] EncodedString = Encoding.UTF8.GetBytes(text);

        int DataLength = sizeof(int) + EncodedString.Length;
        byte[] EncodedStringWithHeader = new byte[DataLength];
        Array.Copy(BitConverter.GetBytes(DataLength), 0, EncodedStringWithHeader, 0, sizeof(int));
        Array.Copy(EncodedString, 0, EncodedStringWithHeader, sizeof(int), EncodedString.Length);

        return EncodedStringWithHeader;
    }

    /// <summary>
    /// Decodes a string.
    /// </summary>
    /// <param name="data">Data to be decoded to a string.</param>
    /// <param name="offset">The offset in <paramref name="data"/> where to start decoding.</param>
    /// <param name="text">The decoded string.</param>
    public static bool TryDecodeString(byte[] data, ref int offset, out string text)
    {
        if (offset + sizeof(int) <= data.Length)
        {
            int DecodedStringLength = BitConverter.ToInt32(data, offset);

            if (DecodedStringLength >= sizeof(int) && offset + DecodedStringLength <= data.Length)
            {
                byte[] DecodedString = new byte[DecodedStringLength - sizeof(int)];
                Array.Copy(data, offset + sizeof(int), DecodedString, 0, DecodedString.Length);

                text = Encoding.UTF8.GetString(DecodedString);
                offset += DecodedStringLength;
                return true;
            }
        }

        text = null!;
        return false;
    }
}