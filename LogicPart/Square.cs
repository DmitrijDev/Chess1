using System.Text;

namespace Chess.LogicPart
{
    internal class Square
    {
        private ChessPiece _containedPiece;
        private List<ChessPiece> _menaces;

        public ChessBoard Board { get; }

        public int Vertical { get; }

        public int Horizontal { get; }

        public Square(ChessBoard board, int vertical, int horizontal)
        {
            Board = board;
            Vertical = vertical;
            Horizontal = horizontal;
        }

        public void SetDefaultValues()
        {
            _containedPiece = null;
            _menaces = null;
        }

        public List<ChessPiece> GetMenaces()
        {
            if (Board.LastMenacesRenewMoment < Board.MovesCount)
            {
                RenewMenaces();
            }

            return _menaces != null ? new List<ChessPiece>(_menaces) : new List<ChessPiece>();
        }

        public bool IsMenaced()
        {
            if (Board.LastMenacesRenewMoment < Board.MovesCount)
            {
                RenewMenaces();
            }

            return _menaces != null;
        }

        private void RenewMenaces()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    Board[i, j]._menaces = null;
                }
            }

            foreach (var piece in Board.MovingSide.Enemy.GetMaterial())
            {
                foreach (var square in piece.GetAttackedSquares())
                {
                    square._menaces ??= new List<ChessPiece>();
                    square._menaces.Add(piece);
                }
            }
            
            Board.LastMenacesRenewMoment = Board.MovesCount;
        }

        public bool IsOnSameDiagonal(Square otherSquare) => otherSquare != null && otherSquare.Board == Board &&
            Math.Abs(Vertical - otherSquare.Vertical) == Math.Abs(Horizontal - otherSquare.Horizontal);

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