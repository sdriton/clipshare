using System.Text;

namespace ClipShare;

public static class Protocol
{
    public const byte STX = 0x02;
    public const byte ETX = 0x03;
    public const byte US = 0x1F;
    public const byte RS = 0x1E;

    public static byte[] BuildTextFrame(string text)
    {
        var payload = Encoding.UTF8.GetBytes(text);
        var length = BitConverter.GetBytes((uint)payload.Length);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(length);

        var frame = new List<byte>();
        frame.Add(STX);
        frame.AddRange(Encoding.ASCII.GetBytes("CLIP"));
        frame.Add(US);
        frame.AddRange(Encoding.ASCII.GetBytes("TEXT"));
        frame.Add(RS);
        frame.AddRange(length);
        frame.AddRange(payload);
        frame.Add(ETX);

        return frame.ToArray();
    }
}

public class FrameParser
{
    private readonly List<byte> _buffer = [];

    public void Feed(byte[] chunk)
    {
        _buffer.AddRange(chunk);
    }

    public bool TryGetNextText(out string text)
    {
        text = string.Empty;

        // Find STX
        int stxIndex = _buffer.IndexOf(Protocol.STX);
        if (stxIndex == -1)
        {
            _buffer.Clear();
            return false;
        }

        if (stxIndex > 0)
        {
            _buffer.RemoveRange(0, stxIndex);
        }

        // Minimum frame length: STX(1) + CLIP(4) + US(1) + TEXT(4) + RS(1) + length(4) + ETX(1) = 16
        if (_buffer.Count < 16)
            return false;

        int idx = 1;

        // Check CLIP
        if (!Matches(_buffer, idx, "CLIP"))
        {
            _buffer.RemoveAt(0);
            return false;
        }
        idx += 4;

        // Check US
        if (_buffer[idx] != Protocol.US)
        {
            _buffer.RemoveRange(0, idx + 1);
            return false;
        }
        idx++;

        // Check TEXT
        if (!Matches(_buffer, idx, "TEXT"))
        {
            _buffer.RemoveRange(0, idx + 4);
            return false;
        }
        idx += 4;

        // Check RS
        if (_buffer[idx] != Protocol.RS)
        {
            _buffer.RemoveRange(0, idx + 1);
            return false;
        }
        idx++;

        // Read length
        if (idx + 4 > _buffer.Count)
            return false;

        var lengthBytes = _buffer.GetRange(idx, 4).ToArray();
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(lengthBytes);
        uint payloadLength = BitConverter.ToUInt32(lengthBytes, 0);
        idx += 4;

        int payloadStart = idx;
        int payloadEnd = payloadStart + (int)payloadLength;

        // Check if we have the full frame
        if (payloadEnd >= _buffer.Count)
            return false;

        // Check ETX
        if (_buffer[payloadEnd] != Protocol.ETX)
        {
            _buffer.RemoveAt(0);
            return false;
        }

        // Extract payload
        var payload = _buffer.GetRange(payloadStart, (int)payloadLength).ToArray();
        text = Encoding.UTF8.GetString(payload);

        // Remove the processed frame
        _buffer.RemoveRange(0, payloadEnd + 1);

        return true;
    }

    private static bool Matches(List<byte> buffer, int offset, string tag)
    {
        var tagBytes = Encoding.ASCII.GetBytes(tag);
        if (offset + tagBytes.Length > buffer.Count)
            return false;

        for (int i = 0; i < tagBytes.Length; i++)
        {
            if (buffer[offset + i] != tagBytes[i])
                return false;
        }

        return true;
    }
}
