using Chess.LogicPart;
using System.Collections;

namespace Chess.TreesOfAnalysis
{
    internal class TreeEnumerator : IEnumerator<AnalysisTreeNode>
    {
        private Stack<Queue<AnalysisTreeNode>> _queues = new();
        private GamePosition _lastPosition;

        public AnalysisTree Tree { get; private set; }

        public ChessBoard Board { get; private set; }

        public int CurrentDepth { get; private set; } = -1;

        public int MaximumDepth { get; }

        public AnalysisTreeNode Current { get; private set; }

        public Predicate<AnalysisTreeNode> ShouldStopAt { get; private set; } = (node) => false;

        public TreeEnumerator(AnalysisTree tree, int maximimDepth, Predicate<AnalysisTreeNode> shoulsStopAt)
        {
            Tree = tree;

            if (maximimDepth < 0)
            {
                throw new ArgumentException("Глубина перебора дерева не может быть отрицательной.");
            }

            MaximumDepth = maximimDepth;
            ShouldStopAt = shoulsStopAt;
        }

        private void CheckPositionChange()
        {
            if (Board.GetCurrentPosition() != _lastPosition)
            {
                throw new InvalidOperationException("Невозможно продолжать перечисление не вернув доску к позиции на предыдущем шаге.");
            }
        }

        public void Reset()
        {
            _queues.Clear();
            Board = null;
            _lastPosition = null;
            CurrentDepth = -1;
            Current = null;
        }

        public bool MoveNext()
        {
            if (Current == null)
            {
                Current = Tree.Root;
                Tree.CheckStartPositionChange();
                Board = new ChessBoard(Tree.Board);
                _lastPosition = Board.GetCurrentPosition();
                CurrentDepth = 0;

                if (Tree.AnalysisDisabled)
                {
                    throw new ApplicationException("Анализ позиции прерван.");
                }

                return true;
            }

            if (CurrentDepth < MaximumDepth && !ShouldStopAt(Current))
            {
                Tree.CheckStartPositionChange();
                CheckPositionChange();
                Current.AddChidren(Board);
            }

            if (_queues.Count < CurrentDepth + 1)
            {
                var newQueue = CurrentDepth < MaximumDepth && !ShouldStopAt(Current) ?
                    new Queue<AnalysisTreeNode>(Current.GetChildren()) : new Queue<AnalysisTreeNode>();
                _queues.Push(newQueue);
            }

            while (_queues.Peek().Count == 0 && CurrentDepth > 0)
            {
                CheckPositionChange();
                Board.TakebackMove();
                _lastPosition = Board.GetCurrentPosition();
                Current = Current.Parent;
                --CurrentDepth;
                _queues.Pop();

                if (ShouldStopAt(Current))
                {
                    _queues.Peek().Clear();
                }
            }

            if (_queues.Peek().Count == 0)
            {
                Tree.CheckStartPositionChange();
                CheckPositionChange();

                if (Tree.AnalysisDisabled)
                {
                    throw new ApplicationException("Анализ позиции прерван.");
                }

                return false;
            }

            Current = _queues.Peek().Dequeue();
            ++CurrentDepth;
            CheckPositionChange();
            var piece = Board[Current.StartSquareVertical, Current.StartSquareHorizontal].ContainedPiece;
            var square = Board[Current.MoveSquareVertical, Current.MoveSquareHorizontal];
            var move = !Current.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, Current.NewPieceName);
            Board.MakeMove(move);
            _lastPosition = Board.GetCurrentPosition();

            Tree.CheckStartPositionChange();

            if (Tree.AnalysisDisabled)
            {
                throw new ApplicationException("Анализ позиции прерван.");
            }

            return true;
        }

        public void Dispose()
        {
            _queues = null;
            _lastPosition = null;
            Tree = null;
            Board = null;
            Current = null;
            ShouldStopAt = null;
        }

        object IEnumerator.Current => Current;
    }
}
