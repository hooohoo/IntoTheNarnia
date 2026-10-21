using System.Collections.Generic;

public sealed class DialogueModel
{
    private readonly Queue<string> lines = new Queue<string>();
    public void Reset(string[] value)
    {
        lines.Clear();
        if (value == null) return;
        foreach (var line in value) lines.Enqueue(line ?? string.Empty);
    }
    public bool TryNext(out string line)
    {
        if (lines.Count == 0) { line = null; return false; }
        line = lines.Dequeue();
        return true;
    }
    public void Clear() { lines.Clear(); }
}
