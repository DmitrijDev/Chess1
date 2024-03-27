
namespace Chess.LogicPart
{
    public sealed class Pawn : ChessPiece
    {
        public Pawn(ChessPieceColor color) : base(color)
        { }

        public override IEnumerable<Square> GetAttackedSquares()
        {
            var position = Position;

            if (position == null)
            {
                yield break;
            }

            var board = position.Board;
            var vertical = position.Vertical;
            var horizontal = position.Horizontal;

            ulong modCount;
            ulong gameStartsCount;

            lock (board)
            {
                modCount = board.ModCount;
                gameStartsCount = board.GameStartsCount;
            }

            if (position != Position)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления.");
            }

            if ((Color == ChessPieceColor.White && horizontal == 7) ||
                (Color == ChessPieceColor.Black && horizontal == 0))
            {
                yield break;
            }

            if (vertical > 0)
            {
                var square = board[vertical - 1, Color == ChessPieceColor.White ? horizontal + 1 : horizontal - 1];

                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;
            }

            if (vertical < 7)
            {
                var square = board[vertical + 1, Color == ChessPieceColor.White ? horizontal + 1 : horizontal - 1];

                if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                {
                    throw new InvalidOperationException("Изменение позиции во время перечисления.");
                }

                yield return square;
            }
        }

        internal override IEnumerable<Square> GetAccessibleSquares()
        {
            var moveSquares = GetAttackedSquares().
             Where(square => (!square.IsEmpty && square.ContainedPiece.Color != Color) || square == Board.PassedByPawnSquare);

            if (Color == ChessPieceColor.White)
            {
                if (Board[Vertical, Horizontal + 1].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, Horizontal + 1]);
                }

                if (Horizontal == 1 && Board[Vertical, 2].IsEmpty && Board[Vertical, 3].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, 3]);
                }
            }
            else
            {
                if (Board[Vertical, Horizontal - 1].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, Horizontal - 1]);
                }

                if (Horizontal == 6 && Board[Vertical, 5].IsEmpty && Board[Vertical, 4].IsEmpty)
                {
                    moveSquares = moveSquares.Append(Board[Vertical, 4]);
                }
            }

            return FilterSafeForKingMoves(moveSquares);
        }

        internal override void CheckLegacy(Move move)
        {
            if (Attacks(move.MoveSquare))
            {
                if (move.MoveSquare.IsEmpty && move.MoveSquare != Board.PassedByPawnSquare)
                {
                    throw new IllegalMoveException("Невозможный ход.");
                }
            }
            else
            {
                if (!move.MoveSquare.IsEmpty || move.MoveSquare.Vertical != Vertical)
                {
                    throw new IllegalMoveException("Невозможный ход.");
                }

                if (Color == ChessPieceColor.White)
                {
                    if (move.MoveSquare.Horizontal < Horizontal || move.MoveSquare.Horizontal > Horizontal + 2)
                    {
                        throw new IllegalMoveException("Невозможный ход.");
                    }

                    if (move.MoveSquare.Horizontal == Horizontal + 2 && !(Horizontal == 1 && Board[Vertical, 2].IsEmpty))
                    {
                        throw new IllegalMoveException("Невозможный ход.");
                    }
                }
                else
                {
                    if (move.MoveSquare.Horizontal > Horizontal || move.MoveSquare.Horizontal < Horizontal - 2)
                    {
                        throw new IllegalMoveException("Невозможный ход.");
                    }

                    if (move.MoveSquare.Horizontal == Horizontal - 2 && !(Horizontal == 6 && Board[Vertical, 5].IsEmpty))
                    {
                        throw new IllegalMoveException("Невозможный ход.");
                    }
                }
            }

            if (IsPinnedVertically())
            {
                if (move.MoveSquare.Vertical != Vertical)
                {
                    throw new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }
            else if (IsPinnedHorizontally())
            {
                throw new IllegalMoveException("Невозможный ход. Фигура связана.");
            }
            else if (IsPinnedDiagonally())
            {
                if (!IsOnSameDiagonal(move.MoveSquare) || !FriendlyKing.IsOnSameDiagonal(move.MoveSquare))
                {
                    throw new IllegalMoveException("Невозможный ход. Фигура связана.");
                }
            }

            var checkingPieces = FriendlyKing.GetCheckingPieces().ToArray();

            if (checkingPieces.Length > 1)
            {
                throw new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }

            if (checkingPieces.Length == 1 && !ProtectsKingByMoveTo(move.MoveSquare, checkingPieces[0]))
            {
                throw new IllegalMoveException("Невозможный ход. Ваш король под шахом.");
            }

            if (move.IsPawnPromotion && !move.NewPieceSelected)
            {
                throw new NewPieceNotSelectedException();
            }
        }

        public override ChessPieceName Name => ChessPieceName.Pawn;

        public override bool IsLongRanged => false;
    }
}
