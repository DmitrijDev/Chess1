
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private Stack<Move> _moves = new();

        public ChessPieceColor MovingSideColor { get; private set; }

        public GamePosition InitialPosition { get; private set; }

        public BoardStatus Status { get; private set; } = BoardStatus.ClearBoard;

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public Square PassedByPawnSquare { get; private set; }

        public DrawReason DrawReason { get; private set; } = DrawReason.None;

        public ulong GameStartsCount { get; private set; }

        public ulong ModCount { get; private set; }

        internal bool WhiteMenacesActual { get; private set; } = true;

        internal bool BlackMenacesActual { get; private set; } = true;

        public ChessBoard()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = new(this, i, j);
                }
            }
        }

        public Square this[int vertical, int horizontal] => _board[vertical, horizontal];

        public void CopyGameState(ChessBoard source)
        {
            lock (source)
            {
                lock (this)
                {
                    if (GameStartsCount == ulong.MaxValue)
                    {
                        throw new OverflowException("Переполнение числа партий.");
                    }
                    else
                    {
                        ++GameStartsCount;
                        ModCount = 0;
                    }

                    MovingSideColor = source.MovingSideColor;

                    if (source.Status == BoardStatus.ClearBoard)
                    {
                        DoAfterPositionSet();
                        return;
                    }

                    SetDefaultValues();

                    WhiteMenacesActual = false;
                    BlackMenacesActual = false;

                    InitialPosition = new(source.InitialPosition);
                    CopyMaterial(source);
                    CopyMoves(source._moves);

                    Status = source.Status;
                    MovesAfterCaptureOrPawnMoveCount = source.MovesAfterCaptureOrPawnMoveCount;

                    if (source.PassedByPawnSquare != null)
                    {
                        PassedByPawnSquare = _board[source.PassedByPawnSquare.Vertical, source.PassedByPawnSquare.Horizontal];
                    }

                    DrawReason = source.DrawReason;
                    DoAfterPositionSet();
                }
            }
        }

        private void CopyMaterial(ChessBoard sourceBoard)
        {
            foreach (var sourcePiece in sourceBoard.GetMaterial())
            {
                var copyPiece = ChessPiece.GetNewPiece(sourcePiece.Name, sourcePiece.Color);
                copyPiece.FirstMoveMoment = sourcePiece.FirstMoveMoment;
                var square = _board[sourcePiece.Vertical, sourcePiece.Horizontal];
                copyPiece.PutTo(square);
            }

            if (sourceBoard.WhiteKing != null)
            {
                WhiteKing = (King)_board[sourceBoard.WhiteKing.Vertical, sourceBoard.WhiteKing.Horizontal].ContainedPiece;
            }

            if (sourceBoard.BlackKing != null)
            {
                BlackKing = (King)_board[sourceBoard.BlackKing.Vertical, sourceBoard.BlackKing.Horizontal].ContainedPiece;
            }
        }

        private void CopyMoves(Stack<Move> source)
        {
            var sourceCapturedPieces = source.Where(move => move.IsCapture).Select(move => move.CapturedPiece);
            var sourcePromotedPawns = source.Where(move => move.IsPawnPromotion).Select(move => move.MovingPiece);
            var sourceRemovedPieces = sourceCapturedPieces.Concat(sourcePromotedPawns).ToArray();

            var sourceRemovedPiecesCopies = sourceRemovedPieces.
                Select(sourcePiece =>
            {
                var copyPiece = ChessPiece.GetNewPiece(sourcePiece.Name, sourcePiece.Color);
                copyPiece.FirstMoveMoment = sourcePiece.FirstMoveMoment;
                return copyPiece;
            }).
            ToArray();

            foreach (var sourceMove in source.Reverse())
            {
                var copyMove = new Move()
                {
                    Precedent = _moves.Count > 0 ? _moves.Peek() : InitialPosition,
                    Depth = sourceMove.Depth,
                    StartSquare = _board[sourceMove.StartSquare.Vertical, sourceMove.StartSquare.Horizontal],
                    MoveSquare = _board[sourceMove.MoveSquare.Vertical, sourceMove.MoveSquare.Horizontal],
                    IsEnPassantCapture = sourceMove.IsEnPassantCapture,
                    IsCastleKingside = sourceMove.IsCastleKingside,
                    IsCastleQueenside = sourceMove.IsCastleQueenside
                };

                if (sourceMove.MovingPiece.IsOnBoard)
                {
                    copyMove.MovingPiece = _board[sourceMove.MovingPiece.Vertical, sourceMove.MovingPiece.Horizontal].ContainedPiece;
                }

                if (sourceMove.IsPawnPromotion && sourceMove.NewPiece.IsOnBoard)
                {
                    copyMove.NewPiece = _board[sourceMove.NewPiece.Vertical, sourceMove.NewPiece.Horizontal].ContainedPiece;
                }

                for (var i = 0; ; ++i)
                {
                    if (copyMove.MovingPiece != null && (!sourceMove.IsCapture || copyMove.CapturedPiece != null) &&
                        (!sourceMove.IsPawnPromotion || copyMove.NewPiece != null))
                    {
                        break;
                    }

                    var sourcePiece = sourceRemovedPieces[i];
                    var copyPiece = sourceRemovedPiecesCopies[i];

                    if (sourceMove.MovingPiece == sourcePiece)
                    {
                        copyMove.MovingPiece = copyPiece;
                    }
                    else if (sourceMove.CapturedPiece == sourcePiece)
                    {
                        copyMove.CapturedPiece = copyPiece;
                    }
                    else if (sourceMove.NewPiece == sourcePiece)
                    {
                        copyMove.NewPiece = copyPiece;
                    }
                }

                _moves.Push(copyMove);
            }
        }

        public IEnumerable<ChessPiece> GetMaterial()
        {
            ulong modCount;
            ulong gameStartsCount;

            lock (this)
            {
                modCount = ModCount;
                gameStartsCount = GameStartsCount;
            }

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (!_board[i, j].IsEmpty)
                    {
                        var piece = _board[i, j].ContainedPiece;

                        if (ModCount != modCount || GameStartsCount != gameStartsCount)
                        {
                            throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
                        }

                        yield return piece;
                    }
                }
            }

            if (ModCount != modCount || GameStartsCount != gameStartsCount)
            {
                throw new InvalidOperationException("Позиция была изменена во время перечисления материала.");
            }
        }

        private void IncreaseModCount()
        {
            if (ModCount == ulong.MaxValue)
            {
                throw new OverflowException("Переполнение ModCount.");
            }

            ++ModCount;
        }

        public IEnumerable<ChessPiece> GetMaterial(ChessPieceColor color) => GetMaterial().Where(piece => piece.Color == color);

        internal void RenewMenaces(ChessPieceColor gameSide)
        {
            lock (this)
            {
                if ((gameSide == ChessPieceColor.White && WhiteMenacesActual) ||
                    (gameSide == ChessPieceColor.Black && BlackMenacesActual))
                {
                    return;
                }

                if (Status != BoardStatus.ClearBoard)
                {
                    for (var i = 0; i < 8; ++i)
                    {
                        for (var j = 0; j < 8; ++j)
                        {
                            if (gameSide == ChessPieceColor.White)
                            {
                                _board[i, j].WhiteMenaces = null;
                            }
                            else
                            {
                                _board[i, j].BlackMenaces = null;
                            }
                        }
                    }
                }

                foreach (var piece in GetMaterial(gameSide))
                {
                    foreach (var square in piece.GetAttackedSquares())
                    {
                        if (gameSide == ChessPieceColor.White)
                        {
                            square.WhiteMenaces ??= new List<ChessPiece>();
                            square.WhiteMenaces.Add(piece);
                        }
                        else
                        {
                            square.BlackMenaces ??= new List<ChessPiece>();
                            square.BlackMenaces.Add(piece);
                        }
                    }
                }

                if (gameSide == ChessPieceColor.White)
                {
                    WhiteMenacesActual = true;
                }
                else
                {
                    BlackMenacesActual = true;
                }
            }
        }

        private void SetDefaultValues()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j].Clear();
                    _board[i, j].WhiteMenaces = null;
                    _board[i, j].BlackMenaces = null;
                }
            }

            WhiteMenacesActual = true;
            BlackMenacesActual = true;

            _moves.Clear();
            InitialPosition = null;

            WhiteKing = null;
            BlackKing = null;

            MovesAfterCaptureOrPawnMoveCount = 0;
            PassedByPawnSquare = null;

            Status = BoardStatus.ClearBoard;
            DrawReason = DrawReason.None;
        }

        public void Clear()
        {
            lock (this)
            {
                IncreaseModCount();
                SetDefaultValues();
                DoAfterClear();
            }
        }

        private void SetPosition(List<ChessPiece> material, List<Square> piecePositons, ChessPieceColor movingSideColor)
        {
            lock (this)
            {
                if (piecePositons.Distinct().Count() != piecePositons.Count)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

                if (GameStartsCount == ulong.MaxValue)
                {
                    throw new OverflowException("Переполнение числа партий.");
                }
                else
                {
                    ++GameStartsCount;
                    ModCount = 0;
                }

                if (Status != BoardStatus.ClearBoard)
                {
                    SetDefaultValues();
                }

                MovingSideColor = movingSideColor;

                if (material.Count == 0)
                {
                    DoAfterPositionSet();
                    return;
                }

                WhiteMenacesActual = false;
                BlackMenacesActual = false;

                for (var i = 0; i < material.Count; ++i)
                {
                    material[i].PutTo(piecePositons[i]);

                    if (material[i].Name == ChessPieceName.King)
                    {
                        if (material[i].Color == ChessPieceColor.White)
                        {
                            WhiteKing = (King)material[i];
                        }
                        else
                        {
                            BlackKing = (King)material[i];
                        }
                    }
                }

                InitialPosition = new(this);

                if (!CheckPositionLegacy())
                {
                    Status = BoardStatus.IllegalPosition;
                    DoAfterPositionSet();
                    return;
                }

                if (IsDrawByMaterial())
                {
                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.NotEnoughMaterial;
                    DoAfterPositionSet();
                    return;
                }

                if (GetMaterial(MovingSideColor).Any(piece => piece.CanMove()))
                {
                    Status = BoardStatus.GameIsIncomplete;
                    DoAfterPositionSet();
                }
                else
                {
                    if (MovingSideColor == ChessPieceColor.White)
                    {
                        if (WhiteKing.IsChecked())
                        {
                            Status = BoardStatus.BlackWin;
                            DoAfterPositionSet();
                            return;
                        }
                    }
                    else if (BlackKing.IsChecked())
                    {
                        Status = BoardStatus.WhiteWin;
                        DoAfterPositionSet();
                        return;
                    }

                    Status = BoardStatus.Draw;
                    DrawReason = DrawReason.Stalemate;
                    DoAfterPositionSet();
                }
            }
        }

        private bool CheckPositionLegacy()
        {
            if (WhiteKing == null || BlackKing == null)
            {
                return false;
            }

            var king = MovingSideColor == ChessPieceColor.White ? BlackKing : WhiteKing;

            if (king.IsChecked())
            {
                return false;
            }

            foreach (var piece in GetMaterial())
            {
                if (piece.Name == ChessPieceName.King && piece != WhiteKing && piece != BlackKing)
                {
                    return false;
                }

                if (piece.Name == ChessPieceName.Pawn && (piece.Horizontal == 0 || piece.Horizontal == 7))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsDrawByMaterial()
        {
            if (_moves.Count > 0 && !_moves.Peek().IsCapture && !_moves.Peek().IsPawnPromotion)
            {
                return false;
            }

            var knightsPresent = false;
            var lightSquaredBishopsPresent = false;
            var darkSquaredBishopsPresent = false;

            foreach (var piece in GetMaterial())
            {
                switch (piece.Name)
                {
                    case ChessPieceName.Queen:
                    case ChessPieceName.Rook:
                    case ChessPieceName.Pawn:
                        {
                            return false;
                        }

                    case ChessPieceName.Knight:
                        {
                            if (knightsPresent || lightSquaredBishopsPresent || darkSquaredBishopsPresent)
                            {
                                return false;
                            }

                            knightsPresent = true;
                            break;
                        }

                    case ChessPieceName.Bishop:
                        {
                            if (knightsPresent)
                            {
                                return false;
                            }

                            if (piece.Vertical % 2 != piece.Horizontal % 2)
                            {
                                lightSquaredBishopsPresent = true;
                            }
                            else
                            {
                                darkSquaredBishopsPresent = true;
                            }

                            if (lightSquaredBishopsPresent && darkSquaredBishopsPresent)
                            {
                                return false;
                            }

                            break;
                        }
                };
            }

            return true;
        }

        private bool IsDrawByThreeRepeats()
        {
            if (MovesAfterCaptureOrPawnMoveCount < 8)
            {
                return false;
            }

            var current = new GamePosition(this);
            var other = new GamePosition(current);

            var positionRepeatsCount = 1;
            var skipsCount = 3;
            var movesLeftCount = MovesAfterCaptureOrPawnMoveCount;

            foreach (var move in _moves.Take(MovesAfterCaptureOrPawnMoveCount))
            {
                other.ToPreceding(move);

                if (skipsCount > 0)
                {
                    --skipsCount;
                }
                else if (current.IsEqualTo(other))
                {
                    ++positionRepeatsCount;

                    if (positionRepeatsCount == 3)
                    {
                        return true;
                    }

                    skipsCount = 3;
                }

                --movesLeftCount;

                if (movesLeftCount < 5 && positionRepeatsCount == 1)
                {
                    break;
                }
            }

            return false;
        }

        public void SetPosition(IEnumerable<ChessPieceName> whitePieceNames, IEnumerable<string> whiteSquareNames,
        IEnumerable<ChessPieceName> blackPieceNames, IEnumerable<string> blackSquareNames, ChessPieceColor movingSideColor)
        {
            var whiteMaterial = whitePieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.White)).ToArray();
            var whitePiecePositions = whiteSquareNames.Select(name => GetSquare(name)).ToArray();

            if (whiteMaterial.Length == 0 || whiteMaterial.Length != whitePiecePositions.Length)
            {
                throw new ArgumentException("Для белых должно быть указано равное ненулевое количество фигур и полей.");
            }

            var blackMaterial = blackPieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.Black)).ToArray();
            var blackPiecePositions = blackSquareNames.Select(name => GetSquare(name)).ToArray();

            if (blackMaterial.Length == 0 || blackMaterial.Length != blackPiecePositions.Length)
            {
                throw new ArgumentException("Для черных должно быть указано равное ненулевое количество фигур и полей.");
            }

            SetPosition(whiteMaterial.Concat(blackMaterial).ToList(), whitePiecePositions.Concat(blackPiecePositions).ToList(), movingSideColor);
        }

        public Square GetSquare(string squareName)
        {
            var coordinates = StringsUsing.GetSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        public void SetPosition(GamePosition position)
        {
            var material = new List<ChessPiece>();
            var squares = new List<Square>();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (position.GetPieceName(i, j) == null)
                    {
                        continue;
                    }

                    var pieceName = (ChessPieceName)position.GetPieceName(i, j);
                    var pieceColor = (ChessPieceColor)position.GetPieceColor(i, j);
                    var piece = ChessPiece.GetNewPiece(pieceName, pieceColor);

                    material.Add(piece);
                    squares.Add(_board[i, j]);
                }
            }

            SetPosition(material, squares, position.MovingSideColor);
        }

        private Square GetPassedByPawnSquare(Move lastMove)
        {
            if (!lastMove.IsPawnDoubleVerticalMove)
            {
                return null;
            }

            var vertical = lastMove.MoveSquare.Vertical;
            var horizontal = lastMove.MovingPiece.Color == ChessPieceColor.White ? 2 : 5;
            return _board[vertical, horizontal];
        }

        public void MakeMove(Move move)
        {
            lock (this)
            {
                if (Status != BoardStatus.GameIsIncomplete)
                {
                    throw new InvalidOperationException("На доске не идет партия.");
                }

                if (move.Board != this)
                {
                    throw new ArgumentException("Указан ход на другой доске.");
                }

                if (_moves.Count > 0)
                {
                    if (move.Precedent != _moves.Peek())
                    {
                        throw new ArgumentException("Ход был создан для другой позиции.");
                    }
                }
                else if (move.Precedent != InitialPosition)
                {
                    throw new ArgumentException("Ход был создан для другой позиции.");
                }

                move.MovingPiece.CheckLegacy(move);

                IncreaseModCount();
                DoBeforeMove();
                WhiteMenacesActual = false;
                BlackMenacesActual = false;

                if (!move.IsPawnPromotion)
                {
                    move.MovingPiece.PutTo(move.MoveSquare);
                }
                else
                {
                    move.MovingPiece.Remove();
                    move.NewPiece.PutTo(move.MoveSquare);
                }

                if (move.IsEnPassantCapture)
                {
                    move.CapturedPiece.Remove();
                }
                else if (move.IsCastleKingside)
                {
                    var rook = _board[7, move.MovingPiece.Horizontal].ContainedPiece;
                    rook.PutTo(_board[5, rook.Horizontal]);
                    rook.FirstMoveMoment = _moves.Count + 1;
                }
                else if (move.IsCastleQueenside)
                {
                    var rook = _board[0, move.MovingPiece.Horizontal].ContainedPiece;
                    rook.PutTo(_board[3, rook.Horizontal]);
                    rook.FirstMoveMoment = _moves.Count + 1;
                }

                MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
                MovesAfterCaptureOrPawnMoveCount = move.IsCapture || move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;
                PassedByPawnSquare = GetPassedByPawnSquare(move);
                _moves.Push(move);

                if (move.MovingPiece.FirstMoveMoment == 0)
                {
                    move.MovingPiece.FirstMoveMoment = _moves.Count;
                }

                CheckGameResult();
                DoAfterMove();
            }
        }

        private void CheckGameResult()
        {
            if (!GetMaterial(MovingSideColor).Any(piece => piece.CanMove()))
            {
                if (MovingSideColor == ChessPieceColor.White)
                {
                    if (WhiteKing.IsChecked())
                    {
                        Status = BoardStatus.BlackWin;
                        return;
                    }
                }
                else if (BlackKing.IsChecked())
                {
                    Status = BoardStatus.WhiteWin;
                    return;
                }

                Status = BoardStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (IsDrawByMaterial())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                return;
            }

            if (IsDrawByThreeRepeats())
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.ThreeRepeatsRule;
                return;
            }

            if (MovesAfterCaptureOrPawnMoveCount >= 100)
            {
                Status = BoardStatus.Draw;
                DrawReason = DrawReason.FiftyMovesRule;
            }
        }

        public void TakebackMove()
        {
            lock (this)
            {
                if (_moves.Count == 0)
                {
                    throw new InvalidOperationException("Невозможно взять ход обратно: на доске начальная позиция партии.");
                }

                IncreaseModCount();
                DoBeforeTakingBack();
                WhiteMenacesActual = false;
                BlackMenacesActual = false;

                var lastMove = _moves.Pop();

                lastMove.MovingPiece.PutTo(lastMove.StartSquare);

                if (lastMove.IsCapture)
                {
                    var capturedPiecePosition = !lastMove.IsEnPassantCapture ? lastMove.MoveSquare :
                    _board[lastMove.MoveSquare.Vertical, lastMove.StartSquare.Horizontal];

                    lastMove.CapturedPiece.PutTo(capturedPiecePosition);
                }

                if (lastMove.IsPawnPromotion)
                {
                    lastMove.NewPiece.Remove();
                }
                else if (lastMove.IsCastleKingside)
                {
                    var rook = _board[5, lastMove.MovingPiece.Horizontal].ContainedPiece;
                    rook.PutTo(_board[7, rook.Horizontal]);
                    rook.FirstMoveMoment = 0;
                }
                else if (lastMove.IsCastleQueenside)
                {
                    var rook = _board[3, lastMove.MovingPiece.Horizontal].ContainedPiece;
                    rook.PutTo(_board[0, rook.Horizontal]);
                    rook.FirstMoveMoment = 0;
                }

                MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
                PassedByPawnSquare = _moves.Count > 0 ? GetPassedByPawnSquare(_moves.Peek()) : null;

                MovesAfterCaptureOrPawnMoveCount = lastMove.IsCapture || lastMove.IsPawnMove ?
                    _moves.TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count() : MovesAfterCaptureOrPawnMoveCount - 1;

                if (lastMove.MovingPiece.FirstMoveMoment > _moves.Count)
                {
                    lastMove.MovingPiece.FirstMoveMoment = 0;
                }

                Status = BoardStatus.GameIsIncomplete;
                DrawReason = DrawReason.None;
                DoAfterTakingBack();
            }
        }

        public List<Move> GetLegalMoves()
        {
            var result = new List<Move>();

            lock (this)
            {
                if (Status == BoardStatus.IllegalPosition)
                {
                    throw new InvalidOperationException("На доске невозможная позиция.");
                }

                if (Status != BoardStatus.GameIsIncomplete)
                {
                    return result;
                }

                foreach (var piece in GetMaterial(MovingSideColor))
                {
                    foreach (var square in piece.GetAccessibleSquares())
                    {
                        if (piece.Name != ChessPieceName.Pawn || !(square.Horizontal == 0 || square.Horizontal == 7))
                        {
                            result.Add(new Move(piece, square));
                            continue;
                        }

                        var pieceNames = new ChessPieceName[] { ChessPieceName.Queen, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Bishop };

                        foreach (var newPieceName in pieceNames)
                        {
                            result.Add(new Move(piece, square, newPieceName));
                        }
                    }
                }
            }

            return result;
        }

        public Move GetLastMove() => _moves.Count > 0 ? _moves.Peek() : null;

        public void SetStatus(BoardStatus newStatus)
        {
            lock (this)
            {
                if (Status != BoardStatus.GameIsIncomplete || (newStatus != BoardStatus.WhiteWin &&
                    newStatus != BoardStatus.BlackWin && newStatus != BoardStatus.Draw))
                {
                    throw new InvalidOperationException("Невозможное присвоение.");
                }

                IncreaseModCount();
                Status = newStatus;
            }
        }

        public string GetGameText() => StringsUsing.GetGameText(this);

        protected virtual void DoAfterPositionSet()
        { }

        protected virtual void DoAfterClear()
        { }

        protected virtual void DoBeforeMove()
        { }

        protected virtual void DoAfterMove()
        { }

        protected virtual void DoBeforeTakingBack()
        { }

        protected virtual void DoAfterTakingBack()
        { }

        public int MovesCount => _moves.Count;
    }
}