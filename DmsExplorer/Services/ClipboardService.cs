using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsExplorer.Services;

internal class ClipboardService : IClipBoardService
{
    private readonly TopLevel _target;

    public ClipboardService(TopLevel target)
    {
        _target = target;
    }

    public async Task SetText(string? text) 
    {
        await _target.Clipboard!.SetTextAsync(text);
    }

    public async Task ClearText()
    {
        await _target.Clipboard!.ClearAsync();
    }

    public async Task<string?> GetText()
    {
        return await _target.Clipboard!.GetTextAsync();
    }
}
