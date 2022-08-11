
namespace Chess.LogicPart
{
    internal class GameSide
    {
        private int _allAttacksRenewLastMoment = -1;
        private readonly ChessBoard _board;

        public PieceColor Color { get; }

        public King King { get; set; }

        public List<ChessPiece> Material { get; } = new();

        public GameSide(PieceColor color, ChessBoard board)
        {
            Color = color;
            _board = board;
        }

        public void RenewAttacksLists()
        {
            foreach (var piece in Material)
            {
                piece.GetAttackedSquares();
            }

            _allAttacksRenewLastMoment = _board.MovesCount;
        }

        public bool AttacksListsAreActual()
        {
            if (_allAttacksRenewLastMoment == _board.MovesCount)
            {
                return true;
            }

            foreach (var piece in Material)
            {
                if (piece.LastAttacksRenewMoment < _board.MovesCount)
                {
                    return false;
                }
            }

            _allAttacksRenewLastMoment = _board.MovesCount;
            return true;
        }

        public GameSide Enemy => Color == PieceColor.White ? _board.Black : _board.White;
    }
}