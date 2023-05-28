using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public abstract class VirtualPlayer
    {
        public ChessBoard Board { get; protected set; }

        public bool ThinkingDisabled { get; set; }

        public abstract Move SelectMove();

        protected abstract int EvaluatePosition();

        protected abstract int EvaluatePositionStatically();

        internal PositionTree MakeFullAnalysis(int depth)
        {
            if (depth < 1)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            var friendlySideColor = Board.MovingSideColor;
            var treeRoot = Board.MovesCount == 0 ? new PositionTree() : new PositionTree(Board.GetLastMove());
            treeRoot.AddChidren(Board);

            var nodesUnderAnalysis = new Stack<PositionTree>();
            nodesUnderAnalysis.Push(treeRoot);

            var queues = new Stack<Queue<PositionTree>>();
            queues.Push(new Queue<PositionTree>(treeRoot.Children));

            while (queues.Peek().Count > 0 || nodesUnderAnalysis.Count > 1)
            {
                if (ThinkingDisabled)
                {
                    for (var i = nodesUnderAnalysis.Count; i > 1; --i)
                    {
                        Board.TakebackMove();
                    }

                    throw new GameInterruptedException();
                }

                if (queues.Peek().Count == 0)
                {
                    Board.TakebackMove();
                    nodesUnderAnalysis.Pop();
                    queues.Pop();
                    continue;
                }

                var currentNode = queues.Peek().Dequeue();
                var piece = Board[currentNode.StartSquareVertical, currentNode.StartSquareHorizontal].ContainedPiece;
                var square = Board[currentNode.MoveSquareVertical, currentNode.MoveSquareHorizontal];
                var move = currentNode.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)currentNode.NewPieceName);
                Board.MakeMove(move);
                nodesUnderAnalysis.Push(currentNode);

                if (nodesUnderAnalysis.Count <= depth)
                {
                    currentNode.AddChidren(Board);
                    queues.Push(new Queue<PositionTree>(currentNode.Children));
                }
                else
                {
                    queues.Push(new Queue<PositionTree>());
                    currentNode.Evaluation = EvaluatePosition();
                    CorrectParentsEvaluations(nodesUnderAnalysis, friendlySideColor);
                }
            }

            return treeRoot;
        }

        internal static Move SelectBestMove(ChessBoard board, PositionTree tree)
        {
            var movesEvaluations = tree.Children.Where(child => child.IsEvaluated).Select(child => child.Evaluation);
            var bestEvaluation = board.MovingSideColor == ChessPieceColor.White ? movesEvaluations.Max() : movesEvaluations.Min();
            var bestMoves = tree.Children.Where(child => child.IsEvaluated && child.Evaluation == bestEvaluation).ToArray();
            PositionTree resultNode;

            if (bestMoves.Length == 0)
            {
                throw new InvalidOperationException("Дерево не содержит оцененных ходов.");
            }

            if (bestMoves.Length == 1)
            {
                resultNode = bestMoves.Single();
            }
            else
            {
                var index = new Random().Next(bestMoves.Length);
                resultNode = bestMoves[index];
            }

            try
            {
                var piece = board[resultNode.StartSquareVertical, resultNode.StartSquareHorizontal].ContainedPiece;
                var square = board[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
                return resultNode.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)resultNode.NewPieceName);
            }

            catch
            {
                throw new ArgumentException("Некорректный аргумент: в дереве встречаются невозможные на данной доске ходы.");
            }
        }

        private static void CorrectParentsEvaluations(Stack<PositionTree> nodesUnderAnalysis, ChessPieceColor friendlySideColor)
        {
            var positionEvaluation = nodesUnderAnalysis.Peek().Evaluation;
            var whiteIsToMove = (friendlySideColor == ChessPieceColor.White && nodesUnderAnalysis.Count % 2 != 0) ||
                (friendlySideColor == ChessPieceColor.Black && nodesUnderAnalysis.Count % 2 == 0);

            foreach (var parentNode in nodesUnderAnalysis.Skip(1))
            {
                whiteIsToMove = !whiteIsToMove;

                if (!parentNode.IsEvaluated)
                {
                    parentNode.Evaluation = positionEvaluation;
                    continue;
                }

                if (parentNode.Evaluation == positionEvaluation)
                {
                    break;
                }

                if ((whiteIsToMove && positionEvaluation > parentNode.Evaluation) || (!whiteIsToMove && positionEvaluation < parentNode.Evaluation))
                {
                    parentNode.Evaluation = positionEvaluation;
                    continue;
                }

                var parentEvaluation = whiteIsToMove ? parentNode.Children.Where(child => child.IsEvaluated).Select(child => child.Evaluation).Max() :
                    parentNode.Children.Where(child => child.IsEvaluated).Select(child => child.Evaluation).Min();

                if (parentNode.Evaluation == parentEvaluation)
                {
                    break;
                }
                else
                {
                    parentNode.Evaluation = parentEvaluation;
                }
            }
        }

        protected static IEnumerable<ChessPiece> GetAttackers(Square square, ChessPieceColor color)
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

                if (piece.Name == ChessPieceName.King || piece.Name == ChessPieceName.Pawn)
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

                if (piece.Name == ChessPieceName.King || piece.Name == ChessPieceName.Pawn)
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

                if (piece.Name == ChessPieceName.King || piece.Name == ChessPieceName.Pawn)
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

                if (piece.Name == ChessPieceName.King || piece.Name == ChessPieceName.Pawn)
                {
                    break;
                }
            }

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

                yield return piece;
            }

            if (square.Board.ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }
    }
}
