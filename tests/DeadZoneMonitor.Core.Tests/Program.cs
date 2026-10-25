using DeadZoneMonitor.Core;

var tests = new (string Name, Action Run)[]
{
    ("half-open rectangle and negative coordinates", () =>
    {
        var rect = new PixelRect(-100, -50, 200, 100);
        IsTrue(rect.Contains(new PixelPoint(-100, -50)));
        IsTrue(rect.Contains(new PixelPoint(99, 49)));
        IsFalse(rect.Contains(new PixelPoint(100, 49)));
        IsFalse(rect.Contains(new PixelPoint(-101, -50)));
    }),

    ("rejects dead zone outside selected panel", () =>
    {
        Throws<ArgumentException>(() => Layout(new(900, 200, 200, 200), new(0, 0, 500, 500)));
    }),

    ("rejects any main-area dead-zone overlap", () =>
    {
        Throws<ArgumentException>(() => Layout(new(300, 200, 200, 100), new(100, 100, 400, 300)));
    }),

    ("rejects ambiguous monitor identities and overlapping monitor bounds", () =>
    {
        Throws<ArgumentException>(() => Layout(new(900, 0, 100, 700), new(0, 0, 800, 700),
            new MonitorFrame("panel", new(1000, 0, 600, 700))));
        Throws<ArgumentException>(() => Layout(new(900, 0, 100, 700), new(0, 0, 800, 700),
            new MonitorFrame("overlap", new(900, 0, 600, 700))));
    }),

    ("main-area suggestion rejects a wider but unusable strip", () =>
    {
        var layout = Layout(new(200, 40, 500, 660), new(0, 0, 200, 700));
        var result = MainAreaAdvisor.Recommend(layout, minimumWidth: 250, minimumHeight: 150);
        Eq(new PixelRect(700, 0, 300, 700), result);
        IsFalse(MainAreaAdvisor.Recommend(layout, 700, 200).HasValue);
    }),

    ("no suitable main area fails without guessing", () =>
    {
        var layout = Layout(new(0, 0, 999, 699), new(999, 0, 1, 700));
        IsFalse(MainAreaAdvisor.Recommend(layout, 100, 100).HasValue);
    }),

    ("forward internal crossing keeps orthogonal coordinate", () =>
    {
        var layout = Layout(new(300, 200, 200, 100), new(0, 0, 300, 700));
        IsTrue(layout.TryFindLanding(BlockedEdge.Left, 270, out var landing));
        Eq(new PixelPoint(500, 270), landing);
        IsTrue(layout.TryFindLanding(BlockedEdge.Right, 270, out landing));
        Eq(new PixelPoint(299, 270), landing);
        IsTrue(layout.TryFindLanding(BlockedEdge.Top, 400, out landing));
        Eq(new PixelPoint(400, 300), landing);
        IsTrue(layout.TryFindLanding(BlockedEdge.Bottom, 400, out landing));
        Eq(new PixelPoint(400, 199), landing);
    }),

    ("unique offset external monitor clamps only Y", () =>
    {
        var layout = Layout(new(900, 100, 100, 300), new(0, 0, 800, 700),
            new MonitorFrame("right", new(1000, 450, 600, 250)));
        IsTrue(layout.TryFindLanding(BlockedEdge.Left, 250, out var landing));
        Eq(new PixelPoint(1000, 450), landing);
    }),

    ("unique left external preserves negative-origin geometry", () =>
    {
        var layout = Layout(new(0, 0, 100, 700), new(100, 0, 900, 700),
            new MonitorFrame("left", new(-800, -100, 800, 500)));
        IsTrue(layout.TryFindLanding(BlockedEdge.Right, 350, out var landing));
        Eq(new PixelPoint(-1, 350), landing);
        IsTrue(layout.TryFindLanding(BlockedEdge.Right, 600, out landing));
        Eq(new PixelPoint(-1, 399), landing);
    }),

    ("multiple forward monitors must not be guessed", () =>
    {
        var layout = Layout(new(900, 0, 100, 700), new(0, 0, 800, 700),
            new MonitorFrame("right-top", new(1000, 0, 600, 300)),
            new MonitorFrame("right-bottom", new(1000, 400, 600, 300)));
        IsFalse(layout.TryFindLanding(BlockedEdge.Left, 150, out _));
    }),

    ("virtual desktop gaps do not imply phantom monitor", () =>
    {
        var layout = Layout(new(900, 0, 100, 700), new(0, 0, 800, 700),
            new MonitorFrame("right-gap", new(1001, 0, 600, 700)));
        IsFalse(layout.TryFindLanding(BlockedEdge.Left, 250, out _));
    }),

    ("no valid forward region remains blocked", () =>
    {
        var layout = Layout(new(900, 0, 100, 700), new(0, 0, 800, 700));
        IsFalse(layout.TryFindLanding(BlockedEdge.Left, 250, out _));
    }),

    ("first blocked contact never teleports despite initial overshoot", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        Eq(RoutingOutcome.Blocked, router.Advance(Contact(BlockedEdge.Left, 500, 0)).Outcome);
        var partial = router.Advance(Contact(BlockedEdge.Left, 25, 0));
        Eq(RoutingOutcome.Blocked, partial.Outcome);
        Eq(25L, partial.AccumulatedInward);
        var result = router.Advance(Contact(BlockedEdge.Left, 15, 0));
        Eq(RoutingOutcome.Jump, result.Outcome);
        Eq(new PixelPoint(500, 250), result.Landing);
    }),

    ("diagonal only accumulates inward-normal attempted motion", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        router.Advance(Contact(BlockedEdge.Left, 50, 50));
        Eq(10L, router.Advance(Contact(BlockedEdge.Left, 10, 100)).AccumulatedInward);
        Eq(10L, router.Advance(Contact(BlockedEdge.Left, 0, 100)).AccumulatedInward);
        Eq(RoutingOutcome.Blocked, router.Advance(Contact(BlockedEdge.Left, 29, 0)).Outcome);
        Eq(RoutingOutcome.Jump, router.Advance(Contact(BlockedEdge.Left, 1, 0)).Outcome);
    }),

    ("retreat discards progress and recontact starts at zero", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        router.Advance(Contact(BlockedEdge.Left, 3, 0));
        Eq(35L, router.Advance(Contact(BlockedEdge.Left, 35, 0)).AccumulatedInward);
        Eq(RoutingOutcome.Free, router.Advance(new(null, null, -5, 0, 250, Retreated: true)).Outcome);
        Eq(0L, router.Advance(Contact(BlockedEdge.Left, 1, 0)).AccumulatedInward);
        Eq(RoutingOutcome.Blocked, router.Advance(Contact(BlockedEdge.Left, 35, 0)).Outcome);
        Eq(RoutingOutcome.Jump, router.Advance(Contact(BlockedEdge.Left, 5, 0)).Outcome);
    }),

    ("corner keeps originally contacted edge", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        router.Advance(Contact(BlockedEdge.Left, 5, 0));
        var second = router.Advance(new(BlockedEdge.Top, BlockedEdge.Left, 39, 100, 250));
        Eq(BlockedEdge.Left, second.LockedEdge);
        Eq(39L, second.AccumulatedInward);
        Eq(RoutingOutcome.Jump, router.Advance(Contact(BlockedEdge.Top, 1, 0)).Outcome);
    }),

    ("simultaneous corner contact chooses larger inward component", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        var touch = router.Advance(new(BlockedEdge.Left, BlockedEdge.Top, 10, 50, 450));
        Eq(BlockedEdge.Top, touch.LockedEdge);
        Eq(0L, touch.AccumulatedInward);
        var result = router.Advance(new(BlockedEdge.Left, BlockedEdge.Top, 100, 40, 450));
        Eq(RoutingOutcome.Jump, result.Outcome);
        Eq(new PixelPoint(450, 300), result.Landing);
    }),

    ("ambiguous corner ties refuse transfer until retreat", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        Eq(RoutingOutcome.Ambiguous, router.Advance(new(BlockedEdge.Left, BlockedEdge.Top, 10, 10, 450)).Outcome);
        Eq(RoutingOutcome.Ambiguous, router.Advance(new(BlockedEdge.Left, BlockedEdge.Top, 100, 0, 450)).Outcome);
        router.Advance(new(null, null, -1, -1, 450, Retreated: true));
        Eq(RoutingOutcome.Blocked, router.Advance(Contact(BlockedEdge.Left, 5, 0)).Outcome);
        Eq(RoutingOutcome.Jump, router.Advance(Contact(BlockedEdge.Left, 40, 0)).Outcome);
    }),

    ("invalid threshold rejects activation", () =>
    {
        Throws<ArgumentOutOfRangeException>(() => new PushThroughRouter(CenterLayout(), 0));
    }),

    ("saturating progress tolerates enormous raw attempted deltas", () =>
    {
        var router = new PushThroughRouter(CenterLayout(), 40);
        router.Advance(Contact(BlockedEdge.Right, -1, 0));
        Eq(RoutingOutcome.Jump, router.Advance(Contact(BlockedEdge.Right, long.MinValue, 0)).Outcome);
    })
};

int passed = 0;
foreach (var (name, run) in tests)
{
    try
    {
        run();
        Console.WriteLine($"PASS {name}");
        passed++;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine($"FAIL {name}: {error}");
        Environment.ExitCode = 1;
    }
}
Console.WriteLine($"{passed}/{tests.Length} cases passed.");

static ZoneLayout Layout(PixelRect dead, PixelRect main, params MonitorFrame[] external)
{
    var monitors = new List<MonitorFrame> { new("panel", new(0, 0, 1000, 700)) };
    monitors.AddRange(external);
    return new ZoneLayout(monitors, "panel", dead, main);
}

static ZoneLayout CenterLayout() =>
    Layout(new PixelRect(300, 200, 200, 100), new PixelRect(0, 0, 300, 700));

static ContactAttempt Contact(BlockedEdge edge, long dx, long dy) =>
    new(edge, null, dx, dy, 250);

static void Eq<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}, got {actual}");
}

static void IsTrue(bool condition)
{
    if (!condition) throw new Exception("Expected true");
}

static void IsFalse(bool condition)
{
    if (condition) throw new Exception("Expected false");
}

static void Throws<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    throw new Exception($"Expected {typeof(TException).Name}");
}
