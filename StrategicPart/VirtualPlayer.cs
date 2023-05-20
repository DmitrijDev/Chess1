using Chess.LogicPart;

namespace Chess.StrategicPart
{
    public class VirtualPlayer
    {
        internal Action<PositionTree> Analyze { get; }

        public Func<ChessBoard, short> EvaluatePosition { get; }

        internal VirtualPlayer(Action<PositionTree> analyze, Func<ChessBoard, short> evaluatePosition)
        {
            Analyze = analyze;
            EvaluatePosition = evaluatePosition;
        }

        public Move SelectMove(ChessBoard board)
        {
            var tree = new PositionTree(board, this);
            Analyze(tree);
            tree.RestoreInitialPosition();

            var legalMoves = tree.Root.Children;
            var bestEvaluation = board.MovingSideColor == ChessPieceColor.White ? legalMoves.Select(node => node.Evaluation).Max() :
               legalMoves.Select(node => node.Evaluation).Min();
            var bestMoves = legalMoves.Where(node => node.Evaluation == bestEvaluation);
            PositionTreeNode resultNode;

            try
            {
                resultNode = bestMoves.Single();
            }

            catch (InvalidOperationException)
            {
                var bestMovesArray = bestMoves.ToArray();
                var index = new Random().Next(bestMovesArray.Length);
                resultNode = bestMovesArray[index];
            }

            var piece = board[resultNode.StartSquareVertical, resultNode.StartSquareHorizontal].ContainedPiece;
            var square = board[resultNode.MoveSquareVertical, resultNode.MoveSquareHorizontal];
            return resultNode.NewPieceName == -1 ? new Move(piece, square) : new Move(piece, square, (ChessPieceName)resultNode.NewPieceName);
        }
    }
}
