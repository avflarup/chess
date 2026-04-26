# Chess Game

A full-featured chess implementation in C# with a cross-platform GUI built using Avalonia. This project was largely generated with the assistance of Microsoft Copilot. I guided the prompts, reviewed the outputs, and made adjustments where needed.

## Features

- **Complete Chess Rules**: Full implementation of standard chess rules including:
  - Pawn movement and promotion
  - Knight, Bishop, Rook, Queen, and King moves
  - Castling (kingside and queenside)
  - En passant captures
  - Check and checkmate detection
  - Proper turn alternation

- **Interactive GUI**: Built with Avalonia for cross-platform desktop support (Windows, Linux, macOS)
  - Click-to-move interface
  - Board rotation for player perspective switching
  - Real-time game status updates
  - Checkmate and stalemate detection with end game screen
  - New game reset functionality

- **Comprehensive Testing**: 15 unit tests covering all major chess rules and edge cases

## Project Structure

```
Chess/
├── Chess.Core/              # Chess engine and rules (class library)
│   ├── Board.cs            # Game board state and move validation
│   ├── Piece.cs            # Piece model and enums
│   └── Position.cs         # Board position (file, rank)
├── Chess.UI/               # Desktop GUI (Avalonia)
│   ├── MainWindow.axaml    # UI layout
│   ├── MainWindow.axaml.cs # UI logic and game flow
│   ├── App.axaml           # Application configuration
│   └── Program.cs          # Entry point
├── Chess.Tests/            # Unit tests (xUnit)
│   └── UnitTest1.cs        # Board and move validation tests
└── Chess.sln              # Solution file
```

## Building

### Prerequisites

- .NET 8.0 SDK or later
- Cross-platform (Windows, Linux, macOS)

### Build from Command Line

```bash
dotnet build
```

Build a specific project:

```bash
dotnet build Chess.UI/Chess.UI.csproj
```

## Running

### Start the Chess Game

```bash
dotnet run --project Chess.UI
```

The application will launch with an interactive chess board. Use the GUI to:
1. Click a piece to select it
2. Click a destination square to move
3. Use "Rotate Board" to flip perspective
4. Use "New Game" to reset

### Run Tests

```bash
dotnet test
```

Run tests for a specific project:

```bash
dotnet test Chess.Tests/Chess.Tests.csproj
```

## Architecture

### Chess.Core

The `Board` class manages the game state and validates all legal moves:

- **MovePiece(from, to, promotion?)**: Execute a move after validation
- **IsLegalMove(from, to)**: Check if a move is legal
- **IsInCheck(color)**: Detect check conditions
- **IsCheckmate(color)**: Detect checkmate conditions
- **GetPieceAt(position)**: Retrieve piece at a board position

The `Piece` class represents individual chess pieces with immutable design:
- `PieceColor` enum: White or Black
- `PieceType` enum: Pawn, Knight, Bishop, Rook, Queen, King
- `HasMoved`: Track castling and en passant eligibility

The `Position` class validates and represents board squares:
- File (a-h) and Rank (1-8)
- Built-in validation and parsing

### Chess.UI

The `MainWindow` class provides the interactive board and game flow:

- **UpdateBoard()**: Render current game state
- **OnSquareClicked()**: Handle player moves
- **ShowVictoryOverlay()**: Display checkmate screen
- **RotateBoardButton_Click()**: Flip board perspective

UI controls are bound at initialization for optimal performance and cleaner code.

## Game Rules Implemented

### Standard Moves

- **Pawns**: One square forward (two on first move), capture diagonally
- **Knights**: L-shaped moves (2+1 or 1+2 squares)
- **Bishops**: Diagonal movement any distance (unobstructed)
- **Rooks**: Horizontal/vertical movement any distance (unobstructed)
- **Queens**: Combination of Rook and Bishop moves
- **Kings**: One square in any direction

### Special Moves

- **Castling**: King and Rook move simultaneously (kingside and queenside)
  - Valid only if neither piece has moved
  - King not in check
  - Path is clear and not attacked
  
- **En Passant**: Pawn captures opponent's pawn after its double-step advance
  - Must be captured immediately on the next move

- **Promotion**: Pawn reaching the opposite end promotes to Queen, Rook, Bishop, or Knight
  - Defaults to Queen if not specified

### Game States

- **Check**: King is under attack; must move out of check
- **Checkmate**: King is in check with no legal moves; game ends
- **Stalemate**: Player has no legal moves but is not in check

## Code Quality

- **Type-safe**: Strong use of C# enums and custom types
- **Immutable Design**: Pieces and positions are immutable for thread safety
- **Board Validation**: All moves validated before execution
- **Test Coverage**: 15 comprehensive unit tests covering rules and edge cases

## Future Enhancements

- Move history and PGN export
- AI opponent
- Network multiplayer
- Move undo/redo
- FEN position loading

## License

Open source - feel free to use and modify.
