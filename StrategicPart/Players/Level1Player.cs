using Chess.LogicPart;
using Chess.TreesOfAnalysis;

namespace Chess.Players
{
    public class Level1Player : VirtualPlayer
    {
        public Level1Player()
        { }

        protected override int EvaluatePosition(ChessBoard board)
        {
            var evaluation = EvaluatePositionStatically(board);
            var newEvaluation = CheckExchangeVariants(board);

            if (board.MovingSideColor == ChessPieceColor.White)
            {
                return Math.Max(evaluation, newEvaluation);
            }

            return Math.Min(evaluation, newEvaluation);
        }

        protected override Move SelectMove()
        {
            if (ThinkingDisabled)
            {
                throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
            }

            Tree = new AnalysisTree(Board);
            Tree.Analyze(1, EvaluatePosition);
            var move = Tree.GetBestMove();

            if (ThinkingDisabled)
            {
                throw new GameInterruptedException("Виртуальному игроку запрещен анализ позиций.");
            }

            return move;
        }

        protected override int EvaluatePositionStatically(ChessBoard board)
        {
            if (board == null || board.Status == GameStatus.IllegalPosition || board.Status == GameStatus.ClearBoard)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            if (board.Status != GameStatus.GameIsNotOver)
            {
                return board.Status switch
                {
                    GameStatus.WhiteWin => int.MaxValue,
                    GameStatus.BlackWin => -int.MaxValue,
                    _ => 0
                };
            }

            var result = 0;

            foreach (var piece in board.GetMaterial())
            {
                if (piece.Name != ChessPieceName.King)
                {
                    result += EvaluatePiece(piece);
                }
            }

            return result;
        }

        private int EvaluateExchanges(Square square)
        {
            var attackersColor = square.Board.MovingSideColor;
            var comparer = new AttackersComparer(square);

            var material = new List<ChessPiece>(GetAttackers(square, attackersColor));
            material.Sort(comparer);
            var attackers = new Queue<ChessPiece>(material);

            material = new List<ChessPiece>(GetAttackers(square, attackersColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White));

            if (!square.IsEmpty)
            {
                if (square.ContainedPiece.Color == attackersColor)
                {
                    throw new InvalidOperationException("Невозможно взять свою фигуру.");
                }

                if (square.ContainedPiece.Name == ChessPieceName.King)
                {
                    throw new InvalidOperationException("Невозможно взять короля.");
                }

                material.Add(square.ContainedPiece);
            }

            material.Sort(comparer);
            var defenders = new Queue<ChessPiece>(material);

            var evaluations = new List<int>();
            var currentEvaluation = EvaluatePositionStatically(square.Board);
            var attackersAreToMove = true;

            while (attackers.Count > 0 && defenders.Count > 0)
            {
                int capturedPieceEvaluation;

                if (evaluations.Count == 0 && square.IsEmpty)
                {
                    capturedPieceEvaluation = 0;
                }
                else
                {
                    var capturedPiece = attackersAreToMove ? defenders.Dequeue() : attackers.Dequeue();

                    if (capturedPiece.Name == ChessPieceName.King)
                    {
                        break;
                    }

                    capturedPieceEvaluation = EvaluatePiece(capturedPiece);
                }

                currentEvaluation -= capturedPieceEvaluation;
                evaluations.Add(currentEvaluation);
                attackersAreToMove = !attackersAreToMove;
            }

            if (evaluations.Count == 0)
            {
                return EvaluatePositionStatically(square.Board);
            }

            if (evaluations.Count == 1)
            {
                return evaluations[0];
            }

            for (var i = evaluations.Count - 2; i >= 0; --i)
            {
                if ((i % 2 == 0 && attackersColor == ChessPieceColor.White) || (i % 2 != 0 && attackersColor == ChessPieceColor.Black))
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

        private int CheckExchangeVariants(ChessBoard board)
        {
            if (ThinkingDisabled)
            {
                throw new GameInterruptedException("Анализ позиции прерван.");
            }

            var captureMoves = board.GetLegalMoves().Where(move => move.IsCapture).ToArray();

            if (captureMoves.Length == 0)
            {
                return EvaluatePositionStatically(board);
            }

            int result;

            if (captureMoves.Length > 1)
            {
                var evaluations = captureMoves.Select(move => move.MoveSquare).Distinct().Select(square => EvaluateExchanges(square));
                result = board.MovingSideColor == ChessPieceColor.White ? evaluations.Max() : evaluations.Min();
                return result;
            }

            board.MakeMove(captureMoves[0]);

            try
            {
                result = board.MovingSideColor == ChessPieceColor.White ? Math.Max(EvaluatePositionStatically(board), CheckExchangeVariants(board)) :
                    Math.Min(EvaluatePositionStatically(board), CheckExchangeVariants(board));
            }

            catch (GameInterruptedException exception)
            {
                board.TakebackMove();
                throw new GameInterruptedException(exception.Message);
            }

            board.TakebackMove();
            return result;
        }
    }
}
