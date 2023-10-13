using Chess.LogicPart;

namespace Chess.TreesOfAnalysis
{
    public class TreeNode
    {
        private sbyte _startSquareVertical = -1;
        private sbyte _startSquareHorizontal = -1;
        private sbyte _moveSquareVertical = -1;
        private sbyte _moveSquareHorizontal = -1;
        private sbyte _newPieceName;

        private int _evaluation = int.MinValue;

        internal short Index { get; private set; } = -1;

        internal short[] Path { get; set; }

        public short Depth { get; internal set; }

        public bool IsWhiteMove { get; internal set; }

        internal TreeNode[] Children { get; set; }

        public long DescendantsCount { get; internal set; }

        internal TreeNode(Tree tree)
        {
            if (tree.Board.MovesCount > 0)
            {
                SetMoveParams(tree.Board.GetLastMove());
                Depth = (short)tree.Board.MovesCount;
            }

            IsWhiteMove = tree.Board.MovesCount > 0 && tree.Board.MovingSideColor == ChessPieceColor.Black;
        }

        internal TreeNode(Move move) => SetMoveParams(move);

        private void SetMoveParams(Move move)
        {
            _startSquareVertical = (sbyte)move.StartSquare.Vertical;
            _startSquareHorizontal = (sbyte)move.StartSquare.Horizontal;
            _moveSquareVertical = (sbyte)move.MoveSquare.Vertical;
            _moveSquareHorizontal = (sbyte)move.MoveSquare.Horizontal;

            if (move.NewPieceSelected)
            {
                _newPieceName = (sbyte)move.NewPiece.Name;
            }

            Index = GetIndex();
        }

        private short GetIndex() => (short)(_startSquareVertical * 4096 + _startSquareHorizontal * 512 +
            _moveSquareVertical * 64 + _moveSquareHorizontal * 8 + _newPieceName);

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

            set
            {
                if (value == int.MinValue)
                {
                    throw new ArgumentException("Для этого свойства недопустимо присвоение значения, равного int.MinValue.");
                }

                _evaluation = value;
            }
        }

        public int StartSquareVertical => _startSquareVertical >= 0 ? _startSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int StartSquareHorizontal => _startSquareHorizontal >= 0 ? _startSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareVertical => _moveSquareVertical >= 0 ? _moveSquareVertical : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public int MoveSquareHorizontal => _moveSquareHorizontal >= 0 ? _moveSquareHorizontal : throw new InvalidOperationException("Этот корневой узел не хранит координат полей.");

        public bool IsPawnPromotion => _newPieceName > 0;

        public ChessPieceName NewPieceName => IsPawnPromotion ? (ChessPieceName)_newPieceName :
        throw new InvalidOperationException("Это свойство может быть вычислено только для узла, соотв. превращению пешки.");        
    }
}
