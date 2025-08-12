using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using DmsUtils;
using System.IO;
using System;
using Avalonia.Dialogs;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;
using Avalonia.Interactivity;
using DmsExplorer.Services;
using Avalonia;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Linq;
using Avalonia.Input;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Converters;
using DynamicData;
using DmsExplorer.Views;
using DialogHostAvalonia;
using System.Diagnostics;
using DynamicData.Binding;

namespace DmsExplorer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private DmsCompositeNode? _root;

    public async void OpenFileCommand()
    {
        var filesService = (IFilesService?)App.Current?.Services?.GetService(typeof(IFilesService));
        if (filesService is null) throw new NullReferenceException("Missing File Service instance.");
        var file = await filesService.OpenFileAsync(new FilePickerOpenOptions
        {
            Title = "Choose a DMS file to open...",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType("Domino Sequence File") { Patterns = new[] { "*.dms", "*.dms256" } } },
        });
        if (file is null) return;
        FileName = file.Name;
        string path = file.Path.GetLeftPart(UriPartial.Path);
        FileFolder = await filesService.GetFolderFromUriAsync(new Uri(path.Substring(0, path.LastIndexOf('/'))));
        byte[] data;
        var dmsReader = new DmsReader();
        using (Stream fs = await file.OpenReadAsync())
        {
            data = dmsReader.DmsDataFromStream(fs);//new DmsCompositeNode(0, -1, fs, (int)fs.Length);
        }
        _root = dmsReader.ReadDmsData(data);
        _nodes = _root.Children;
        Source.Items = _nodes;
    }

    public async void SaveAsFileCommand()
    {
        if (_root is null) return;
        var filesService = (IFilesService?)App.Current?.Services?.GetService(typeof(IFilesService));
        if (filesService is null) throw new NullReferenceException("Missing File Service instance.");
        var file = await filesService.SaveFileAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = _fileName,
            SuggestedStartLocation = FileFolder,
            //DefaultExtension = Path.GetExtension(_fileName),
            Title = "Choose a name to save...",
            FileTypeChoices = new List<FilePickerFileType> {
                new FilePickerFileType("Domino Sequence File") { Patterns = new[] { "*.dms" } },
                new FilePickerFileType("Domino Sequence File (256 keys)") { Patterns = new[] { "*.dms256" } }
            },
        });
        if (file is null) return;

        var writer = new DmsWriter();
        var buffer = new byte[_root.Length];
        using (var ms = new MemoryStream(buffer))
        {
            using (Stream fs = await file.OpenWriteAsync()) {
                writer.WriteWhole(ms, _root);
                writer.DataToFile(buffer, fs);
            }
        }
        FileName = file.Name;

    }

    public void CopySelectedCommand()
    {
        var clipboardService = (IClipBoardService?)App.Current?.Services?.GetService(typeof(IClipBoardService));
        if (clipboardService is null) throw new NullReferenceException("Missing Clipboard Service instance.");
        string content = Source.RowSelection!.SelectedItem!.ShowContent();
        clipboardService.ClearText();
        clipboardService.SetText(content);
    }

    public void CopySelectedRawCommand()
    {
        var clipboardService = (IClipBoardService?)App.Current?.Services?.GetService(typeof(IClipBoardService));
        if (clipboardService is null) throw new NullReferenceException("Missing Clipboard Service instance.");
        var item = Source.RowSelection!.SelectedItem!;
        var d = new Dictionary<ushort, object>();
        d[item.TypeId] = item.ContentRaw;
        string content = JsonSerializer.Serialize(d, new JsonSerializerOptions()
        {
            TypeInfoResolver = JsonSourceGenerationContext.Default
        }
        );
        clipboardService.ClearText();
        clipboardService.SetText(content);
    }

    public bool CanEditCommand { get => Source.RowSelection!.SelectedItem is DmsDataNode; } // TODO: needs update on selection change!

    private readonly EditDialog editDialog = new EditDialog();

    public async void EditCommand()
    {
        var item = Source.RowSelection!.SelectedItem!;
        IndexPath path = Source.RowSelection!.SelectedIndex;
        var layout = new DockPanel() { };
        if (item is null) return;

        editDialog.DataContext = new EditDialogViewModel(item);
        await DialogHostAvalonia.DialogHost.Show(editDialog, delegate (object sender, DialogOpenedEventArgs args)
        {
            (editDialog.DataContext as EditDialogViewModel)!.Session = args.Session;
            (editDialog.DataContext as EditDialogViewModel)!.SetDirty = SetDirty;
        });
    }

    private string? _fileName = null;

    private bool _fileDirty;

    private void SetDirty() { FileDirty = true; }

    public bool FileDirty { get => _fileDirty; private set { this.RaiseAndSetIfChanged(ref _fileDirty, value); } }

    public string? FileName { get => _fileName; set { FileDirty = false; this.RaiseAndSetIfChanged(ref _fileName, value); this.RaisePropertyChanged(nameof(FileNameDisplay)); } }

    public string FileNameDisplay { get => FileName ?? "No files loaded."; }

    public IStorageFolder? FileFolder { get; set; }

    private ObservableCollectionExtended<DmsNode> _nodes = new()
    {
        /*new DmsNode
        {
            NodeType = DmsNodeTypes.Layer1;
        }*/
    };

    private IndexPath? willExpandRow;

    public void TreeDoubleTapExpand(object[] a) 
    {
        object? sender = a[0];  TappedEventArgs args = (TappedEventArgs)a[1];
        
        IndexPath selected = Source.RowSelection!.SelectedIndex;
        TreeDataGrid s = (TreeDataGrid)sender;
        Point point = args.GetPosition(s);
        var width = point.X - s.Source!.Columns[0].ActualWidth;
        if (width < 0) return;
        int indent = (int)(width / 20) + 1;
        if (selected.Count > indent) 
        {
            selected = new IndexPath(selected[..indent]);
        }

        willExpandRow = null;
        //IndentConverter
        Source.Expand(selected);
        if (willExpandRow == selected)
        {
            return;
        }
        Source.Collapse(selected);
    }

    public static void PointerPressedHandler(object sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;
        var msg = $"Pointer press at {x}, {y} relative to sender.";
        if (point.Properties.IsLeftButtonPressed)
        {
            msg += " Left button pressed.";
        }
        if (point.Properties.IsRightButtonPressed)
        {
            msg += " Right button pressed.";
        }
        //results.Text = msg;
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public object Blah { get { return TreeDataGrid.DoubleTappedEvent; } }

    private void TreeOnRowExpanding(object? sender, RowEventArgs<HierarchicalRow<DmsNode>> args) 
    {
        if (!args.Row.IsExpanded)
            willExpandRow = args.Row.ModelIndexPath;
    }

    public MainViewModel()
    {
        /*
        for (var i = 0; i < 1000000; i++) 
        {
            _nodes.Add(new DmsDataNode() { });
        }*/
        //Test();
        Source = new HierarchicalTreeDataGridSource<DmsNode>(_nodes)
        {
            Columns =
            {
                new TextColumn<DmsNode, string>("Index", x => (((object)x.RelativeIndex).ToString())),
                new HierarchicalExpanderColumn<DmsNode>(new TextColumn<DmsNode, string>("Node Type (Type ID)", x => (Enum.IsDefined(x.NodeType) ? ((object)x.NodeType).ToString()!.Split("_", 2, StringSplitOptions.None).Last() : "") + "(" + ((object)((int)x.NodeType & 0xFFFF)).ToString() + ")" , null, new TextColumnOptions<DmsNode>{CanUserSortColumn = false,}),
                    x => x.Children),
                new TextColumn<DmsNode, string>("Content", x => x.ShowContent(), GridLength.Star, new TextColumnOptions<DmsNode>(){
                    TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
                    TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
                    CanUserSortColumn = false,
                } ),
                new TextColumn<DmsNode, string>("Content Type", x => x.ContentType, null, new TextColumnOptions<DmsNode>{CanUserSortColumn = false,}),
                new TextColumn<DmsNode, string>("Length", x => x is DmsCompositeNode ? "" : "" + x.Length, null, new TextColumnOptions<DmsNode>{CanUserSortColumn = false,}),
                //new TextColumn<DmsNode, int>("Age", x => x.Age),
            },
        };
        Source.RowSelection!.SingleSelect = true;
        //Source.RowSelection!.
        Source.RowExpanding += TreeOnRowExpanding;
    }

    public HierarchicalTreeDataGridSource<DmsNode> Source { get; set; }

    public static string VersionInfo => "v0.1.0 by MilkyCanoe";
}

