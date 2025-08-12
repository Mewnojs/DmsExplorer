using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsExplorer.Services;

public interface IClipBoardService
{
    public Task SetText(string? text);
    public Task ClearText();
    public Task<string?> GetText();

}
