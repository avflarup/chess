using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Chess.Core;

namespace Chess.UI;

public partial class MainWindow : Window
{
    private readonly Button[,] _squareButtons = new Button[8, 8];
    private readonly Board _board = new();
    private Position? _selectedPosition;
    private bool _isBoardRotated;
    private bool _isGameOver;
    private Border? _victoryOverlay;
    private TextBlock? _victoryMessageTextBlock;
    private TextBlock? _statusTextBlock;

    public MainWindow()
    {
        InitializeComponent();
        BindControls();
        BuildBoardGrid();
        ResetGame();
    }

    private void BindControls()
    {
        _statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
        _victoryOverlay = this.FindControl<Border>("VictoryOverlay");
        _victoryMessageTextBlock = this.FindControl<TextBlock>("VictoryMessageTextBlock");
    }

    private void BuildBoardGrid()
    {
        var boardGrid = this.FindControl<UniformGrid>("BoardGrid");
        if (boardGrid is null)
            return;

        for (var rank = 8; rank >= 1; rank--)
        {
            for (var fileIndex = 0; fileIndex < 8; fileIndex++)
            {
                var file = (char)('a' + fileIndex);
                var position = new Position(file, rank);
                var button = new Button
                {
                    Tag = position,
                    FontSize = 42,
                    Width = 80,
                    Height = 80,
                    Background = GetSquareBackground(position),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Avalonia.Thickness(1),
                };

                button.Click += OnSquareClicked;
                _squareButtons[fileIndex, 8 - rank] = button;
                boardGrid.Children.Add(button);
            }
        }
    }

    private void ResetGame()
    {
        _board.InitializeStandardBoard();
        _selectedPosition = null;
        _isGameOver = false;
        HideVictoryOverlay();
        UpdateBoard();
        SetStatus("White to move");
    }

    private void UpdateBoard()
    {
        for (var rank = 8; rank >= 1; rank--)
        {
            for (var fileIndex = 0; fileIndex < 8; fileIndex++)
            {
                var displayPosition = new Position((char)('a' + fileIndex), rank);
                var boardPosition = GetBoardPosition(displayPosition);
                var button = _squareButtons[fileIndex, 8 - rank];
                RenderSquare(button, boardPosition);
            }
        }
    }

    private void RenderSquare(Button button, Position boardPosition)
    {
        var piece = _board.GetPieceAt(boardPosition);
        button.Content = piece is null ? string.Empty : CreatePieceTextBlock(piece);
        button.Background = _selectedPosition is not null && _selectedPosition.Equals(boardPosition)
            ? Brushes.Yellow
            : GetSquareBackground(boardPosition);
    }

    private TextBlock CreatePieceTextBlock(Piece piece)
    {
        return new TextBlock
        {
            Text = GetPieceSymbol(piece),
            Foreground = GetPieceForeground(piece.Color),
            Background = Brushes.Transparent,
            FontSize = 42,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
    }

    private Position GetBoardPosition(Position displayPosition)
    {
        if (!_isBoardRotated)
            return displayPosition;

        return new Position((char)('h' - (displayPosition.File - 'a')), 9 - displayPosition.Rank);
    }

    private void OnSquareClicked(object? sender, RoutedEventArgs e)
    {
        if (_isGameOver)
        {
            SetStatus("Game over. Start a new game to continue.");
            return;
        }

        if (sender is not Button button)
            return;

        if (button.Tag is not Position displayPosition)
            return;

        var position = GetBoardPosition(displayPosition);
        var piece = _board.GetPieceAt(position);

        if (_selectedPosition is null)
        {
            if (piece is null)
            {
                SetStatus("Select a piece to move.");
                return;
            }

            if (piece.Color != _board.CurrentTurn)
            {
                SetStatus($"It is {_board.CurrentTurn}'s turn.");
                return;
            }

            _selectedPosition = position;
            UpdateBoard();
            SetStatus($"Selected {piece.Type} at {position}. Choose destination.");
            return;
        }

        if (_selectedPosition.Equals(position))
        {
            _selectedPosition = null;
            UpdateBoard();
            SetStatus($"Selection cleared. {_board.CurrentTurn} to move.");
            return;
        }

        try
        {
            _board.MovePiece(_selectedPosition, position);
            _selectedPosition = null;
            UpdateBoard();

            if (_board.IsCheckmate(_board.CurrentTurn))
            {
                ShowVictoryOverlay(GetOpponent(_board.CurrentTurn));
            }
            else
            {
                SetStatus(_board.IsInCheck(_board.CurrentTurn) ? $"{_board.CurrentTurn} is in check." : $"{_board.CurrentTurn} to move.");
            }
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        ResetGame();
    }

    private void RotateBoardButton_Click(object? sender, RoutedEventArgs e)
    {
        _isBoardRotated = !_isBoardRotated;
        UpdateBoard();
        SetStatus($"Board rotated. {_board.CurrentTurn} to move.");
    }

    private void ShowVictoryOverlay(PieceColor winner)
    {
        _isGameOver = true;

        if (_victoryOverlay is not null)
            _victoryOverlay.IsVisible = true;

        if (_victoryMessageTextBlock is not null)
            _victoryMessageTextBlock.Text = winner == PieceColor.White ? "White wins by checkmate!" : "Black wins by checkmate!";

        SetStatus("Checkmate!");
    }

    private void HideVictoryOverlay()
    {
        if (_victoryOverlay is not null)
            _victoryOverlay.IsVisible = false;
    }

    private static PieceColor GetOpponent(PieceColor color)
    {
        return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private IBrush GetPieceForeground(PieceColor color)
    {
        return color == PieceColor.White ? Brushes.White : Brushes.Black;
    }

    private static string GetPieceSymbol(Piece piece)
    {
        return piece.Color switch
        {
            PieceColor.White => piece.Type switch
            {
                PieceType.Pawn => "♙",
                PieceType.Knight => "♘",
                PieceType.Bishop => "♗",
                PieceType.Rook => "♖",
                PieceType.Queen => "♕",
                PieceType.King => "♔",
                _ => string.Empty,
            },
            PieceColor.Black => piece.Type switch
            {
                PieceType.Pawn => "♟",
                PieceType.Knight => "♞",
                PieceType.Bishop => "♝",
                PieceType.Rook => "♜",
                PieceType.Queen => "♛",
                PieceType.King => "♚",
                _ => string.Empty,
            },
            _ => string.Empty,
        };
    }

    private IBrush GetSquareBackground(Position position)
    {
        var isLightSquare = (position.File - 'a' + position.Rank) % 2 == 0;
        return isLightSquare ? Brushes.Bisque : Brushes.SaddleBrown;
    }

    private void SetStatus(string text)
    {
        if (_statusTextBlock is not null)
        {
            _statusTextBlock.Text = text;
        }
    }
}