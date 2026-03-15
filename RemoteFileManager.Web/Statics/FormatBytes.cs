namespace RemoteFileManager.Web.Statics;

public static class FormatBytes
{
    public static string Format(long length)
    {
        if (length < 1024) return $"{length} B";
        if (length < 1024 * 1024) return $"{length / 1024d:F2} KB";
        if (length < 1024L * 1024 * 1024) return $"{length / 1024d / 1024:F2} MB";
        if (length < 1024L * 1024 * 1024 * 1024) return $"{length / 1024d / 1024 / 1024:F2} GB";
        return $"{length / 1024d / 1024 / 1024 / 1024:F2} TB";
    }
}