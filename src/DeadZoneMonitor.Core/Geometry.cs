namespace DeadZoneMonitor.Core;

/// <summary>A virtual-desktop point in physical pixels (not WPF/WinForms logical units).</summary>
public readonly record struct PixelPoint(long X, long Y);

/// <summary>Half-open physical-pixel rectangle: [X, Right) x [Y, Bottom).</summary>
public readonly record struct PixelRect(long X, long Y, long Width, long Height)
{
    public long Right => checked(X + Width);
    public long Bottom => checked(Y + Height);
    public long Area => checked(Width * Height);

    public void Validate()
    {
        if (Width <= 0 || Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(Width), "Rectangle must have positive dimensions.");
        _ = Right;
        _ = Bottom;
    }

    public bool Contains(PixelPoint point) =>
        point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;

    public bool Contains(PixelRect other) =>
        other.Width > 0 && other.Height > 0 &&
        other.X >= X && other.Y >= Y &&
        other.Right <= Right && other.Bottom <= Bottom;

    public bool Intersects(PixelRect other) =>
        X < other.Right && other.X < Right && Y < other.Bottom && other.Y < Bottom;

    public PixelRect Translate(long dx, long dy) =>
        new(checked(X + dx), checked(Y + dy), Width, Height);
}

public enum BlockedEdge
{
    Left,
    Right,
    Top,
    Bottom
}

/// <summary>Existing Windows monitor identity and virtual-desktop pixel bounds.</summary>
public sealed record MonitorFrame(string Id, PixelRect Bounds);

/// <summary>
/// Validated single-target topology. Construction never guesses which monitor is damaged.
/// No input hooks or OS display changes occur in this model.
/// </summary>
public sealed class ZoneLayout
{
    public IReadOnlyList<MonitorFrame> Monitors { get; }
    public MonitorFrame Target { get; }
    public PixelRect DeadZone { get; }
    public PixelRect MainArea { get; }

    public ZoneLayout(
        IEnumerable<MonitorFrame> monitors,
        string targetId,
        PixelRect deadZoneLocal,
        PixelRect mainAreaLocal)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("A selected physical monitor identity is required.", nameof(targetId));

        var frames = monitors.ToArray();
        if (frames.Length == 0)
            throw new ArgumentException("No monitors were provided.", nameof(monitors));

        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var frame in frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Id) || !identities.Add(frame.Id))
                throw new ArgumentException("Monitor identities must be nonempty and unique.", nameof(monitors));
            frame.Bounds.Validate();
        }

        for (int i = 0; i < frames.Length; i++)
        for (int j = i + 1; j < frames.Length; j++)
            if (frames[i].Bounds.Intersects(frames[j].Bounds))
                throw new ArgumentException("Overlapping monitor bounds are ambiguous.", nameof(monitors));

        Target = frames.SingleOrDefault(m => m.Id == targetId)
            ?? throw new ArgumentException("Selected target monitor is not present.", nameof(targetId));

        deadZoneLocal.Validate();
        mainAreaLocal.Validate();
        var targetLocalBounds = new PixelRect(0, 0, Target.Bounds.Width, Target.Bounds.Height);
        if (!targetLocalBounds.Contains(deadZoneLocal))
            throw new ArgumentException("Dead zone is outside the selected monitor.", nameof(deadZoneLocal));
        if (!targetLocalBounds.Contains(mainAreaLocal) || mainAreaLocal.Intersects(deadZoneLocal))
            throw new ArgumentException("Main area must be wholly intact on the selected monitor.", nameof(mainAreaLocal));

        DeadZone = deadZoneLocal.Translate(Target.Bounds.X, Target.Bounds.Y);
        MainArea = mainAreaLocal.Translate(Target.Bounds.X, Target.Bounds.Y);
        Monitors = Array.AsReadOnly(frames);
    }

    public bool IsAllowed(PixelPoint point) =>
        Monitors.Any(m => m.Bounds.Contains(point)) && !DeadZone.Contains(point);

    /// <summary>
    /// Returns a safe destination only when the forward region is unambiguous.
    /// A directly reachable intact part of the target always has precedence.
    /// External-monitor fallback requires exactly one monitor touching the forward side.
    /// Coordinates are corrected only after that target is uniquely selected.
    /// A virtual-desktop gap is not treated as a traversable display.
    /// </summary>
    public bool TryFindLanding(BlockedEdge edge, long orthogonal, out PixelPoint landing)
    {
        PixelPoint internalLanding = edge switch
        {
            BlockedEdge.Left => new(DeadZone.Right, orthogonal),
            BlockedEdge.Right => new(DeadZone.X - 1, orthogonal),
            BlockedEdge.Top => new(orthogonal, DeadZone.Bottom),
            BlockedEdge.Bottom => new(orthogonal, DeadZone.Y - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(edge))
        };

        if (Target.Bounds.Contains(internalLanding) && !DeadZone.Contains(internalLanding))
        {
            landing = internalLanding;
            return true;
        }

        var adjacent = Monitors
            .Where(m => m.Id != Target.Id && IsForwardAdjacent(m.Bounds, edge))
            .ToArray();
        if (adjacent.Length != 1)
        {
            landing = default;
            return false;
        }

        var bounds = adjacent[0].Bounds;
        landing = edge switch
        {
            BlockedEdge.Left => new(bounds.X, Math.Clamp(orthogonal, bounds.Y, bounds.Bottom - 1)),
            BlockedEdge.Right => new(bounds.Right - 1, Math.Clamp(orthogonal, bounds.Y, bounds.Bottom - 1)),
            BlockedEdge.Top => new(Math.Clamp(orthogonal, bounds.X, bounds.Right - 1), bounds.Y),
            BlockedEdge.Bottom => new(Math.Clamp(orthogonal, bounds.X, bounds.Right - 1), bounds.Bottom - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(edge))
        };
        return IsAllowed(landing);
    }

    private bool IsForwardAdjacent(PixelRect other, BlockedEdge edge) => edge switch
    {
        BlockedEdge.Left => other.X == Target.Bounds.Right,
        BlockedEdge.Right => other.Right == Target.Bounds.X,
        BlockedEdge.Top => other.Y == Target.Bounds.Bottom,
        BlockedEdge.Bottom => other.Bottom == Target.Bounds.Y,
        _ => false
    };
}

/// <summary>
/// Suggests usable main rectangles, with minimum dimensions supplied by the eventual shell UI.
/// No unverified fixed UI minimum is embedded in the geometry engine.
/// </summary>
public static class MainAreaAdvisor
{
    public static IReadOnlyList<PixelRect> Candidates(ZoneLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var bounds = layout.Target.Bounds;
        var dead = layout.DeadZone;
        var result = new List<PixelRect>(4);

        if (dead.Y > bounds.Y)
            result.Add(new(bounds.X, bounds.Y, bounds.Width, dead.Y - bounds.Y));
        if (dead.Bottom < bounds.Bottom)
            result.Add(new(bounds.X, dead.Bottom, bounds.Width, bounds.Bottom - dead.Bottom));
        if (dead.X > bounds.X)
            result.Add(new(bounds.X, bounds.Y, dead.X - bounds.X, bounds.Height));
        if (dead.Right < bounds.Right)
            result.Add(new(dead.Right, bounds.Y, bounds.Right - dead.Right, bounds.Height));

        return result.AsReadOnly();
    }

    public static PixelRect? Recommend(ZoneLayout layout, long minimumWidth, long minimumHeight)
    {
        if (minimumWidth <= 0 || minimumHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumWidth), "Minimum dimensions must be positive.");
        return Candidates(layout)
            .Where(rect => rect.Width >= minimumWidth && rect.Height >= minimumHeight)
            .OrderByDescending(rect => rect.Area)
            .ThenByDescending(rect => Math.Min(rect.Width, rect.Height))
            .ThenBy(rect => rect.Y)
            .ThenBy(rect => rect.X)
            .Select(rect => (PixelRect?)rect)
            .FirstOrDefault();
    }
}
