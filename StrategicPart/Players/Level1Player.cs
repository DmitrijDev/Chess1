using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public class Level1Player : VirtualPlayer
    {
        public Level1Player(ChessBoard board) => Board = board;

        protected override int EvaluatePosition()
        {
            var evaluation = EvaluatePositionStatically();
            var newEvaluation = CheckExchangeVariants();

            if (Board.MovingSideColor == ChessPieceColor.White)
            {
                return Math.Max(evaluation, newEvaluation);
            }

            return Math.Min(evaluation, newEvaluation);
        }

        public override Move SelectMove() => SelectBestMove(Board, MakeFullAnalysis(1));

        protected override int EvaluatePositionStatically()
        {
            if (Board == null || Board.Status == GameStatus.IllegalPosition || Board.Status == GameStatus.ClearBoard)
            {
                throw new ArgumentException("Некорректный аргумент.");
            }

            if (Board.Status != GameStatus.GameIsNotOver)
            {
                return Board.Status switch
                {
                    GameStatus.WhiteWin => int.MaxValue,
                    GameStatus.BlackWin => -int.MaxValue,
                    _ => 0
                };
            }

            var result = 0;

            foreach (var piece in Board.GetMaterial())
            {
                var pieceEvaluation = piece.Name switch
                {
                    ChessPieceName.Pawn => 10,
                    ChessPieceName.Knight => 30,
                    ChessPieceName.Bishop => 30,
                    ChessPieceName.Rook => 45,
                    ChessPieceName.Queen => 90,
                    _ => 0
                };

                if (piece.Color == ChessPieceColor.Black)
                {
                    pieceEvaluation = -pieceEvaluation;
                }

                result += pieceEvaluation;
            }

            return result;
        }

        private int EvaluateExchanges(Square square)
        {
            var attackersColor = Board.MovingSideColor;

            var materialList = new List<ChessPiece>(GetAttackers(square, attackersColor));
            materialList.Sort(new AttackersComparer(square));
            var attackers = new Queue<ChessPiece>(materialList);

            materialList = new(GetAttackers(square, attackersColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White));

            if (!square.IsEmpty)
            {
                materialList.Add(square.ContainedPiece);
            }

            materialList.Sort(new AttackersComparer(square));
            var defenders = new Queue<ChessPiece>(materialList);

            var evaluations = new List<int>() { EvaluatePositionStatically() };
            var currentEvaluation = evaluations[0];
            var attackersAreToMove = true;

            while (attackers.Count > 0 && defenders.Count > 0)
            {
                var piece = attackersAreToMove ? defenders.Dequeue() : attackers.Dequeue();

                var pieceEvaluation = piece.Name switch
                {
                    ChessPieceName.Pawn => 10,
                    ChessPieceName.Knight => 30,
                    ChessPieceName.Bishop => 30,
                    ChessPieceName.Rook => 45,
                    ChessPieceName.Queen => 90,
                    _ => 0
                };

                if (piece.Color == ChessPieceColor.White)
                {
                    pieceEvaluation = -pieceEvaluation;
                }

                currentEvaluation = currentEvaluation + pieceEvaluation;
                evaluations.Add(currentEvaluation);
                attackersAreToMove = !attackersAreToMove;
            }

            if (evaluations.Count <= 2)
            {
                return evaluations[evaluations.Count - 1];
            }

            for (var i = evaluations.Count - 2; i >= 1; --i)
            {
                if ((i % 2 != 0 && attackersColor == ChessPieceColor.White) || (i % 2 == 0 && attackersColor == ChessPieceColor.Black))
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

            return evaluations[1];
        }

        private int CheckExchangeVariants()
        {
            if (ThinkingDisabled)
            {
                throw new GameInterruptedException();
            }

            var captureMoves = new List<Move>(Board.GetLegalMoves().Where(move => move.IsCapture));

            if (captureMoves.Count == 0)
            {
                return EvaluatePositionStatically();
            }

            int newEvaluation;

            if (captureMoves.Count > 1)
            {
                newEvaluation = Board.MovingSideColor == ChessPieceColor.White ? captureMoves.Select(move => EvaluateExchanges(move.MoveSquare)).Max() :
                captureMoves.Select(move => EvaluateExchanges(move.MoveSquare)).Min();
                return newEvaluation;
            }

            Board.MakeMove(captureMoves[0]);

            try
            {
                newEvaluation = Board.MovingSideColor == ChessPieceColor.White ? Math.Max(EvaluatePositionStatically(), CheckExchangeVariants()) :
                    Math.Min(EvaluatePositionStatically(), CheckExchangeVariants());
            }

            catch (GameInterruptedException)
            {
                Board.TakebackMove();
                throw new GameInterruptedException();
            }

            Board.TakebackMove();
            return newEvaluation;
        }
    }
}
