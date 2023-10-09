using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public static class Tactics
    {
        /*internal static int EvaluateExchanges(this GamePosition position, int vertical, int horizontal, Func<GamePosition, int, int, int> evaluatePiece)
        {
            var attackers = new Queue<int[]>(position.GetSortedAttackers(vertical, horizontal, position.MovingSideColor, evaluatePiece));

            var defendersColor = position.MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;

            var defenders = new Queue<int[]>();
            defenders.Enqueue(new[] { vertical, horizontal });

            foreach (var piece in position.GetSortedAttackers(vertical, horizontal, defendersColor, evaluatePiece))
            {
                defenders.Enqueue(piece);
            }

            var evaluations = new List<int>();
            var currentEvaluation = 0;
            var attackersAreToMove = true;

            while (attackers.Count > 0 && defenders.Count > 0)
            {
                var movingPiece = attackersAreToMove ? attackers.Peek() : defenders.Peek();
                var movingPieceName = position.GetPieceName(movingPiece[0], movingPiece[1]);

                if (movingPieceName == ChessPieceName.King && ((attackersAreToMove && defenders.Count > 1) ||
                    (!attackersAreToMove && attackers.Count > 1)))
                {
                    break;
                }

                var capturedPiece = attackersAreToMove ? defenders.Dequeue() : attackers.Dequeue();
                currentEvaluation += evaluatePiece(position, capturedPiece[0], capturedPiece[1]);
                evaluations.Add(currentEvaluation);
                attackersAreToMove = !attackersAreToMove;
            }

            if (evaluations.Count == 0)
            {
                return 0;
            }

            if (evaluations.Count == 1)
            {
                return evaluations[0];
            }

            for (var i = evaluations.Count - 2; i >= 0; --i)
            {
                if ((i % 2 == 0 && position.MovingSideColor == ChessPieceColor.White) ||
                    (i % 2 != 0 && position.MovingSideColor == ChessPieceColor.Black))
                {
                    if (evaluations[i + 1] < evaluations[i])
                    {
                        evaluations[i] = evaluations[i + 1];
                    }
                }
                else
                {
                    if (evaluations[i + 1] > evaluations[i])
                    {
                        evaluations[i] = evaluations[i + 1];
                    }
                }
            }

            return evaluations[0];
        }

        public IEnumerable<int[]> GetAttackers(int vertical, int horizontal, ChessPieceColor color) =>
            GetVerticalAttackers(vertical, horizontal, color).Concat(GetHorizontalAttackers(vertical, horizontal, color)).
            Concat(GetDiagonalAttackers(vertical, horizontal, color)).Concat(GetAttackingKnights(vertical, horizontal, color));

        public IEnumerable<int[]> GetSortedAttackers(int vertical, int horizontal, ChessPieceColor color,
            Func<GamePosition, int, int, int> evaluatePiece)
        {
            var attackers = GetAttackers(vertical, horizontal, color).ToArray();

            var compareAttackersPriority = new Comparison<int[]>((first, second) =>
            CompareAttackersPriority(vertical, horizontal, first, second, evaluatePiece));

            Array.Sort(attackers, compareAttackersPriority);
            return attackers;
        }

        public IEnumerable<int[]> GetVerticalAttackers(int vertical, int horizontal, ChessPieceColor color)
        {
            for (var i = horizontal + 1; i < 8; ++i)
            {
                var pieceName = _pieceNames[vertical, i];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[vertical, i] != color)
                {
                    break;
                }

                if (pieceName != ChessPieceName.King && pieceName != ChessPieceName.Queen && pieceName != ChessPieceName.Rook)
                {
                    break;
                }

                if (pieceName == ChessPieceName.King && i > horizontal + 1)
                {
                    break;
                }

                yield return new[] { vertical, i };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }

            for (var i = horizontal - 1; i >= 0; --i)
            {
                var pieceName = _pieceNames[vertical, i];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[vertical, i] != color)
                {
                    break;
                }

                if (pieceName != ChessPieceName.King && pieceName != ChessPieceName.Queen && pieceName != ChessPieceName.Rook)
                {
                    break;
                }

                if (pieceName == ChessPieceName.King && i < horizontal - 1)
                {
                    break;
                }

                yield return new[] { vertical, i };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }
        }

        public IEnumerable<int[]> GetHorizontalAttackers(int vertical, int horizontal, ChessPieceColor color)
        {
            for (var i = vertical + 1; i < 8; ++i)
            {
                var pieceName = _pieceNames[i, horizontal];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, horizontal] != color)
                {
                    break;
                }

                if (pieceName != ChessPieceName.King && pieceName != ChessPieceName.Queen && pieceName != ChessPieceName.Rook)
                {
                    break;
                }

                if (pieceName == ChessPieceName.King && i > vertical + 1)
                {
                    break;
                }

                yield return new[] { i, horizontal };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }

            for (var i = vertical - 1; i >= 0; --i)
            {
                var pieceName = _pieceNames[i, horizontal];

                if (pieceName == null)
                {
                    continue;
                }

                if (_pieceColors[i, horizontal] != color)
                {
                    break;
                }

                if (pieceName != ChessPieceName.King && pieceName != ChessPieceName.Queen && pieceName != ChessPieceName.Rook)
                {
                    break;
                }

                if (pieceName == ChessPieceName.King && i < vertical - 1)
                {
                    break;
                }

                yield return new[] { i, horizontal };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }
        }

        public IEnumerable<int[]> GetDiagonalAttackers(int vertical, int horizontal, ChessPieceColor color)
        {
            for (int i = vertical + 1, j = horizontal + 1; i < 8 && j < 8; ++i, ++j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                var pieceColor = _pieceColors[i, j];

                if (pieceColor != color || pieceName == ChessPieceName.Rook || pieceName == ChessPieceName.Knight)
                {
                    break;
                }

                if ((pieceName == ChessPieceName.King || pieceName == ChessPieceName.Pawn) && i > vertical + 1)
                {
                    break;
                }

                if (pieceName == ChessPieceName.Pawn && pieceColor == ChessPieceColor.White)
                {
                    break;
                }

                yield return new[] { i, j };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal - 1; i >= 0 && j >= 0; --i, --j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                var pieceColor = _pieceColors[i, j];

                if (pieceColor != color || pieceName == ChessPieceName.Rook || pieceName == ChessPieceName.Knight)
                {
                    break;
                }

                if ((pieceName == ChessPieceName.King || pieceName == ChessPieceName.Pawn) && i < vertical - 1)
                {
                    break;
                }

                if (pieceName == ChessPieceName.Pawn && pieceColor == ChessPieceColor.Black)
                {
                    break;
                }

                yield return new[] { i, j };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = vertical + 1, j = horizontal - 1; i < 8 && j >= 0; ++i, --j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                var pieceColor = _pieceColors[i, j];

                if (pieceColor != color || pieceName == ChessPieceName.Rook || pieceName == ChessPieceName.Knight)
                {
                    break;
                }

                if ((pieceName == ChessPieceName.King || pieceName == ChessPieceName.Pawn) && i > vertical + 1)
                {
                    break;
                }

                if (pieceName == ChessPieceName.Pawn && pieceColor == ChessPieceColor.Black)
                {
                    break;
                }

                yield return new[] { i, j };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }

            for (int i = vertical - 1, j = horizontal + 1; i >= 0 && j < 8; --i, ++j)
            {
                var pieceName = _pieceNames[i, j];

                if (pieceName == null)
                {
                    continue;
                }

                var pieceColor = _pieceColors[i, j];

                if (pieceColor != color || pieceName == ChessPieceName.Rook || pieceName == ChessPieceName.Knight)
                {
                    break;
                }

                if ((pieceName == ChessPieceName.King || pieceName == ChessPieceName.Pawn) && i < vertical - 1)
                {
                    break;
                }

                if (pieceName == ChessPieceName.Pawn && pieceColor == ChessPieceColor.White)
                {
                    break;
                }

                yield return new[] { i, j };

                if (pieceName == ChessPieceName.King)
                {
                    break;
                }
            }
        }

        public IEnumerable<int[]> GetAttackingKnights(int vertical, int horizontal, ChessPieceColor color)
        {
            var verticalShifts = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
            var horizontalShifts = new int[] { -1, 1, -1, 1, -2, 2, -2, 2 };

            for (var i = 0; i < 8; ++i)
            {
                var targetVertical = vertical + horizontalShifts[i];
                var targetHorizontal = horizontal + verticalShifts[i];

                if (targetVertical < 0 || targetHorizontal < 0 || targetVertical >= 8 || targetHorizontal >= 8)
                {
                    continue;
                }

                if (_pieceNames[targetVertical, targetHorizontal] == ChessPieceName.Knight &&
                    _pieceColors[targetVertical, targetHorizontal] == color)
                {
                    yield return new[] { targetVertical, targetHorizontal };
                }
            }
        }

        private int CompareAttackersPriority(int vertical, int horizontal, int[] firstPiece, int[] secondPiece,
            Func<GamePosition, int, int, int> evaluatePiece)
        {
            var piece1Vertical = firstPiece[0];
            var piece1Horizontal = firstPiece[1];
            var piece1Name = _pieceNames[piece1Vertical, piece1Horizontal];

            var piece2Vertical = secondPiece[0];
            var piece2Horizontal = secondPiece[1];
            var piece2Name = _pieceNames[piece2Vertical, piece2Horizontal];

            if (piece1Name == ChessPieceName.King)
            {
                return 1;
            }

            if (piece2Name == ChessPieceName.King)
            {
                return -1;
            }

            if (HasDiagonalBattery(vertical, horizontal, firstPiece, secondPiece))
            {
                return piece1Vertical > vertical ? piece1Vertical - piece2Vertical :
                piece2Vertical - piece1Vertical;
            }

            if ((piece1Name == ChessPieceName.Queen || piece1Name == ChessPieceName.Rook) &&
            (piece2Name == ChessPieceName.Queen || piece2Name == ChessPieceName.Rook))
            {
                if (piece1Vertical == piece2Vertical && piece2Vertical == vertical)
                {
                    if (piece1Horizontal > horizontal && piece2Horizontal > horizontal)
                    {
                        return piece1Horizontal - piece2Horizontal;
                    }

                    if (piece1Horizontal < horizontal && piece2Horizontal < horizontal)
                    {
                        return piece2Horizontal - piece1Horizontal;
                    }
                }

                if (piece1Horizontal == piece2Horizontal && piece2Horizontal == horizontal)
                {
                    if (piece1Vertical > vertical && piece2Vertical > vertical)
                    {
                        return piece1Vertical - piece2Vertical;
                    }

                    if (piece1Vertical < vertical && piece2Vertical < vertical)
                    {
                        return piece2Vertical - piece1Vertical;
                    }
                }
            }

            var difference = evaluatePiece(this, piece1Vertical, piece1Horizontal) -
                evaluatePiece(this, piece2Vertical, piece2Horizontal);

            if (_pieceColors[piece1Vertical, piece1Horizontal] == ChessPieceColor.Black)
            {
                difference = -difference;
            }

            return difference;
        }

        private bool HasDiagonalBattery(int targetVertical, int targetHorizontal, int[] firstPiece, int[] secondPiece)
        {
            var firstPieceVertical = firstPiece[0];
            var firstPieceHorizontal = firstPiece[1];
            var firstPieceName = _pieceNames[firstPieceVertical, firstPieceHorizontal];

            var secondPieceVertical = secondPiece[0];
            var secondPieceHorizontal = secondPiece[1];
            var secondPieceName = _pieceNames[secondPieceVertical, secondPieceHorizontal];

            if (!(firstPieceName == ChessPieceName.Queen || firstPieceName == ChessPieceName.Bishop || firstPieceName == ChessPieceName.Pawn) ||
               !(secondPieceName == ChessPieceName.Queen || secondPieceName == ChessPieceName.Bishop || secondPieceName == ChessPieceName.Pawn))
            {
                return false;
            }

            if (firstPieceName == ChessPieceName.Pawn && secondPieceName == ChessPieceName.Pawn)
            {
                return false;
            }

            if (firstPieceName == ChessPieceName.Queen)
            {
                if (Math.Abs(firstPieceVertical - targetVertical) != Math.Abs(firstPieceHorizontal - targetHorizontal))
                {
                    return false;
                }
            }

            if (secondPieceName == ChessPieceName.Queen)
            {
                if (Math.Abs(secondPieceVertical - targetVertical) != Math.Abs(secondPieceHorizontal - targetHorizontal))
                {
                    return false;
                }
            }

            if (Math.Abs(firstPieceVertical - secondPieceVertical) != Math.Abs(firstPieceHorizontal - secondPieceHorizontal))
            {
                return false;
            }

            return (firstPieceVertical > targetVertical && secondPieceVertical > targetVertical) ||
            (firstPieceVertical < targetVertical && secondPieceVertical < targetVertical);
        }*/
    }
}
