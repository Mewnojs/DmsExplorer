using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsExplorer.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync(FilePickerOpenOptions options);
    public Task<IStorageFile?> SaveFileAsync(FilePickerSaveOptions options);

    public Task<IStorageFolder?> GetFolderFromUriAsync(Uri path);
}
