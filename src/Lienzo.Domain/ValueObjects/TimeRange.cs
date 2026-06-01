namespace Lienzo.Domain.ValueObjects;

public class TimeRange : IEquatable<TimeRange>
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }

    public TimeRange(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new ArgumentException("Start time must be before end time", nameof(start));

        Start = start;
        End = end;
    }

    public bool OverlapsWith(TimeRange other) => Start < other.End && other.Start < End;

    public bool Equals(TimeRange? other) =>
        other is not null && Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => Equals(obj as TimeRange);

    public override int GetHashCode() => HashCode.Combine(Start, End);
}
