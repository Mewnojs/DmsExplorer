using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsUtils;

public class DmsReader
{
    public const string MAGIC = "PortalSequenceData";
    const int MAGIC_LENGTH = 18;

    public byte[] DmsDataFromStream(Stream stream) 
    {
        byte[] buffer = new byte[MAGIC_LENGTH + 4];
        stream.Read(buffer, 0, MAGIC_LENGTH + 4);
        if (Encoding.ASCII.GetString(buffer, 0, MAGIC_LENGTH) != MAGIC)
        {
            throw new Exception();
        }
        int decompressed_length = (int)BitConverter.ToUInt32(buffer, MAGIC_LENGTH);
        using (var zstream = new ZLibStream(stream, CompressionMode.Decompress))
        {
            byte[] b = new byte[decompressed_length];

            int offset = 0;
            for (; ; )
            {
                int readbytes = zstream.Read(b, offset, decompressed_length - offset);
                if (readbytes == 0)
                    break;
                offset += readbytes;
            }
            return b;
        }
    }

    public DmsCompositeNode ReadDmsData(byte[] data) 
    {
        //DmsNodeType nodeType = 0 | DmsNodeType.L_Root;
        return new DmsCompositeNode(0, -1, null, new MemoryStream(data), data.Length);
    }
    public DmsReader() 
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}

