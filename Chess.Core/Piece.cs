namespace Chess.Core;

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public enum PieceColor
{
    White,
    Black
}

public sealed class Piece
{
    public PieceColor Color { get; }
    public PieceType Type { get; }
    public bool HasMoved { get; }

    public Piece(PieceColor color, PieceType type, bool hasMoved = false)
    {
        Color = color;
        Type = type;
        HasMoved = hasMoved;
    }

    public Piece WithMoved() => new(Color, Type, true);

    public Piece WithType(PieceType type) => new(Color, type, true);

    public override string ToString() => $"{Color} {Type}";
}
