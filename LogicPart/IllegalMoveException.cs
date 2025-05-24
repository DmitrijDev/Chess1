namespace Chess.LogicPart
{
    public class IllegalMoveException : ApplicationException { }

    public class KingUnsafetyException : IllegalMoveException { }

    public class PiecePinnedException : KingUnsafetyException { }

    public class PawnPinnedException : PiecePinnedException { }

    public class KingCheckedException : KingUnsafetyException { }

    public class KingMovesIntoCheckException : KingUnsafetyException { }

    public class IllegalCastlingException : IllegalMoveException { }

    public class KingHasMovedException : IllegalCastlingException { }

    public class RookHasMovedException : IllegalCastlingException { }

    public class CastlingKingCheckedException : IllegalCastlingException { }

    public class KingPassesUnsafeSquareException : IllegalCastlingException { }

    public class NewPieceNotSelectedException : IllegalMoveException { }
}
