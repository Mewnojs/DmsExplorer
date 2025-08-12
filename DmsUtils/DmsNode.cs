
using DynamicData.Binding;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DmsUtils;

public abstract class DmsNode : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler? handler = PropertyChanged;
        if (handler is not null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    const int INVALID_TYPE = 0xFFFF;
    public const int TYPEID_SIZE = 2;
    public const int DATALENGTH_SIZE = 4;
    protected static readonly ObservableCollectionExtended<DmsNode> EMPTY_COLLECTION = new ();
    protected static readonly byte[] EMPTY_DATA = new byte[0];

    public DmsNode? Parent { get; set; }
    public int RelativeIndex { get => 0; set { } }

    public ushort TypeId { get; set; }
    public int Layer { get => 0; set { } }

    public virtual DmsNodeType NodeType { get { return DmsNodeFactory.ToNodeType(TypeId, Layer, Parent); } }
    public virtual bool HasData() => false;
    public virtual byte[] RawData { get { return EMPTY_DATA; } set { throw new NotImplementedException(); } }
    public virtual int Length { get { return 0; } set { throw new NotImplementedException(); } }
    public virtual string ContentType { get => "none"; }
    public virtual object ContentRaw => throw new NotImplementedException();
    public virtual string ShowContent() 
    {
        return String.Empty;
    }
 

    public virtual bool IsComposite() => false;
    public virtual ObservableCollectionExtended<DmsNode> Children { get { return EMPTY_COLLECTION; } set { throw new NotImplementedException(); } }

    //public virtual DmsNode GetParent() => mParent;

    public DmsNode() 
    {
        TypeId = INVALID_TYPE;
        Layer = 0;
    }
    public DmsNode(ushort type, int layer, DmsNode? parent)
    {
        TypeId = type;
        Layer = layer;
        Parent = parent;
    }
    /*public DmsNode(byte[] bytes)
    {
        //mTypeId = BitConverter.ToUInt16(bytes, 0);
        //int aDataLength = (int)BitConverter.ToUInt32(bytes, 2);
        //mData = new byte[aDataLength];
        //Buffer.BlockCopy(bytes, 6, mData, 0, (int)aDataLength);
    }*/
}

public class DmsDataNode : DmsNode 
{
    protected byte[] _rawData = EMPTY_DATA;
    public override byte[] RawData { get => _rawData; set { _rawData = value; OnPropertyChanged(nameof(RawData)); } }
    public override bool HasData() => true;
    public override string ContentType => "binary";
    public override string ShowContent()
    {
        return BitConverter.ToString(RawData).Replace("-", " ");
    }
    public override object ContentRaw => Encoding.GetEncoding("ISO-8859-1").GetString(RawData);
    public override int Length { get => RawData.Length; set { if (value > RawData.Length) { byte[] raw = new byte[value];  RawData.CopyTo(raw, 0); RawData = raw; } else if (value == RawData.Length) {} else { throw new NotSupportedException("Do not support decreasing data length to prevent unintentional data loss."); } } }

    public DmsDataNode() : base() { RawData = new byte[0]; }
    public DmsDataNode(ushort type, int layer, DmsNode? parent, byte[] data) : base(type, layer, parent)
    {
        RawData = data;
    }
    public DmsDataNode(ushort type, int layer, DmsNode? parent, Stream stream) : base(type, layer, parent)
    {
        RawData = DmsNodeFactory.GetNodeData(stream);
    }
}


public class DmsCompositeNode : DmsNode
{
    public override ObservableCollectionExtended<DmsNode> Children { get; set; }
    public override bool IsComposite() => true;
    public override string ContentType => "";
    public override string ShowContent()
    {
        return $"{Children.Count} members";
    }
    public override object ContentRaw
    {
        get 
        {
            var d = new List<Dictionary<ushort, object>>();
            foreach (DmsNode node in Children) 
            {
                var temp = new Dictionary<ushort, object>();
                temp[node.TypeId] = node.ContentRaw;
                d.Add(temp);
            }
            return d;
        }
    }
    public override int Length { get => (from child in Children select child.Length + TYPEID_SIZE + DATALENGTH_SIZE).Sum(); set => base.Length = value; }

    public DmsCompositeNode() : base() { Children = new(); }
    public DmsCompositeNode(ushort type, int layer, DmsNode? parent, in ObservableCollection<DmsNode> children) : base(type, layer, parent)
    {
        Children = new ObservableCollectionExtended<DmsNode>(children);
    }

    public DmsCompositeNode(DmsDataNode node) : this(node.TypeId, node.Layer, node.Parent, node.RawData) {}

    public DmsCompositeNode(ushort type, int layer, DmsNode? parent, Stream stream, int length) : base(type, layer, parent)
    {
        Children = new ObservableCollectionExtended<DmsNode>();
        if (length == 0)
            return;

        long streamPos = stream.Position;
        
        int index = 0;
        for (; ; )
        {
            var child = DmsNodeFactory.Produce(stream, Layer + 1, this);
            child.RelativeIndex = index;
            Children.Add(child);
            index++;
            if (stream.Position == streamPos + length) { break; }
            else if (stream.Position > streamPos + length) { throw new Exception("The file may be corrupted."); }
        }
    }

    public DmsCompositeNode(ushort type, int layer, DmsNode? parent, byte[] data) : this(type, layer, parent, new MemoryStream(data), data.Length) { }
}

public class DmsAnsiStringNode : DmsDataNode
{
    public string StringData { get { return Encoding.GetEncoding("GB18030").GetString(RawData); } set { RawData = Encoding.GetEncoding("GB18030").GetBytes(value); } }
    public override string ContentType => "string";
    public override string ShowContent()
    {
        return StringData;
    }

    public DmsAnsiStringNode() : base() { }
    public DmsAnsiStringNode(ushort type, int layer, DmsNode? parent, byte[] data) : base(type, layer, parent, data) { }
    public DmsAnsiStringNode(ushort type, int layer, DmsNode? parent, Stream stream) : base(type, layer, parent, stream) { }
}

public class DmsIntegerNode : DmsDataNode 
{
    public virtual BigInteger IntegerData { get { return new BigInteger(RawData, true, false); } set { RawData = value.ToByteArray(true, false); } }
    public override string ContentType => "integer";
    public override string ShowContent()
    {
        return IntegerData.ToString();
    }

    public DmsIntegerNode() : base() { }
    public DmsIntegerNode(ushort type, int layer, DmsNode? parent, byte[] data) : base(type, layer, parent, data) { }
    public DmsIntegerNode(ushort type, int layer, DmsNode? parent, Stream stream) : base(type, layer, parent, stream) { }
}

/*public class DmsIntegerVirtualNode : DmsIntegerNode 
{
    public override BigInteger IntegerData { get { return new BigInteger(RawData, true, false); } set {; } }
}*/

public class DmsFloatNode : DmsDataNode
{
    public double NumberData { get { if (!mIsDouble) { return BitConverter.ToSingle(RawData.AsSpan(6..)); } else { return 0.0; } } set { if (!mIsDouble) { RawData = RawData[..6].Concat(BitConverter.GetBytes((float)value)).ToArray(); } else { RawData = RawData[..6].Concat(BitConverter.GetBytes((double)value)).ToArray(); } } }
    public override byte[] RawData { get => base.RawData; set { base.RawData = value; mIsDouble = ParseRawData(); } }
    private bool mIsDouble = true;
    public override string ContentType => mIsDouble ? "double" : "float";
    public override string ShowContent()
    {
        return NumberData.ToString();
    }

    private bool ParseRawData() 
    {
        var b = RawData;
        if (BitConverter.ToUInt16(b, 0) == 0)
        {
            if (b.Length == TYPEID_SIZE + DATALENGTH_SIZE + 4 && BitConverter.ToUInt32(b, TYPEID_SIZE) == 4)
            {
                return false;
            } 
            else if (b.Length == TYPEID_SIZE + DATALENGTH_SIZE + 8 && BitConverter.ToUInt32(b, TYPEID_SIZE) == 8)
            {
                return true;
            }
        }
        throw new Exception("Unsupported float type.");
    }

    public DmsFloatNode(bool isDouble = true) : base() { mIsDouble = isDouble; }
    public DmsFloatNode(ushort type, int layer, DmsNode? parent, byte[] data) : base(type, layer, parent, data) {}
    public DmsFloatNode(ushort type, int layer, DmsNode? parent, Stream stream) : base(type, layer, parent, stream) {}
}



/*public interface IDmsNodeParental 
{
    public override ObservableCollection<DmsNode> Children { get; set; }
}*/