using MudBlazor;

namespace RemoteFileManager.Web.Statics;

public static class FileIcon
{

    public static string Get(FileSystemInfo item)
    {
        if (item is DirectoryInfo dir)
        {
            if (item.Attributes.HasFlag(FileAttributes.Hidden))
            {
                return Icons.Material.Filled.FolderOpen;
            }
            else return Icons.Material.Filled.Folder;
        }
        else if (item is FileInfo file)
        {
            if (file.Attributes.HasFlag(FileAttributes.Hidden))
            {
                return Icons.Material.Outlined.Description;
            }
            return Icons.Material.Filled.Description;
        }
        else return "";
    }
    
}