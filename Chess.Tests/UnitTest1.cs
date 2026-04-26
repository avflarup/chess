using System.Linq;
using Chess.Core;

namespace Chess.Tests;

public class BoardTests
{
    [Fact]
    public void StandardBoard_ShouldPlaceKingsAndPawnsCorrectly()
    {
        var board = new Board();

        var whiteKing = board.GetPieceAt(Position.Parse("e1"));
        var blackKing = board.GetPieceAt(Position.Parse("e8"));
        var whitePawn = board.GetPieceAt(Position.Parse("e2"));

        Assert.NotNull(whiteKing);
        Assert.Equal(PieceColor.White, whiteKing!.Color);
        Assert.Equal(PieceType.King, whiteKing.Type);

        Assert.NotNull(blackKing);
        Assert.Equal(PieceColor.Black, blackKing!.Color);
        Assert.Equal(PieceType.King, blackKing.Type);

        Assert.NotNull(whitePawn);
        Assert.Equal(PieceColor.White, whitePawn!.Color);
        Assert.Equal(PieceType.Pawn, whitePawn.Type);
    }

    [Fact]
    public void MovePiece_ShouldAlternateTurns()
    {
        var board = new Board();

        board.MovePiece(Position.Parse("e2"), Position.Parse("e4"));
        Assert.Equal(PieceColor.Black, board.CurrentTurn);

        board.MovePiece(Position.Parse("d7"), Position.Parse("d5"));
        Assert.Equal(PieceColor.White, board.CurrentTurn);
    }

    [Fact]
    public void MovePiece_ShouldNotAllowMovingOpponentsPiece()
    {
        var board = new Board();

        Assert.Throws<InvalidOperationException>(() => board.MovePiece(Position.Parse("a7"), Position.Parse("a6")));
    }

    [Fact]
    public void Pawn_ShouldAllowSingleAndDoubleAdvance()
    {
        var board = new Board();

        Assert.True(board.IsLegalMove(Position.Parse("e2"), Position.Parse("e3")));
        Assert.True(board.IsLegalMove(Position.Parse("e2"), Position.Parse("e4")));
        Assert.False(board.IsLegalMove(Position.Parse("e2"), Position.Parse("e5")));
    }

    [Fact]
    public void Pawn_ShouldAllowDiagonalCapture()
    {
        var board = new Board();

        board.MovePiece(Position.Parse("e2"), Position.Parse("e4"));
        board.MovePiece(Position.Parse("d7"), Position.Parse("d5"));

        Assert.True(board.IsLegalMove(Position.Parse("e4"), Position.Parse("d5")));
        board.MovePiece(Position.Parse("e4"), Position.Parse("d5"));

        var capturedPawn = board.GetPieceAt(Position.Parse("d5"));
        Assert.NotNull(capturedPawn);
        Assert.Equal(PieceColor.White, capturedPawn!.Color);
        Assert.Equal(PieceType.Pawn, capturedPawn.Type);
    }

    [Fact]
    public void EnPassant_ShouldBeLegalImmediatelyAfterDoubleStep()
    {
        var board = new Board();

        board.MovePiece(Position.Parse("e2"), Position.Parse("e4"));
        board.MovePiece(Position.Parse("a7"), Position.Parse("a6"));
        board.MovePiece(Position.Parse("e4"), Position.Parse("e5"));
        board.MovePiece(Position.Parse("d7"), Position.Parse("d5"));

        Assert.True(board.IsLegalMove(Position.Parse("e5"), Position.Parse("d6")));
        board.MovePiece(Position.Parse("e5"), Position.Parse("d6"));

        Assert.Null(board.GetPieceAt(Position.Parse("d5")));
        var capturedPawn = board.GetPieceAt(Position.Parse("d6"));
        Assert.NotNull(capturedPawn);
        Assert.Equal(PieceType.Pawn, capturedPawn!.Type);
    }

    [Fact]
    public void Knight_ShouldMoveInLShape()
    {
        var board = new Board();

        Assert.True(board.IsLegalMove(Position.Parse("g1"), Position.Parse("f3")));
        Assert.False(board.IsLegalMove(Position.Parse("g1"), Position.Parse("g3")));
    }

    [Fact]
    public void Bishop_ShouldMoveDiagonallyWhenPathIsClear()
    {
        var board = new Board();
        board.MovePiece(Position.Parse("d2"), Position.Parse("d3"));

        Assert.True(board.IsLegalMove(Position.Parse("c1"), Position.Parse("g5")));
    }

    [Fact]
    public void Rook_ShouldMoveStraightWhenPathIsClear()
    {
        var board = new Board();
        board.MovePiece(Position.Parse("a2"), Position.Parse("a4"));

        Assert.True(board.IsLegalMove(Position.Parse("a1"), Position.Parse("a2")));
    }

    [Fact]
    public void Queen_ShouldMoveLikeRookAndBishop()
    {
        var board = new Board();
        board.MovePiece(Position.Parse("e2"), Position.Parse("e3"));

        Assert.True(board.IsLegalMove(Position.Parse("d1"), Position.Parse("h5")));
    }

    [Fact]
    public void King_ShouldCastleKingsideWhenPathIsClear()
    {
        var board = new Board();
        board.MovePiece(Position.Parse("e2"), Position.Parse("e3"));
        board.MovePiece(Position.Parse("a7"), Position.Parse("a6"));
        board.MovePiece(Position.Parse("g1"), Position.Parse("f3"));
        board.MovePiece(Position.Parse("a6"), Position.Parse("a5"));
        board.MovePiece(Position.Parse("f1"), Position.Parse("e2"));
        board.MovePiece(Position.Parse("a5"), Position.Parse("a4"));

        Assert.True(board.IsLegalMove(Position.Parse("e1"), Position.Parse("g1")));
        board.MovePiece(Position.Parse("e1"), Position.Parse("g1"));

        var rook = board.GetPieceAt(Position.Parse("f1"));
        Assert.NotNull(rook);
        Assert.Equal(PieceType.Rook, rook!.Type);
    }

    [Fact]
    public void Pawn_ShouldPromoteToQueen()
    {
        var board = new Board();

        board.Clear();

        board.SetPiece(Position.Parse("e1"), new Piece(PieceColor.White, PieceType.King));
        board.SetPiece(Position.Parse("d8"), new Piece(PieceColor.Black, PieceType.King));
        board.SetPiece(Position.Parse("e7"), new Piece(PieceColor.White, PieceType.Pawn));

        Assert.True(board.IsLegalMove(Position.Parse("e7"), Position.Parse("e8"), PieceType.Queen));
        board.MovePiece(Position.Parse("e7"), Position.Parse("e8"), PieceType.Queen);

        var promotedPiece = board.GetPieceAt(Position.Parse("e8"));
        Assert.NotNull(promotedPiece);
        Assert.Equal(PieceType.Queen, promotedPiece!.Type);
        Assert.Equal(PieceColor.White, promotedPiece.Color);
    }

    [Fact]
    public void IsInCheck_ShouldDetectCheck()
    {
        var board = new Board();

        board.Clear();

        board.SetPiece(Position.Parse("e1"), new Piece(PieceColor.White, PieceType.King));
        board.SetPiece(Position.Parse("e8"), new Piece(PieceColor.Black, PieceType.King));
        board.SetPiece(Position.Parse("e5"), new Piece(PieceColor.White, PieceType.Queen));

        Assert.True(board.IsInCheck(PieceColor.Black));
    }

    [Fact]
    public void IsCheckmate_ShouldDetectCheckmate()
    {
        var board = new Board();

        board.Clear();

        board.SetPiece(Position.Parse("f6"), new Piece(PieceColor.White, PieceType.King));
        board.SetPiece(Position.Parse("h6"), new Piece(PieceColor.White, PieceType.Queen));
        board.SetPiece(Position.Parse("g1"), new Piece(PieceColor.White, PieceType.Rook));
        board.SetPiece(Position.Parse("h8"), new Piece(PieceColor.Black, PieceType.King));

        Assert.True(board.IsInCheck(PieceColor.Black));
        Assert.True(board.IsCheckmate(PieceColor.Black));
    }

    [Fact]
    public void MovePiece_ShouldRelocatePieceOnBoard()
    {
        var board = new Board();
        var from = Position.Parse("e2");
        var to = Position.Parse("e4");

        board.MovePiece(from, to);

        Assert.Null(board.GetPieceAt(from));

        var movedPiece = board.GetPieceAt(to);
        Assert.NotNull(movedPiece);
        Assert.Equal(PieceColor.White, movedPiece!.Color);
        Assert.Equal(PieceType.Pawn, movedPiece.Type);
    }
}
