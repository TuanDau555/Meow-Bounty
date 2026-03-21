using System.Collections.Generic;

public class TargetRegistry
{
    private static readonly List<ITargetable> targetables = new List<ITargetable>();

    public static IReadOnlyList<ITargetable> Targetables => targetables;

    public static void Register(ITargetable target)
    {
        if (!targetables.Contains(target))
        {
            targetables.Add(target);
        }
    }

    public static void UnRegister(ITargetable target)
    {
        if (targetables.Contains(target))
        {
            targetables.Remove(target);
        }
    }
}