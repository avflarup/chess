namespace Chess.Core;

public sealed class Board
{
    private readonly Dictionary<Position, Piece> _pieces;

    public IReadOnlyDictionary<Position, Piece> Pieces => _pieces;

    public PieceColor CurrentTurn { get; private set; }
    public Position? EnPassantTarget { get; private set; }
    public bool WhiteCanCastleKingSide { get; private set; }
    public bool WhiteCanCastleQueenSide { get; private set; }
    public bool BlackCanCastleKingSide { get; private set; }
    public bool BlackCanCastleQueenSide { get; private set; }

    public Board()
    {
        _pieces = new Dictionary<Position, Piece>();
        InitializeStandardBoard();
    }

    public Board(Board other)
    {
        _pieces = new Dictionary<Position, Piece>(other._pieces);
        CurrentTurn = other.CurrentTurn;
        EnPassantTarget = other.EnPassantTarget;
        WhiteCanCastleKingSide = other.WhiteCanCastleKingSide;
        WhiteCanCastleQueenSide = other.WhiteCanCastleQueenSide;
        BlackCanCastleKingSide = other.BlackCanCastleKingSide;
        BlackCanCastleQueenSide = other.BlackCanCastleQueenSide;
    }

    public Board Clone() => new(this);

    public Piece? GetPieceAt(Position position)
    {
        return _pieces.TryGetValue(position, out var piece) ? piece : null;
    }

    public bool IsLegalMove(Position from, Position to, PieceType? promotion = null)
    {
        if (!_pieces.TryGetValue(from, out var piece))
            return false;

        if (from.Equals(to))
            return false;

        if (_pieces.TryGetValue(to, out var targetPiece) && targetPiece.Color == piece.Color)
            return false;

        if (!IsLegalMoveByPiece(piece, from, to, promotion))
            return false;

        var simulated = Clone();
        simulated.ApplyMove(from, to, promotion, ignoreTurn: true);
        return !simulated.IsInCheck(piece.Color);
    }

    public bool IsLegalMoveForCurrentPlayer(Position from, Position to, PieceType? promotion = null)
    {
        if (!_pieces.TryGetValue(from, out var piece))
            return false;

        if (piece.Color != CurrentTurn)
            return false;

        return IsLegalMove(from, to, promotion);
    }

    public void MovePiece(Position from, Position to, PieceType? promotion = null)
    {
        if (!_pieces.TryGetValue(from, out var piece))
            throw new InvalidOperationException($"No piece at {from}.");

        if (piece.Color != CurrentTurn)
            throw new InvalidOperationException($"It's {CurrentTurn}'s turn.");

        if (!IsLegalMove(from, to, promotion))
            throw new InvalidOperationException($"Illegal move from {from} to {to}.");

        ApplyMove(from, to, promotion, ignoreTurn: true);
        CurrentTurn = Opponent(CurrentTurn);
    }

    public void InitializeStandardBoard()
    {
        _pieces.Clear();
        CurrentTurn = PieceColor.White;
        EnPassantTarget = null;
        WhiteCanCastleKingSide = true;
        WhiteCanCastleQueenSide = true;
        BlackCanCastleKingSide = true;
        BlackCanCastleQueenSide = true;

        AddPiece("a1", PieceColor.White, PieceType.Rook);
        AddPiece("b1", PieceColor.White, PieceType.Knight);
        AddPiece("c1", PieceColor.White, PieceType.Bishop);
        AddPiece("d1", PieceColor.White, PieceType.Queen);
        AddPiece("e1", PieceColor.White, PieceType.King);
        AddPiece("f1", PieceColor.White, PieceType.Bishop);
        AddPiece("g1", PieceColor.White, PieceType.Knight);
        AddPiece("h1", PieceColor.White, PieceType.Rook);
        for (char file = 'a'; file <= 'h'; file++)
        {
            AddPiece($"{file}2", PieceColor.White, PieceType.Pawn);
        }

        AddPiece("a8", PieceColor.Black, PieceType.Rook);
        AddPiece("b8", PieceColor.Black, PieceType.Knight);
        AddPiece("c8", PieceColor.Black, PieceType.Bishop);
        AddPiece("d8", PieceColor.Black, PieceType.Queen);
        AddPiece("e8", PieceColor.Black, PieceType.King);
        AddPiece("f8", PieceColor.Black, PieceType.Bishop);
        AddPiece("g8", PieceColor.Black, PieceType.Knight);
        AddPiece("h8", PieceColor.Black, PieceType.Rook);
        for (char file = 'a'; file <= 'h'; file++)
        {
            AddPiece($"{file}7", PieceColor.Black, PieceType.Pawn);
        }
    }

    public void SetPiece(Position position, Piece piece)
    {
        _pieces[position] = piece;
    }

    public void RemovePiece(Position position)
    {
        _pieces.Remove(position);
    }

    public void Clear()
    {
        _pieces.Clear();
    }

    public bool IsInCheck(PieceColor color)
    {
        var kingPosition = _pieces.FirstOrDefault(kvp => kvp.Value.Type == PieceType.King && kvp.Value.Color == color).Key;
        if (kingPosition is null)
            return false;

        return IsSquareAttacked(kingPosition, Opponent(color));
    }

    public bool IsCheckmate(PieceColor color)
    {
        if (!IsInCheck(color))
            return false;

        foreach (var kvp in _pieces.Where(kvp => kvp.Value.Color == color))
        {
            var from = kvp.Key;
            for (char file = 'a'; file <= 'h'; file++)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    var to = new Position(file, rank);
                    if (IsLegalMove(from, to))
                        return false;
                }
            }
        }

        return true;
    }

    public bool IsStalemate(PieceColor color)
    {
        if (IsInCheck(color))
            return false;

        foreach (var kvp in _pieces.Where(kvp => kvp.Value.Color == color))
        {
            var from = kvp.Key;
            for (char file = 'a'; file <= 'h'; file++)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    var to = new Position(file, rank);
                    if (IsLegalMove(from, to))
                        return false;
                }
            }
        }

        return true;
    }

    private static bool IsLinearMove(Position from, Position to)
    {
        return from.File == to.File || from.Rank == to.Rank;
    }

    private static bool IsDiagonalMove(Position from, Position to)
    {
        return Math.Abs(from.File - to.File) == Math.Abs(from.Rank - to.Rank);
    }

    private bool IsPathClear(Position from, Position to)
    {
        var fileStep = Math.Sign(to.File - from.File);
        var rankStep = Math.Sign(to.Rank - from.Rank);

        var currentFile = (char)(from.File + fileStep);
        var currentRank = from.Rank + rankStep;

        while (currentFile != to.File || currentRank != to.Rank)
        {
            var currentPosition = new Position(currentFile, currentRank);
            if (_pieces.ContainsKey(currentPosition))
                return false;

            currentFile = (char)(currentFile + fileStep);
            currentRank += rankStep;
        }

        return true;
    }

    private bool IsLegalMoveByPiece(Piece piece, Position from, Position to, PieceType? promotion)
    {
        return piece.Type switch
        {
            PieceType.Pawn => IsLegalPawnMove(piece, from, to, promotion),
            PieceType.Knight => IsLegalKnightMove(from, to),
            PieceType.Bishop => IsLegalBishopMove(from, to),
            PieceType.Rook => IsLegalRookMove(from, to),
            PieceType.Queen => IsLegalQueenMove(from, to),
            PieceType.King => IsLegalKingMove(piece, from, to),
            _ => false,
        };
    }

    private bool IsLegalPawnMove(Piece piece, Position from, Position to, PieceType? promotion)
    {
        var direction = piece.Color == PieceColor.White ? 1 : -1;
        var fileDelta = to.File - from.File;
        var rankDelta = to.Rank - from.Rank;
        var targetPiece = GetPieceAt(to);

        var isPromotionRank = to.Rank == (piece.Color == PieceColor.White ? 8 : 1);
        if (isPromotionRank && promotion is null)
            promotion = PieceType.Queen;

        if (promotion is not null && piece.Type != PieceType.Pawn)
            return false;

        if (fileDelta == 0)
        {
            if (targetPiece is not null)
                return false;

            if (rankDelta == direction)
                return true;

            if (rankDelta == 2 * direction)
            {
                var startingRank = piece.Color == PieceColor.White ? 2 : 7;
                var intermediate = new Position(from.File, from.Rank + direction);
                return from.Rank == startingRank && GetPieceAt(intermediate) is null;
            }

            return false;
        }

        if (Math.Abs(fileDelta) == 1 && rankDelta == direction)
        {
            if (targetPiece is not null)
                return targetPiece.Color != piece.Color;

            return EnPassantTarget is not null && to.Equals(EnPassantTarget);
        }

        return false;
    }

    private static bool IsLegalKnightMove(Position from, Position to)
    {
        var fileDelta = Math.Abs(from.File - to.File);
        var rankDelta = Math.Abs(from.Rank - to.Rank);

        return (fileDelta == 1 && rankDelta == 2) || (fileDelta == 2 && rankDelta == 1);
    }

    private bool IsLegalBishopMove(Position from, Position to)
    {
        if (!IsDiagonalMove(from, to))
            return false;

        return IsPathClear(from, to);
    }

    private bool IsLegalRookMove(Position from, Position to)
    {
        if (!IsLinearMove(from, to))
            return false;

        return IsPathClear(from, to);
    }

    private bool IsLegalQueenMove(Position from, Position to)
    {
        if (!IsLinearMove(from, to) && !IsDiagonalMove(from, to))
            return false;

        return IsPathClear(from, to);
    }

    private bool IsLegalKingMove(Piece king, Position from, Position to)
    {
        var fileDelta = Math.Abs(from.File - to.File);
        var rankDelta = Math.Abs(from.Rank - to.Rank);

        if (fileDelta <= 1 && rankDelta <= 1)
            return true;

        if (rankDelta == 0 && fileDelta == 2)
            return CanCastle(king, from, to);

        return false;
    }

    private bool CanCastle(Piece king, Position from, Position to)
    {
        if (king.HasMoved)
            return false;

        if (IsInCheck(king.Color))
            return false;

        var kingSide = to.File > from.File;
        var rookFrom = kingSide ? new Position('h', from.Rank) : new Position('a', from.Rank);
        var rook = GetPieceAt(rookFrom);

        if (rook is null || rook.Type != PieceType.Rook || rook.Color != king.Color || rook.HasMoved)
            return false;

        if (IsCastlingDisabled(king.Color, kingSide))
            return false;

        var pathSquares = kingSide
            ? new[] { new Position((char)(from.File + 1), from.Rank), new Position((char)(from.File + 2), from.Rank) }
            : new[] { new Position('d', from.Rank), new Position((char)(from.File - 2), from.Rank) };

        foreach (var square in pathSquares)
        {
            if (GetPieceAt(square) is not null)
                return false;

            if (IsSquareAttacked(square, Opponent(king.Color)))
                return false;
        }

        return true;
    }

    private bool IsCastlingDisabled(PieceColor color, bool kingSide)
    {
        return color == PieceColor.White
            ? kingSide ? !WhiteCanCastleKingSide : !WhiteCanCastleQueenSide
            : kingSide ? !BlackCanCastleKingSide : !BlackCanCastleQueenSide;
    }

    private bool IsSquareAttacked(Position square, PieceColor byColor)
    {
        foreach (var kvp in _pieces)
        {
            var attacker = kvp.Value;
            if (attacker.Color != byColor)
                continue;

            if (CanAttack(attacker, kvp.Key, square))
                return true;
        }

        return false;
    }

    private bool CanAttack(Piece attacker, Position from, Position to)
    {
        return attacker.Type switch
        {
            PieceType.Pawn => CanPawnAttack(attacker, from, to),
            PieceType.Knight => IsLegalKnightMove(from, to),
            PieceType.Bishop => IsLegalBishopMove(from, to),
            PieceType.Rook => IsLegalRookMove(from, to),
            PieceType.Queen => IsLegalQueenMove(from, to),
            PieceType.King => Math.Abs(from.File - to.File) <= 1 && Math.Abs(from.Rank - to.Rank) <= 1,
            _ => false,
        };
    }

    private bool CanPawnAttack(Piece pawn, Position from, Position to)
    {
        var direction = pawn.Color == PieceColor.White ? 1 : -1;
        var fileDelta = to.File - from.File;
        var rankDelta = to.Rank - from.Rank;

        return Math.Abs(fileDelta) == 1 && rankDelta == direction;
    }

    private void ApplyMove(Position from, Position to, PieceType? promotion, bool ignoreTurn)
    {
        var piece = _pieces[from];
        var targetPiece = GetPieceAt(to);
        var isCastling = piece.Type == PieceType.King && Math.Abs(to.File - from.File) == 2;
        var wasPawnDoubleStep = piece.Type == PieceType.Pawn && Math.Abs(to.Rank - from.Rank) == 2;
        var movedPiece = piece.WithMoved();

        if (targetPiece is not null && targetPiece.Type == PieceType.Rook && targetPiece.Color != piece.Color)
            DisableCastlingRightsForRook(targetPiece.Color, to);

        if (isCastling)
            MoveRookForCastling(piece.Color, from, to);

        if (piece.Type == PieceType.Pawn && targetPiece is null && from.File != to.File && EnPassantTarget is not null && to.Equals(EnPassantTarget))
        {
            var capturePosition = new Position(to.File, from.Rank);
            _pieces.Remove(capturePosition);
        }

        if (piece.Type == PieceType.Pawn && IsPromotionRank(to, piece.Color) && promotion is null)
            promotion = PieceType.Queen;

        if (piece.Type == PieceType.Pawn && promotion is not null && IsPromotionRank(to, piece.Color))
            movedPiece = movedPiece.WithType(promotion.Value);

        if (piece.Type == PieceType.King)
            DisableCastlingRightsForKing(piece.Color);

        if (piece.Type == PieceType.Rook)
            DisableCastlingRightsForRook(piece.Color, from);

        _pieces.Remove(from);
        _pieces[to] = movedPiece;

        EnPassantTarget = wasPawnDoubleStep ? new Position(from.File, from.Rank + Math.Sign(to.Rank - from.Rank)) : null;

        if (targetPiece is not null && targetPiece.Type == PieceType.Rook && targetPiece.Color != piece.Color)
            DisableCastlingRightsForRook(targetPiece.Color, to);
    }

    private static bool IsPromotionRank(Position position, PieceColor color)
    {
        return position.Rank == (color == PieceColor.White ? 8 : 1);
    }

    private void MoveRookForCastling(PieceColor color, Position kingFrom, Position kingTo)
    {
        var kingSide = kingTo.File > kingFrom.File;
        var rookFrom = kingSide ? new Position('h', kingFrom.Rank) : new Position('a', kingFrom.Rank);
        var rookTo = kingSide ? new Position('f', kingFrom.Rank) : new Position('d', kingFrom.Rank);

        if (_pieces.TryGetValue(rookFrom, out var rook))
        {
            _pieces.Remove(rookFrom);
            _pieces[rookTo] = rook.WithMoved();
        }
    }

    private void DisableCastlingRightsForKing(PieceColor color)
    {
        if (color == PieceColor.White)
        {
            WhiteCanCastleKingSide = false;
            WhiteCanCastleQueenSide = false;
        }
        else
        {
            BlackCanCastleKingSide = false;
            BlackCanCastleQueenSide = false;
        }
    }

    private void DisableCastlingRightsForRook(PieceColor color, Position rookPosition)
    {
        if (color == PieceColor.White)
        {
            if (rookPosition.Equals(Position.Parse("h1")))
                WhiteCanCastleKingSide = false;
            else if (rookPosition.Equals(Position.Parse("a1")))
                WhiteCanCastleQueenSide = false;
        }
        else
        {
            if (rookPosition.Equals(Position.Parse("h8")))
                BlackCanCastleKingSide = false;
            else if (rookPosition.Equals(Position.Parse("a8")))
                BlackCanCastleQueenSide = false;
        }
    }

    private static PieceColor Opponent(PieceColor color)
    {
        return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private void AddPiece(string square, PieceColor color, PieceType type)
    {
        var position = Position.Parse(square);
        _pieces[position] = new Piece(color, type);
    }
}
