using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsUtils;

public enum DmsNodeType : ulong
{
    Root                = 0x0000,

    SongName            = 1000,
    SongCopyright       = 1001,
    SongPPQN            = 1002,

    CurrentVars         = 1006,         
    MidiOutCfg          = 1008,
    WorkingTimeSec      = 1013,
    Track               = 1003,
    KeyPalette          = 1017,
    PortCfg             = 1018,
    SongComment         = 1019,
    PianoRollSelNoteToolIndex  = 1020,
    MasterSelNoteToolIndex     = 1023,

    PortCfg_A           = 1000 | PortCfg << 16,
    PortCfg_B           = 1001 | PortCfg << 16,
    PortCfg_C           = 1002 | PortCfg << 16,
    PortCfg_D           = 1003 | PortCfg << 16,
    PortCfg_E           = 1004 | PortCfg << 16,
    PortCfg_F           = 1005 | PortCfg << 16,
    PortCfg_G           = 1006 | PortCfg << 16,
    PortCfg_H           = 1007 | PortCfg << 16,
    PortCfg_I           = 1008 | PortCfg << 16,
    PortCfg_J           = 1009 | PortCfg << 16,
    PortCfg_K           = 1010 | PortCfg << 16,
    PortCfg_L           = 1011 | PortCfg << 16,
    PortCfg_M           = 1012 | PortCfg << 16,
    PortCfg_N           = 1013 | PortCfg << 16,
    PortCfg_O           = 1014 | PortCfg << 16,
    PortCfg_P           = 1015 | PortCfg << 16,

    Track_Port              = 1000 | Track << 16,
    Track_Channel           = 1001 | Track << 16,
    Track_Name              = 1002 | Track << 16,
    //Track_IsMuted           = 1003 | Track << 16,
    Track_IsDrum            = 1004 | Track << 16,
    Track_SelectedVelocity  = 1006 | Track << 16,
    Track_SelectedGate      = 1007 | Track << 16,
    Track_DrumSetName       = 1009 | Track << 16,
    Track_OnionskinData     = 1010 | Track << 16,
    Track_TickComp          = 1012 | Track << 16,
    Track_TickCompFromMea   = 1019 | Track << 16,
    Track_GateCompPercent   = 1016 | Track << 16,
    Track_KeyComp           = 1017 | Track << 16,
    Track_OnionskinColorIndex   = 1018 | Track << 16,
    Track_NoteRange_L       = 1021 | Track << 16,
    Track_NoteRange_H       = 1022 | Track << 16,

    NoteEvent           = 2001 | Track << 16,
    ProgramChangeEvent  = 2002 | Track << 16,
    ControlEvent        = 2003 | Track << 16,
    CustomSysExEvent    = 2004 | Track << 16,
    CommentEvent        = 2005 | Track << 16,
    FormulaEvent        = 2007 | Track << 16,
    TempoEvent          = 2008 | Track << 16,
    EndOfTrackEvent     = 2009 | Track << 16,
    LyricsEvent         = 2011 | Track << 16,
    CuePointEvent       = 2012 | Track << 16,
    MeasureLinkEvent    = 2014 | Track << 16,
    TimeSigEvent        = 2015 | Track << 16,
    KeySigEvent         = 2016 | Track << 16,
    MarkerEvent         = 2017 | Track << 16,
    ScaleEvent          = 2018 | Track << 16,
    ChordEvent          = 2019 | Track << 16,

    AbsTickPos          = 1001 | Track << 32,   // Special.

    Note_KeyNumber      = 2001 | NoteEvent << 16,
    Note_Velocity       = 2002 | NoteEvent << 16,
    Note_Gate           = 2003 | NoteEvent << 16,

    Control_Type        = 2001 | ControlEvent << 16,
    Control_Gate        = 2002 | ControlEvent << 16,
    Control_Value       = 2003 | ControlEvent << 16,

    Comment_Text        = 2001 | CommentEvent << 16,

    Formula_VarName     = 2001 | FormulaEvent << 16,
    Formula_Expression  = 2002 | FormulaEvent << 16,
    
    Tempo_Value         = 2001 | TempoEvent << 16,

    CustomSysEx_Data    = 2001 | CustomSysExEvent << 16,

    Lyrics_Lyrics       = 2001 | LyricsEvent << 16,

    CuePoint_Value      = 2001 | CuePointEvent << 16,
    
    MeasureLink_Measure = 2001 | MeasureLinkEvent << 16,
    MeasureLink_KeyComp = 2002 | MeasureLinkEvent << 16,

    KeySig_Index        = 2001 | KeySigEvent << 16,

    TimeSig_Numerator   = 2001 | TimeSigEvent << 16,
    TimeSig_Denominator = 2002 | TimeSigEvent << 16,

    Marker_Name         = 2001 | MarkerEvent << 16,

}


/*public enum DmsNodeContentType 
{
    None, 
    Binary,
    Nodes,
    AnsiString,
    Number,
}*/