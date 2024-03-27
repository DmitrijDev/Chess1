
namespace Chess.LogicPart
{
    public sealed class King : ChessPiece
    {
        public King(ChessPieceColor color) : base(color)
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

            for (var i = vertical > 0 ? vertical - 1 : 0; i <= vertical + 1 && i < 8; ++i)
            {
                for (var j = horizontal > 0 ? horizontal - 1 : 0; j <= horizontal + 1 && j < 8; ++j)
                {
                    if (i == vertical && j == horizontal)
                    {
                        continue;
                    }

                    if (board.ModCount != modCount || board.GameStartsCount != gameStartsCount)
                    {
                        throw new InvalidOperationException("Изменение позиции во время перечисления.");
                    }

                    yield return board[i, j];
                }
            }
        }

        internal override IEnumerable<Square> GetAccessibleSquares()
        {
            var result = base.GetAccessibleSquares();

            if (Vertical != 4)
            {
                return result;
            }

            if (!(Color == ChessPieceColor.White && Horizontal == 0) &&
                !(Color == ChessPieceColor.Black && Horizontal == 7))
            {
                return result;
            }

            if (FirstMoveMoment > 0 || IsChecked())
            {
                return result;
            }

            if (CanCastleKingside())
            {
                result = result.Append(Board[6, Horizontal]);
            }

            if (CanCastleQueenside())
            {
                result = result.Append(Board[2, Horizontal]);
            }

            return result;
        }

        private bool CanCastleKingside()
        {
            var rookPosition = Board[7, Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                rookPosition.ContainedPiece.Color != Color)
            {
                return false;
            }

            if (!Board[5, Horizontal].IsEmpty || !Board[6, Horizontal].IsEmpty)
            {
                return false;
            }

            if (rookPosition.ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            return !Board[5, Horizontal].IsMenacedBy(enemyColor) && !Board[6, Horizontal].IsMenacedBy(enemyColor);
        }

        private bool CanCastleQueenside()
        {
            var rookPosition = Board[0, Horizontal];

            if (rookPosition.IsEmpty || rookPosition.ContainedPiece.Name != ChessPieceName.Rook ||
                rookPosition.ContainedPiece.Color != Color)
            {
                return false;
            }

            if (!Board[1, Horizontal].IsEmpty || !Board[2, Horizontal].IsEmpty || !Board[3, Horizontal].IsEmpty)
            {
                return false;
            }

            if (rookPosition.ContainedPiece.FirstMoveMoment > 0)
            {
                return false;
            }

            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            return !Board[3, Horizontal].IsMenacedBy(enemyColor) && !Board[2, Horizontal].IsMenacedBy(enemyColor);
        }

        public bool IsChecked()
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            var position = Position;
            return position != null && position.IsMenacedBy(enemyColor);
        }

        internal IEnumerable<ChessPiece> GetCheckingPieces()
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            return Position.GetMenaces(enemyColor);
        }

        private protected override IEnumerable<Square> FilterSafeForKingMoves(IEnumerable<Square> moveSquares)
        {
            var checkingPieces = GetCheckingPieces().ToArray();
            return moveSquares.Where(square => CanSafelyMoveTo(square, checkingPieces));
        }            

        private bool CanSafelyMoveTo(Square square, ChessPiece[] checkingPieces)
        {
            var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            if (square.IsMenacedBy(enemyColor))
            {
                return false;
            }

            foreach (var menace in checkingPieces)
            {
                if (menace.Vertical == Vertical)
                {
                    if (square.Vertical == Vertical && square != menace.Position)
                    {
                        return false;
                    }
                }
                else if (menace.Horizontal == Horizontal)
                {
                    if (square.Horizontal == Horizontal && square != menace.Position)
                    {
                        return false;
                    }
                }
                else if (IsOnSameDiagonal(menace) && menace.IsLongRanged)
                {
                    if (square.IsOnSameDiagonal(menace) && square != menace.Position)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal override bool CanMove() => base.GetAccessibleSquares().Any();

        internal override void CheckLegacy(Move move)
        {
            if (move.IsCastleKingside)
            {
                if (FirstMoveMoment > 0)
                {
                    throw new IllegalMoveException("Рокировка невозможна: король уже сделал ход.");
                }

                if (Board[7, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    throw new IllegalMoveException("Рокировка невозможна: ладья уже сделала ход.");
                }

                if (IsChecked())
                {
                    throw new IllegalMoveException("Рокировка невозможна: король под шахом.");
                }

                var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

                if (Board[5, Horizontal].IsMenacedBy(enemyColor))
                {
                    throw new IllegalMoveException("При рокировке король не может пересекать угрожаемое поле.");
                }

                if (move.MoveSquare.IsMenacedBy(enemyColor))
                {
                    throw new IllegalMoveException("Король не может становиться под шах.");
                }

                return;
            }

            if (move.IsCastleQueenside)
            {
                if (FirstMoveMoment > 0)
                {
                    throw new IllegalMoveException("Рокировка невозможна: король уже сделал ход.");
                }

                if (Board[0, Horizontal].ContainedPiece.FirstMoveMoment > 0)
                {
                    throw new IllegalMoveException("Рокировка невозможна: ладья уже сделала ход.");
                }

                if (IsChecked())
                {
                    throw new IllegalMoveException("Рокировка невозможна: король под шахом.");
                }

                var enemyColor = Color == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

                if (Board[3, Horizontal].IsMenacedBy(enemyColor))
                {
                    throw new IllegalMoveException("При рокировке король не может пересекать угрожаемое поле.");
                }

                if (move.MoveSquare.IsMenacedBy(enemyColor))
                {
                    throw new IllegalMoveException("Король не может становиться под шах.");
                }

                return;
            }

            if (!Attacks(move.MoveSquare))
            {
                throw new IllegalMoveException("Невозможный ход.");
            }

            var checkingPieces = GetCheckingPieces().ToArray();

            if (!CanSafelyMoveTo(move.MoveSquare, checkingPieces))
            {
                var message = IsChecked() ? "Невозможный ход. Король не может оставаться под шахом." : "Король не может становиться под шах.";
                throw new IllegalMoveException(message);
            }
        }

        public override ChessPieceName Name => ChessPieceName.King;

        public override bool IsLongRanged => false;
    }
}