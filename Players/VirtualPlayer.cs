using Chess.LogicPart;
using Chess.TreesOfAnalysis;

namespace Chess.Players
{
    public abstract class VirtualPlayer
    {
        protected AnalysisTree Tree { get; set; }

        protected ChessBoard Board { get; private set; }

        public bool ThinkingDisabled { get; private set; }

        public Move SelectMove(ChessBoard board)
        {
            if (board.Status != GameStatus.GameIsNotOver)
            {
                throw new ArgumentException("На доске невозможно сделать ход.");
            }

            if (ThinkingDisabled)
            {
                throw new ApplicationException("Виртуальному игроку запрещен анализ позиций.");
            }

            var modCount = board.ModCount;
            Board = new ChessBoard(board);
            var result = SelectMove();

            if (ThinkingDisabled)
            {
                throw new ApplicationException("Виртуальному игроку запрещен анализ позиций.");
            }

            if (board.ModCount != modCount)
            {
                throw new InvalidOperationException("На доске изменилась позиция во время анализа.");
            }

            var piece = board[result.StartSquare.Vertical, result.StartSquare.Horizontal].ContainedPiece;
            var square = board[result.MoveSquare.Vertical, result.MoveSquare.Horizontal];
            return !result.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, result.NewPiece.Name);
        }

        protected abstract Move SelectMove();

        protected abstract int EvaluatePosition(ChessBoard board);

        protected abstract int EvaluatePositionStatically(ChessBoard board);

        protected static IEnumerable<ChessPiece> GetHorizontalAttackers(Square square, ChessPieceColor color)
        {
            var initialModCountValue = square.Board.ModCount;

            for (var i = square.Vertical + 1; i < 8; ++i)
            {
                if (square.Board[i, square.Horizontal].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, square.Horizontal].ContainedPiece;

                if (piece.Color != color || (piece.Name != ChessPieceName.King && piece.Name != ChessPieceName.Queen && piece.Name != ChessPieceName.Rook))
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i > square.Vertical + 1)
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            for (var i = square.Vertical - 1; i >= 0; --i)
            {
                if (square.Board[i, square.Horizontal].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, square.Horizontal].ContainedPiece;

                if (piece.Color != color || (piece.Name != ChessPieceName.King && piece.Name != ChessPieceName.Queen && piece.Name != ChessPieceName.Rook))
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i < square.Vertical - 1)
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            if (square.Board.ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        protected static IEnumerable<ChessPiece> GetVerticalAttackers(Square square, ChessPieceColor color)
        {
            var initialModCountValue = square.Board.ModCount;

            for (var i = square.Horizontal + 1; i < 8; ++i)
            {
                if (square.Board[square.Vertical, i].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[square.Vertical, i].ContainedPiece;

                if (piece.Color != color || (piece.Name != ChessPieceName.King && piece.Name != ChessPieceName.Queen && piece.Name != ChessPieceName.Rook))
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i > square.Horizontal + 1)
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            for (var i = square.Horizontal - 1; i >= 0; --i)
            {
                if (square.Board[square.Vertical, i].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[square.Vertical, i].ContainedPiece;

                if (piece.Color != color || (piece.Name != ChessPieceName.King && piece.Name != ChessPieceName.Queen && piece.Name != ChessPieceName.Rook))
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i < square.Horizontal - 1)
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            if (square.Board.ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        protected static IEnumerable<ChessPiece> GetDiagonalAttackers(Square square, ChessPieceColor color)
        {
            var initialModCountValue = square.Board.ModCount;

            for (int i = square.Vertical + 1, j = square.Horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                if (square.Board[i, j].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, j].ContainedPiece;

                if (piece.Color != color || piece.Name == ChessPieceName.Rook || piece.Name == ChessPieceName.Knight)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i > square.Vertical + 1)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.Pawn && !piece.Attacks(square))
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = square.Vertical - 1, j = square.Horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                if (square.Board[i, j].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, j].ContainedPiece;

                if (piece.Color != color || piece.Name == ChessPieceName.Rook || piece.Name == ChessPieceName.Knight)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i < square.Vertical - 1)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.Pawn && !piece.Attacks(square))
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = square.Vertical + 1, j = square.Horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                if (square.Board[i, j].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, j].ContainedPiece;

                if (piece.Color != color || piece.Name == ChessPieceName.Rook || piece.Name == ChessPieceName.Knight)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i > square.Vertical + 1)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.Pawn && !piece.Attacks(square))
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = square.Vertical - 1, j = square.Horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                if (square.Board[i, j].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[i, j].ContainedPiece;

                if (piece.Color != color || piece.Name == ChessPieceName.Rook || piece.Name == ChessPieceName.Knight)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.King && i < square.Vertical - 1)
                {
                    break;
                }

                if (piece.Name == ChessPieceName.Pawn && !piece.Attacks(square))
                {
                    break;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return piece;

                if (piece.Name == ChessPieceName.King)
                {
                    break;
                }
            }

            if (square.Board.ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        protected static IEnumerable<Knight> GetAttackingKnights(Square square, ChessPieceColor color)
        {
            var initialModCountValue = square.Board.ModCount;

            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = square.Vertical + horizontalShifts[i];
                var targetHorizontal = square.Horizontal + verticalShifts[i];

                if (targetVertical < 0 || targetHorizontal < 0 || targetVertical >= 8 || targetHorizontal >= 8)
                {
                    continue;
                }

                if (square.Board[targetVertical, targetHorizontal].IsEmpty)
                {
                    continue;
                }

                var piece = square.Board[targetVertical, targetHorizontal].ContainedPiece;

                if (piece.Name != ChessPieceName.Knight || piece.Color != color)
                {
                    continue;
                }

                if (square.Board.ModCount != initialModCountValue)
                {
                    throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                }

                yield return (Knight)piece;
            }

            if (square.Board.ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        protected static IEnumerable<ChessPiece> GetAttackers(Square square, ChessPieceColor color) => GetVerticalAttackers(square, color).
            Concat(GetHorizontalAttackers(square, color)).Concat(GetDiagonalAttackers(square, color)).Concat(GetAttackingKnights(square, color));

        protected virtual int EvaluatePiece(ChessPiece piece)
        {            
            var result = piece.Name switch
            {
                ChessPieceName.Pawn => 10,
                ChessPieceName.Knight => 30,
                ChessPieceName.Bishop => 30,
                ChessPieceName.Rook => 45,
                ChessPieceName.Queen => 90,
                _ => throw new InvalidOperationException("Короля невозможно оценить в баллах.")
            };

            if (piece.Color == ChessPieceColor.Black)
            {
                result = -result;
            }

            return result;
        }

        public void EnableThinking()
        {
            ThinkingDisabled = false;

            if (Tree != null)
            {
                Tree.AnalysisDisabled = false;
            }
        }

        public void DisableThinking()
        {
            ThinkingDisabled = true;

            if (Tree != null)
            {
                Tree.AnalysisDisabled = true;
            }
        }
    }
}
