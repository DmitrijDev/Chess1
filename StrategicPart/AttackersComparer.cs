using Chess.LogicPart;

namespace Chess.StrategicPart
{
    internal class AttackersComparer : IComparer<ChessPiece>
    {
        private readonly Square _targetSquare;

        public AttackersComparer(Square targetSquare) => _targetSquare = targetSquare;

        public int Compare(ChessPiece firstPiece, ChessPiece secondPiece)
        {
            if (firstPiece.Position == _targetSquare)
            {
                return -1;
            }

            if (secondPiece.Position == _targetSquare)
            {
                return 1;
            }

            if (firstPiece.Name == ChessPieceName.King)
            {
                return 1;
            }

            if (secondPiece.Name == ChessPieceName.King)
            {
                return -1;
            }

            if ((firstPiece.Name == ChessPieceName.Queen || firstPiece.Name == ChessPieceName.Rook) &&
                (secondPiece.Name == ChessPieceName.Queen || secondPiece.Name == ChessPieceName.Rook))
            {
                if (firstPiece.Vertical == secondPiece.Vertical && secondPiece.Vertical == _targetSquare.Vertical)
                {
                    if (firstPiece.Horizontal < _targetSquare.Horizontal && secondPiece.Horizontal < _targetSquare.Horizontal)
                    {
                        return firstPiece.Horizontal > secondPiece.Horizontal ? -1 : 1;
                    }

                    if (firstPiece.Horizontal > _targetSquare.Horizontal && secondPiece.Horizontal > _targetSquare.Horizontal)
                    {
                        return firstPiece.Horizontal < secondPiece.Horizontal ? -1 : 1;
                    }
                }

                if (firstPiece.Horizontal == secondPiece.Horizontal && secondPiece.Horizontal == _targetSquare.Horizontal)
                {
                    if (firstPiece.Vertical < _targetSquare.Vertical && secondPiece.Vertical < _targetSquare.Vertical)
                    {
                        return firstPiece.Vertical > secondPiece.Vertical ? -1 : 1;
                    }

                    if (firstPiece.Vertical > _targetSquare.Vertical && secondPiece.Vertical > _targetSquare.Vertical)
                    {
                        return firstPiece.Vertical < secondPiece.Vertical ? -1 : 1;
                    }
                }
            }

            if ((firstPiece.Name == ChessPieceName.Queen || firstPiece.Name == ChessPieceName.Bishop) &&
               (secondPiece.Name == ChessPieceName.Queen || secondPiece.Name == ChessPieceName.Bishop))
            {
                if (firstPiece.IsOnSameDiagonal(secondPiece) && _targetSquare.IsOnSameDiagonal(firstPiece)
                    && _targetSquare.IsOnSameDiagonal(secondPiece))
                {
                    if (firstPiece.Vertical > _targetSquare.Vertical && secondPiece.Vertical > _targetSquare.Vertical)
                    {
                        if ((firstPiece.Horizontal > _targetSquare.Horizontal && secondPiece.Horizontal > _targetSquare.Horizontal)
                            || (firstPiece.Horizontal < _targetSquare.Horizontal && secondPiece.Horizontal < _targetSquare.Horizontal))
                        {
                            return firstPiece.Vertical < secondPiece.Vertical ? -1 : 1;
                        }
                    }

                    if (firstPiece.Vertical < _targetSquare.Vertical && secondPiece.Vertical < _targetSquare.Vertical)
                    {
                        if ((firstPiece.Horizontal > _targetSquare.Horizontal && secondPiece.Horizontal > _targetSquare.Horizontal)
                            || (firstPiece.Horizontal < _targetSquare.Horizontal && secondPiece.Horizontal < _targetSquare.Horizontal))
                        {
                            return firstPiece.Vertical > secondPiece.Vertical ? -1 : 1;
                        }
                    }
                }
            }

            if (firstPiece.Name == secondPiece.Name)
            {
                return 0;
            }

            if ((firstPiece.Name == ChessPieceName.Knight || firstPiece.Name == ChessPieceName.Bishop) &&
                (secondPiece.Name == ChessPieceName.Knight || secondPiece.Name == ChessPieceName.Bishop))
            {
                return 0;
            }

            return firstPiece.Name > secondPiece.Name ? -1 : 1;
        }
    }
}
