using Microsoft.AspNetCore.Components.Web;

namespace RemoteFileManager.Web.Services;

public class SelectionManager
{
    public HashSet<string> Selected { get; } = [];

    private string? _lastClicked;

    public event Action? Changed;

    void Notify() => Changed?.Invoke();

    public void Clear()
    {
        Selected.Clear();
        _lastClicked = null;
        Notify();
    }

    public void Click(
        string path,
        MouseEventArgs mouse,
        List<string> orderedPaths)
    {
        bool ctrl = mouse.CtrlKey;
        bool shift = mouse.ShiftKey;

        if (!ctrl && !shift)
        {
            Selected.Clear();
            Selected.Add(path);
            _lastClicked = path;
            Notify();
            return;
        }

        if (ctrl && !shift)
        {
            if (!Selected.Remove(path))
                Selected.Add(path);

            _lastClicked = path;
            Notify();
            return;
        }

        if (shift)
        {
            if (_lastClicked == null)
            {
                Selected.Add(path);
                _lastClicked = path;
                Notify();
                return;
            }

            var start = orderedPaths.IndexOf(_lastClicked);
            var end = orderedPaths.IndexOf(path);

            if (start == -1 || end == -1)
                return;

            if (start > end)
                (start, end) = (end, start);

            if (!ctrl)
                Selected.Clear();

            for (int i = start; i <= end; i++)
                Selected.Add(orderedPaths[i]);

            Notify();
        }
    }
}