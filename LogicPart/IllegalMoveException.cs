namespace Chess.LogicPart
{
    public class IllegalMoveException : ApplicationException { }

    public class KingUnsafetyException : IllegalMoveException { }

    public class PiecePinnedException : KingUnsafetyException { }

    public class PawnPinnedException : PiecePinnedException { }

    public class KingCheckedException : KingUnsafetyException { }

    public class KingMovesToCheckedSquareException : KingCheckedException { }

    public class CastlingIllegalException : IllegalMoveException { }

    public class CastlingKingHasMovedException : CastlingIllegalException { }

    public class CastlingRookHasMovedException : CastlingIllegalException { }

    public class CastlingKingCheckedException : CastlingIllegalException { }

    public class CastlingKingCrossesMenacedSquareException : CastlingIllegalException { }

    public class NewPieceNotSelectedException : IllegalMoveException { }
}
