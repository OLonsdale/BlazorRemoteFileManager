namespace RemoteFileManager.Web;

public class FileSystemService
{
    public record FileOperationResult(bool Success, string? Error = null);

    public record ClipboardItem(string Path);

    private List<ClipboardItem> _clipboard = [];
    private bool _isCut;

    public IReadOnlyList<ClipboardItem> Clipboard => _clipboard;

    public bool ClipboardIsCut => _isCut;

    public List<FileSystemInfo> GetDirectoryContents(string path, bool showHidden)
    {
        var dir = new DirectoryInfo(path);

        return
        [
            ..dir.EnumerateFileSystemInfos()
                .Where(f => showHidden ||
                            (!f.Attributes.HasFlag(FileAttributes.Hidden) &&
                             !f.Attributes.HasFlag(FileAttributes.System)))
                .OrderByDescending(x => x is DirectoryInfo)
                .ThenBy(x => x.Name)
        ];
    }

    public HashSet<string> GetNames(string path)
    {
        return new DirectoryInfo(path)
            .EnumerateFileSystemInfos()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public FileOperationResult CreateFolder(string parentPath, string name)
    {
        try
        {
            var target = Path.Combine(parentPath, name);

            if (Directory.Exists(target) || File.Exists(target))
                return new(false, "A file or folder with that name already exists.");

            Directory.CreateDirectory(target);

            return new(true);
        }
        catch (Exception ex)
        {
            return new(false, ex.Message);
        }
    }

    public FileOperationResult Rename(string path, string newName)
    {
        try
        {
            var parent = Directory.GetParent(path)?.FullName;

            if (parent == null)
                return new(false, "Cannot determine parent directory.");

            var target = Path.Combine(parent, newName);

            if (Directory.Exists(target) || File.Exists(target))
                return new(false, "Target already exists.");

            if (Directory.Exists(path))
                Directory.Move(path, target);
            else if (File.Exists(path))
                File.Move(path, target);
            else
                return new(false, "Source does not exist.");

            return new(true);
        }
        catch (Exception ex)
        {
            return new(false, ex.Message);
        }
    }

    public List<FileOperationResult> Delete(IEnumerable<string> paths)
    {
        var results = new List<FileOperationResult>();

        foreach (var path in paths)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
                else
                    results.Add(new(false, $"{Path.GetFileName(path)} no longer exists"));

                results.Add(new(true));
            }
            catch (Exception ex)
            {
                results.Add(new(false, ex.Message));
            }
        }

        return results;
    }

    public void Copy(IEnumerable<string> paths)
    {
        _clipboard = paths.Select(p => new ClipboardItem(p)).ToList();
        _isCut = false;
    }

    public void Cut(IEnumerable<string> paths)
    {
        _clipboard = paths.Select(p => new ClipboardItem(p)).ToList();
        _isCut = true;
    }

    public List<FileOperationResult> Paste(string destination)
    {
        var results = new List<FileOperationResult>();

        foreach (var item in _clipboard)
        {
            try
            {
                var name = Path.GetFileName(item.Path);
                var target = Path.Combine(destination, name);

                if (File.Exists(target) || Directory.Exists(target))
                {
                    results.Add(new(false, $"{name} already exists"));
                    continue;
                }

                if (Directory.Exists(item.Path))
                {
                    if (_isCut)
                        Directory.Move(item.Path, target);
                    else
                        CopyDirectory(item.Path, target);
                }
                else if (File.Exists(item.Path))
                {
                    if (_isCut)
                        File.Move(item.Path, target);
                    else
                        File.Copy(item.Path, target);
                }
                else
                {
                    results.Add(new(false, $"{name} no longer exists"));
                    continue;
                }

                results.Add(new(true));
            }
            catch (Exception ex)
            {
                results.Add(new(false, ex.Message));
            }
        }

        if (_isCut)
            _clipboard.Clear();

        return results;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);

        foreach (var file in Directory.GetFiles(source))
        {
            var name = Path.GetFileName(file);
            File.Copy(file, Path.Combine(target, name));
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            var name = Path.GetFileName(dir);
            CopyDirectory(dir, Path.Combine(target, name));
        }
    }
}