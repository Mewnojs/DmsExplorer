using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmsExplorer.Services;

public class FilesService : IFilesService
{
    private readonly TopLevel _target;

    public FilesService(TopLevel target)
    {
        _target = target;
    }

    public async Task<IStorageFile?> OpenFileAsync(FilePickerOpenOptions options)
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(options);

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync(FilePickerSaveOptions options)
    {
        return await _target.StorageProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFolder?> GetFolderFromUriAsync(Uri path)
    {
        var folder = await _target.StorageProvider.TryGetFolderFromPathAsync(path);

        return folder;
    }
}
