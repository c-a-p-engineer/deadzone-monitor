namespace DeadZoneMonitor.Core;

/// <summary>
/// Input-adapter report for one sampled blocked-boundary interaction.
/// A null first edge means the pointer is no longer on the blocked boundary.
/// Retreated is asserted only when the pointer has moved back into allowed space.
/// DeltaX/DeltaY are attempted input deltas, NOT cursor movement after clamping.
/// </summary>
public readonly record struct ContactAttempt(
    BlockedEdge? FirstEdge,
    BlockedEdge? SecondEdge,
    long DeltaX,
    long DeltaY,
    long OrthogonalPosition,
    bool Retreated = false);

public enum RoutingOutcome
{
    Free,
    Blocked,
    Jump,
    Ambiguous
}

public readonly record struct RoutingDecision(
    RoutingOutcome Outcome,
    BlockedEdge? LockedEdge,
    long AccumulatedInward,
    PixelPoint? Landing = null);

/// <summary>
/// Pure push-through gesture state machine. No Windows hooks, injection, global
/// pointer clipping or implicit startup. The future adapter must validate the
/// source contact and deliver reliable attempted input deltas.
/// </summary>
public sealed class PushThroughRouter
{
    private readonly ZoneLayout _layout;
    private readonly long _threshold;
    private BlockedEdge? _lockedEdge;
    private long _progress;
    private bool _ambiguousContact;

    public PushThroughRouter(ZoneLayout layout, long inwardThreshold)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        if (inwardThreshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(inwardThreshold));
        _threshold = inwardThreshold;
    }

    public void Reset()
    {
        _lockedEdge = null;
        _progress = 0;
        _ambiguousContact = false;
    }

    public RoutingDecision Advance(ContactAttempt sample)
    {
        if (sample.Retreated || sample.FirstEdge is null)
        {
            Reset();
            return new(RoutingOutcome.Free, null, 0);
        }

        if (_ambiguousContact)
            return new(RoutingOutcome.Ambiguous, null, 0);

        if (_lockedEdge is null)
        {
            var selected = sample.FirstEdge.Value;
            if (sample.SecondEdge is { } second && selected != second)
            {
                var firstDelta = PositiveInward(selected, sample.DeltaX, sample.DeltaY);
                var secondDelta = PositiveInward(second, sample.DeltaX, sample.DeltaY);
                if (firstDelta == secondDelta)
                {
                    // Never resolve an exact or indeterminate corner tie arbitrarily.
                    // A fresh retreat and contact is required before transfer.
                    _ambiguousContact = true;
                    return new(RoutingOutcome.Ambiguous, null, 0);
                }
                if (secondDelta > firstDelta)
                    selected = second;
            }

            _lockedEdge = selected;
            // Initial touch ALWAYS stops first: any overshoot in this sample is
            // deliberately ignored until a later explicit inward attempt.
            return new(RoutingOutcome.Blocked, selected, 0);
        }

        var inward = PositiveInward(_lockedEdge.Value, sample.DeltaX, sample.DeltaY);
        if (inward == 0)
            return new(RoutingOutcome.Blocked, _lockedEdge, _progress);

        // Saturating arithmetic prevents high-speed/invalid device deltas from
        // overflowing the threshold counter.
        var add = Math.Min(inward, _threshold - _progress);
        _progress += add;
        if (_progress < _threshold)
            return new(RoutingOutcome.Blocked, _lockedEdge, _progress);

        if (!_layout.TryFindLanding(_lockedEdge.Value, sample.OrthogonalPosition, out var landing))
            return new(RoutingOutcome.Blocked, _lockedEdge, _progress);

        Reset(); // Synthetic warp must not inherit any previous push.
        return new(RoutingOutcome.Jump, null, 0, landing);
    }

    private static long PositiveInward(BlockedEdge edge, long dx, long dy)
    {
        // Comparing signs first avoids negating long.MinValue.
        return edge switch
        {
            BlockedEdge.Left when dx > 0 => dx,
            BlockedEdge.Right when dx < 0 => SaturatingAbs(dx),
            BlockedEdge.Top when dy > 0 => dy,
            BlockedEdge.Bottom when dy < 0 => SaturatingAbs(dy),
            _ => 0
        };
    }

    private static long SaturatingAbs(long value) =>
        value == long.MinValue ? long.MaxValue : -value;
}
