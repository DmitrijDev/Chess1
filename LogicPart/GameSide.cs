
namespace Chess.LogicPart
{
    internal class GameSide
    {
        private readonly ChessBoard _board;

        public PieceColor Color { get; }

        public King King { get; set; }

        public GameSide(PieceColor color, ChessBoard board)
        {
            Color = color;
            _board = board;
        }

        internal IEnumerable<ChessPiece> GetMaterial() => _board.GetMaterial().Where(piece => piece.Color == Color);

        public GameSide Enemy => Color == PieceColor.White ? _board.Black : _board.White;
    }
}