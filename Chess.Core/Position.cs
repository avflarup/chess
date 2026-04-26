namespace Chess.Core;

public sealed class Position : IEquatable<Position>
{
    public char File { get; }
    public int Rank { get; }

    public Position(char file, int rank)
    {
        if (file < 'a' || file > 'h')
            throw new ArgumentOutOfRangeException(nameof(file), "File must be between 'a' and 'h'.");
        if (rank < 1 || rank > 8)
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 1 and 8.");

        File = file;
        Rank = rank;
    }

    public static Position Parse(string square)
    {
        if (string.IsNullOrWhiteSpace(square) || square.Length != 2)
            throw new ArgumentException("Invalid square format.", nameof(square));

        return new Position(square[0], square[1] - '0');
    }

    public override bool Equals(object? obj) => Equals(obj as Position);

    public bool Equals(Position? other) => other is not null && File == other.File && Rank == other.Rank;

    public override int GetHashCode() => HashCode.Combine(File, Rank);

    public override string ToString() => $"{File}{Rank}";
}
