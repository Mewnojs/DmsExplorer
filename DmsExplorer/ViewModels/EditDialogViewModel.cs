using DialogHostAvalonia;
using DmsUtils;
using static DmsUtils.DmsNodeStringEditExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsExplorer.ViewModels;

public class EditDialogViewModel
{
    public DmsNode Item { get; set; }
    public string EditTitle => $"Editing \"{Item?.ContentType}\" type: " + Hint;
    public string Hint { get; set; } = string.Empty ;
    public bool AcceptsReturn => true;
    public bool AcceptsTab => true;
    public string Text {  get; set; }
    public string Digits { get; set; } = String.Empty;

    public bool ShowDigits { get; set; } = true;

    public DialogSession? Session { get; set; }

    public Action? SetDirty { get; set; }

    public bool IsOpen {  get=> true; set { if (value == false) { Session?.Close(false); } } }

    public void EditCancel() 
    {
        IsOpen = false;
    }

    public void EditSave() 
    {
        if (Item.SaveStringEdit(Text, Digits))
        {
            //Item.OnPropertyChanged();
            SetDirty!();
            IsOpen = false;
        }
        else 
        {
            Hint = "\nSaving failed, The content is invalid!";
        }
        
    }

    private EditDialogViewModel() { Item = new DmsDataNode(); Text = string.Empty;}

    public EditDialogViewModel(DmsNode item) 
    {
        Item = item; 
        Text = item.ShowContent(); 
        IsOpen = true;
        if (item is DmsCompositeNode)
        {
            IsOpen = false;
            return;
        }
        
        if (item.GetType() == typeof(DmsDataNode) || item.GetType() == typeof(DmsFloatNode) || item.GetType() == typeof(DmsAnsiStringNode)) 
        {
            ShowDigits = false;
        }
        else
        {
            Digits = (item as DmsDataNode)!.RawData.Length.ToString();
        }
    }
}
