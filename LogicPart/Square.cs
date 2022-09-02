using System.Text;

namespace Chess.LogicPart
{
    internal class Square
    {
        private ChessPiece _containedPiece;
        private readonly List<ChessPiece> _menaces = new();
        private int _lastMenacesListClearMoment = -1;

        public ChessBoard Board { get; }

        public int Vertical { get; }

        public int Horizontal { get; }

        public bool IsLegalForEnPassantCapture { get; set; }

        public Square(ChessBoard board, int vertical, int horizontal)
        {
            Board = board;
            Vertical = vertical;
            Horizontal = horizontal;
        }

        public void SetDefaultValues()
        {
            _containedPiece = null;
            _lastMenacesListClearMoment = -1;
        }

        public void AddMenace(ChessPiece piece)
        {
            if (_lastMenacesListClearMoment < Board.MovesCount)
            {
                _menaces.Clear();
                _lastMenacesListClearMoment = Board.MovesCount;
            }

            _menaces.Add(piece);
        }

        public List<ChessPiece> GetMenaces() => IsMenaced() ? _menaces : new List<ChessPiece>();

        public bool IsMenaced()
        {
            if (!Board.MovingSide.Enemy.AttacksListsAreActual())
            {
                Board.MovingSide.Enemy.RenewAttacksLists();
            }

            return _lastMenacesListClearMoment == Board.MovesCount && _menaces.Count > 0;
        }

        public bool IsOnSameDiagonal(Square otherSquare) => otherSquare != null && Math.Abs(Vertical - otherSquare.Vertical) == Math.Abs(Horizontal - otherSquare.Horizontal);

        public bool IsOnSameDiagonal(ChessPiece piece) => piece != null && IsOnSameDiagonal(piece.Position);

        public string Name
        {
            get
            {
                const string verticalNames = "abcdefgh";
                return new StringBuilder().Append(verticalNames[Vertical]).Append(Horizontal + 1).ToString();
            }
        }

        public ChessPiece ContainedPiece
        {
            get => _containedPiece;

            set
            {
                var oldPiece = _containedPiece;
                _containedPiece = value;

                if (oldPiece != null)
                {
                    oldPiece.Position = null;
                }

                if (_containedPiece != null && _containedPiece.Position != this)
                {
                    _containedPiece.Position = this;
                }
            }
        }

        public bool IsEmpty => _containedPiece == null;
    }
}