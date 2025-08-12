using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsUtils;

public static class DmsNodeFactory
{
    public static DmsNode Produce(Stream stream, int layer, DmsNode? parent)
    {
        ushort type = GetNodeMetadata(stream);
        switch (ToNodeType(type, layer, parent))
        {
            case DmsNodeType.Root:
            case DmsNodeType.CurrentVars:
            case DmsNodeType.MidiOutCfg:
            case DmsNodeType.Track:
            case DmsNodeType.KeyPalette:
            case DmsNodeType.PortCfg:
            case DmsNodeType.PortCfg_A:
            case DmsNodeType.PortCfg_B:
            case DmsNodeType.PortCfg_C:
            case DmsNodeType.PortCfg_D:
            case DmsNodeType.PortCfg_E:
            case DmsNodeType.PortCfg_F:
            case DmsNodeType.PortCfg_G:
            case DmsNodeType.PortCfg_H:
            case DmsNodeType.PortCfg_I:
            case DmsNodeType.PortCfg_J:
            case DmsNodeType.PortCfg_K:
            case DmsNodeType.PortCfg_L:
            case DmsNodeType.PortCfg_M:
            case DmsNodeType.PortCfg_N:
            case DmsNodeType.PortCfg_O:
            case DmsNodeType.PortCfg_P:
            case DmsNodeType.Track_OnionskinData:
            case DmsNodeType.NoteEvent:
            case DmsNodeType.ProgramChangeEvent:
            case DmsNodeType.ControlEvent:
            case DmsNodeType.CustomSysExEvent:
            case DmsNodeType.CommentEvent:
            case DmsNodeType.FormulaEvent:
            case DmsNodeType.TempoEvent:
            case DmsNodeType.EndOfTrackEvent:
            case DmsNodeType.LyricsEvent:
            case DmsNodeType.CuePointEvent:
            case DmsNodeType.MeasureLinkEvent:
            case DmsNodeType.TimeSigEvent:
            case DmsNodeType.KeySigEvent:
            case DmsNodeType.MarkerEvent:
            case DmsNodeType.ScaleEvent:
            case DmsNodeType.ChordEvent:
                return new DmsCompositeNode(type, layer, parent, stream, GetNodeDataLength(stream));
            case DmsNodeType.SongName:
            case DmsNodeType.SongCopyright:
            case DmsNodeType.SongComment:
            case DmsNodeType.Track_Name:
            case DmsNodeType.Track_DrumSetName:
            case DmsNodeType.Comment_Text:
            case DmsNodeType.Formula_VarName:
            case DmsNodeType.Formula_Expression:
            case DmsNodeType.CustomSysEx_Data:
            case DmsNodeType.Lyrics_Lyrics:
            case DmsNodeType.CuePoint_Value:
            case DmsNodeType.Marker_Name:
                return new DmsAnsiStringNode(type, layer, parent, GetNodeData(stream));
            case DmsNodeType.SongPPQN:
            case DmsNodeType.PianoRollSelNoteToolIndex:
            case DmsNodeType.MasterSelNoteToolIndex:
            case DmsNodeType.WorkingTimeSec:
            case DmsNodeType.Track_Port:
            case DmsNodeType.Track_Channel:
            //case DmsNodeType.Track_IsMuted:
            case DmsNodeType.Track_IsDrum:
            case DmsNodeType.Track_SelectedVelocity:
            case DmsNodeType.Track_SelectedGate:
            case DmsNodeType.Track_TickComp:
            case DmsNodeType.Track_GateCompPercent:
            case DmsNodeType.Track_KeyComp:
            case DmsNodeType.Track_OnionskinColorIndex:
            case DmsNodeType.Track_TickCompFromMea:
            case DmsNodeType.Track_NoteRange_L:
            case DmsNodeType.Track_NoteRange_H:
            case DmsNodeType.AbsTickPos:
            case DmsNodeType.Note_KeyNumber:
            case DmsNodeType.Note_Velocity:
            case DmsNodeType.Note_Gate:
            case DmsNodeType.Control_Type:
            case DmsNodeType.MeasureLink_Measure:
            case DmsNodeType.MeasureLink_KeyComp:
            case DmsNodeType.KeySig_Index:
            case DmsNodeType.TimeSig_Numerator:
            case DmsNodeType.TimeSig_Denominator:
                return new DmsIntegerNode(type, layer, parent, GetNodeData(stream));
            case DmsNodeType.Control_Gate:
            case DmsNodeType.Control_Value:
            case DmsNodeType.Tempo_Value:
                return new DmsFloatNode(type, layer, parent, GetNodeData(stream));
            default:
                return new DmsDataNode(type, layer, parent, GetNodeData(stream));
        }
    }

    public static ushort GetNodeMetadata(Stream stream)
    {
        byte[] buffer = new byte[DmsNode.TYPEID_SIZE];
        if (stream.Read(buffer, 0, DmsNode.TYPEID_SIZE) < DmsNode.TYPEID_SIZE)
        {
            throw new EndOfStreamException();
        }
        return BitConverter.ToUInt16(buffer, 0);
    }

    public static int GetNodeDataLength(Stream stream)
    {
        byte[] buffer = new byte[DmsNode.DATALENGTH_SIZE];
        if (stream.Read(buffer, 0, DmsNode.DATALENGTH_SIZE) < DmsNode.DATALENGTH_SIZE)
        {
            throw new EndOfStreamException();
        }
        int theDataLength = (int)BitConverter.ToUInt32(buffer, 0);
        return theDataLength;
    }

    public static byte[] GetNodeData(Stream stream)
    {
        return GetNodeData(stream, GetNodeDataLength(stream));
    }

    public static byte[] GetNodeData(Stream stream, int length)
    {
        var aData = new byte[length];
        if (stream.Read(aData, 0, length) < length)
        {
            throw new EndOfStreamException();
        }
        return aData;
    }

    public static DmsNodeType ToNodeType(ushort type, int layer, DmsNode? parent)
    {
        //if (type == 0)
        //    throw new Exception();
        if (parent is null)
        {
            return (DmsNodeType)type;
        }
        DmsNodeType result = (DmsNodeType)(type | (ulong)parent.NodeType << 16);
        return (((ulong)result & 0x00000000FFFF0000) >= (2000 << 16) && ((ulong)result & 0xFFFFFFFF0000FFFF) == (ulong)DmsNodeType.AbsTickPos) ? DmsNodeType.AbsTickPos : result;
    }
}
