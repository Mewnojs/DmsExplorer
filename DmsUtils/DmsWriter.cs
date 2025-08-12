using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DmsUtils;

public class DmsWriter
{
    public void WriteWhole(Stream stream, DmsCompositeNode root) 
    {
        foreach (DmsNode node in root.Children) 
        {
            stream.Write(BitConverter.GetBytes(node.TypeId));
            stream.Write(BitConverter.GetBytes(node.Length));
            if (node is DmsCompositeNode newroot)
            {
                WriteWhole(stream, newroot);
            }
            else 
            {
                stream.Write(node.RawData);
            }
        }
    }

    public void DataToFile(byte[] data, Stream filestream) 
    {
        filestream.Write(Encoding.ASCII.GetBytes(DmsReader.MAGIC));
        filestream.Write(BitConverter.GetBytes(data.Length));
        var zstream = new ZLibStream(filestream, CompressionLevel.SmallestSize);
        zstream.Write(data);
        zstream.Flush();
        zstream.Close();
    }


    public DmsWriter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
