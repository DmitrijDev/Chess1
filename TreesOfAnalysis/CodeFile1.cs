/*public void ReturnTo(GamePosition position)
        {
            if (_gamePositions.Peek() == position)
            {
                return;
            }

            if (!position.OccuredOn(this))
            {
                throw new ArgumentException("Указанная позиция не встречалась на этой доске.");
            }

            if (position.Depth == 0)
            {
                if (_gamePositions.Last() == position)
                {
                    while (Moves.Count > 0)
                    {
                        TakebackMove();
                    }

                    return;
                }
                else
                {
                    throw new ArgumentException("Указанная позиция не встречалась в текущей партии.");
                }
            }

            if (position.GetPrecedingMoves().Any(move => move.CreationMoment <= GameStartMoment))
            {
                throw new ArgumentException("Указанная позиция не встречалась в текущей партии.");
            }

            while (Moves.Count > 0 && !position.GetPrecedingMoves().Contains(Moves.Peek()))
            {
                TakebackMove();
            }

            foreach (var move in position.GetPrecedingMoves().Skip(Moves.Count))
            {
                MakeMove(move);
            }
        }*/

//public Move[] GetMadeInGameMoves() => _moves.Reverse().ToArray();

/*public class AnalysisPosition : GamePosition
    {
        private int _evaluation = int.MinValue;

        internal Move[] PrecedingMoves { get; }

        public AnalysisTree Tree { get; }

        internal AnalysisPosition(AnalysisTree tree) : base(tree.AnalysisBoard)
        {
            PrecedingMoves = tree.AnalysisBoard.GetMadeInGameMoves();
        }

        public int Depth => PrecedingMoves.Length;

        public bool IsEvaluated => _evaluation != int.MinValue;

        public int Evaluation
        {
            get
            {
                if (!IsEvaluated)
                {
                    throw new InvalidOperationException("Узел не имеет оценки.");
                }

                return _evaluation;
            }

            private set
            {
                if (value == int.MinValue)

                {
                    throw new ArgumentException("Для аргумента недопустимо значение, равное int.MinValue.");
                }

                _evaluation = value;
            }
        }
    }*/

/*private int CompareChildrenByAscending(MovesTreeNode child1, MovesTreeNode child2)
        {
            if (!child1.IsEvaluated)
            {
                return child2.IsEvaluated ? 1 : 0;
            }

            if (!child2.IsEvaluated)
            {
                return -1;
            }

            if (child1._evaluation == child2._evaluation)
            {
                return 0;
            }

            return child1._evaluation < child2._evaluation ? -1 : 1;
        }

        private int CompareChildrenByDescending(MovesTreeNode child1, MovesTreeNode child2)
        {
            if (!child1.IsEvaluated)
            {
                return child2.IsEvaluated ? 1 : 0;
            }

            if (!child2.IsEvaluated)
            {
                return -1;
            }

            if (child1._evaluation == child2._evaluation)
            {
                return 0;
            }

            return child1._evaluation > child2._evaluation ? -1 : 1;
        }

        public void SortChildrenByAscending() => Array.Sort(_children, CompareChildrenByAscending);

        public void SortChildrenByDescending() => Array.Sort(_children, CompareChildrenByDescending);*/

/*protected void AnalyzeUntil(Func<bool> stopCondition)
        {
            var currentNode = _analysisTree;
            var nodesUnderAnalysis = new Stack<MovesTreeNode>();
            nodesUnderAnalysis.Push(_analysisTree);

            for (; ; )
            {
                if (ThinkingDisabled)
                {
                    for (var i = nodesUnderAnalysis.Count; i > 1; --i)
                    {
                        Board.TakebackMove();
                    }

                    throw new GameInterruptedException();
                }

                if (!currentNode.HasChildren)
                {
                    currentNode.AddChidren(Board);
                }

                if (currentNode.GetChildren().Any(child => child.IsEvaluated))
                {
                    if (Board.MovingSideColor == ChessPieceColor.White)
                    {
                        currentNode.SortChildrenByDescending();
                    }
                    else
                    {
                        currentNode.SortChildrenByAscending();
                    }
                }
            }
        }*/

//public void RemoveChildren() => _children = null;

//public int ChildrenCount => _children != null ? _children.Length : 0;

/*private void Clear()
        {            
            //AnalysisBoard = null;
            //_startPosition = null;
            _root = null;
            //_activeNode = null;
            //_activeNodeAncestors = null;
        }*/

/*private void ReturnToRoot()
{
    if (_activeNode == _root)
    {
        return;
    }

    while (AnalysisBoard.MovesCount > _board.MovesCount)
    {
        AnalysisBoard.TakebackMove();
    }

    _activeNodeAncestors.Clear();
    _activeNode = _root;
}
*/

/*private void MoveTo(AnalysisPosition position)
{
    if (position == null)
    {
        throw new ArgumentNullException();
    }

    if (position.Tree != this)
    {
        throw new ArgumentException("Невозможно перейти к позиции не из этого дерева.");
    }

    ReturnToRoot();

    foreach (var move in position.PrecedingMoves.Skip(_startPosition.Depth))
    {
        if (!_activeNode.HasChildren)
        {
            _activeNode.AddChidren(AnalysisBoard);
            _activeNode.DescendantsCount = _activeNode.ChildrenCount;

            foreach (var node in _activeNodeAncestors)
            {
                node.DescendantsCount += _activeNode.ChildrenCount;
            }
        }

        var nodes = _activeNode.GetChildren().Where(node => node.StartSquareVertical == move.StartSquare.Vertical &&
        node.StartSquareHorizontal == move.StartSquare.Horizontal && node.MoveSquareVertical == move.MoveSquare.Vertical &&
        node.MoveSquareHorizontal == move.MoveSquare.Horizontal);
        var nextNode = !move.IsPawnPromotion ? nodes.Single() : nodes.Where(node => node.NewPieceName == move.NewPiece.Name).Single();

        _activeNodeAncestors.Push(_activeNode);
        _activeNode = nextNode;

        var newMove = !move.IsPawnPromotion ? new Move(move.MovingPiece, move.MoveSquare) : new Move(move.MovingPiece, move.MoveSquare, move.NewPiece.Name);
        AnalysisBoard.MakeMove(newMove);
    }
}*/


/*public void EvaluatePosition(AnalysisPosition position, int evaluation)
{
    CheckStartPositionChange();

    if (position != AnalysisBoard.GetCurrentPosition())
    {
        MoveTo(position);
    }

    _activeNode.Evaluation = evaluation;
    CorrectAncestorsEvaluations();
}*/

/*using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    internal class GameLine
    {
        private readonly ChessBoard _board;
        private readonly Stack<AnalysisTreeNode> _treeNodes;

        public GameLine(ChessBoard board, Stack<AnalysisTreeNode> treeNodes)
        {
            _board = board;
            _treeNodes = treeNodes;
        }

        

        public void Evaluate(Func<ChessBoard, int> evaluatePosition)
        {
            var movesCount = _board.MovesCount;
            var evaluation = evaluatePosition(_board);

            while (_board.MovesCount > movesCount)
            {
                _board.TakebackMove();
            }

            Evaluate(evaluation);
        }
    }
}*/

/*public void ForbidToStartNewGame() => _isAbleToStartNewGame = false;

        public void ForbidToTakebackMove(int moveIndex)
        {
            if (moveIndex < 0)
            {
                throw new ArgumentException("Невозможно взять обратно ход с отрицательным номером.");
            }

            if (_indexOfMoveForbiddenToTakeback >=0)
            {
                throw new InvalidOperationException("Запрещенный для взятия обратно ход уже установлен.");
            }

            _indexOfMoveForbiddenToTakeback = moveIndex;
        }*/

/*public void AnalyzeToDepth(int depth, Func<ChessBoard, int> evaluatePosition)
        {
            foreach (var line in EnumerateGameLines(depth))
            {
                line.Evaluate(evaluatePosition());
            }
        }*/

