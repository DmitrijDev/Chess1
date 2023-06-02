
namespace Chess.LogicPart
{
    public class ChessBoard
    {
        private readonly Square[,] _board = new Square[8, 8];
        private readonly Stack<GamePosition> _gamePositions = new();
        private Stack<Move> _moves = new();
        private GameStatus _status = GameStatus.ClearBoard;
        //private bool _isAbleToStartNewGame = true;
        //private int _indexOfMoveForbiddenToTakeback = -1;

        public King WhiteKing { get; private set; }

        public King BlackKing { get; private set; }

        public ChessPieceColor MovingSideColor { get; private set; }

        public int MovesAfterCaptureOrPawnMoveCount { get; private set; }

        public Square PassedByPawnSquare { get; private set; }

        public DrawReason DrawReason { get; private set; } = DrawReason.None;

        public ulong ModCount { get; private set; }

        public ulong GameStartMoment { get; private set; }

        public Comparison<ChessPiece> ComparePieceValues { get; set; }

        public ChessBoard()
        {
            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j] = new Square(this, i, j);
                }
            }
        }

        public ChessBoard(ChessBoard otherBoard) : this()
        {
            if (otherBoard._status == GameStatus.ClearBoard)
            {
                MovingSideColor = otherBoard.MovingSideColor;
                ModCount = otherBoard.ModCount;
                GameStartMoment = otherBoard.GameStartMoment;
                ComparePieceValues = otherBoard.ComparePieceValues;
                return;
            }

            if (otherBoard._status == GameStatus.IllegalPosition)
            {
                SetPosition(otherBoard._gamePositions.Single());
                ModCount = otherBoard.ModCount;
                GameStartMoment = otherBoard.GameStartMoment;
                ComparePieceValues = otherBoard.ComparePieceValues;
                return;
            }

            SetPosition(otherBoard._gamePositions.Last());

            if (otherBoard.ModCount - (ulong)otherBoard._moves.Count > 1)
            {
                ModCount = otherBoard.ModCount - (ulong)otherBoard._moves.Count;
            }

            foreach (var move in otherBoard._moves.Reverse())
            {
                var piece = _board[move.StartSquare.Vertical, move.StartSquare.Horizontal].ContainedPiece;
                var square = _board[move.MoveSquare.Vertical, move.MoveSquare.Horizontal];
                MakeMove(!move.IsPawnPromotion ? new Move(piece, square) : new Move(piece, square, move.NewPiece.Name));
            }

            GameStartMoment = otherBoard.GameStartMoment;
            ComparePieceValues = otherBoard.ComparePieceValues;
        }

        public Square this[int vertical, int horizontal] => _board[vertical, horizontal];

        public IEnumerable<ChessPiece> GetMaterial()
        {
            var initialModCountValue = ModCount;

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (_board[i, j].IsEmpty)
                    {
                        continue;
                    }

                    if (ModCount != initialModCountValue)
                    {
                        throw new InvalidOperationException("Изменение позиции во время перечисления материала.");
                    }

                    yield return _board[i, j].ContainedPiece;
                }
            }

            if (ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение позиции во время перечисления материала.");
            }
        }

        public IEnumerable<ChessPiece> GetMaterial(ChessPieceColor color) => GetMaterial().Where(piece => piece.Color == color);

        public void SetPosition(IEnumerable<ChessPieceName> whitePieceNames, IEnumerable<string> whiteSquareNames,
            IEnumerable<ChessPieceName> blackPieceNames, IEnumerable<string> blackSquareNames, ChessPieceColor movingSideColor)
        {
            if (!whitePieceNames.Any() || whitePieceNames.Count() != whiteSquareNames.Count())
            {
                throw new ArgumentException("Для белых должно быть указано равное положительное количество фигур и полей.");
            }

            if (!blackPieceNames.Any() || blackPieceNames.Count() != blackSquareNames.Count())
            {
                throw new ArgumentException("Для черных должно быть указано равное положительное количество фигур и полей.");
            }

            var whiteMaterial = whitePieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.White));
            var blackMaterial = blackPieceNames.Select(name => ChessPiece.GetNewPiece(name, ChessPieceColor.Black));

            var whitePiecePositions = whiteSquareNames.Select(name => GetSquare(name));
            var blackPiecePositions = blackSquareNames.Select(name => GetSquare(name));

            SetPosition(whiteMaterial.Concat(blackMaterial).ToArray(), whitePiecePositions.Concat(blackPiecePositions).ToArray(), movingSideColor);
        }

        public void SetPosition(GamePosition position)
        {
            var material = new List<ChessPiece>();
            var squares = new List<Square>();

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    if (position.GetPieceName(i, j) != null)
                    {
                        material.Add(ChessPiece.GetNewPiece((ChessPieceName)position.GetPieceName(i, j), (ChessPieceColor)position.GetPieceColor(i, j)));
                        squares.Add(_board[i, j]);
                    }
                }
            }

            SetPosition(material.ToArray(), squares.ToArray(), position.MovingSideColor);
        }

        private void SetPosition(ChessPiece[] material, Square[] piecePositons, ChessPieceColor movingSideColor)
        {
            /*if (!_isAbleToStartNewGame)
            {
                throw new InvalidOperationException("На этой доске невозможно начать новую партию.");
            }*/

            var boardWasCleared = _status != GameStatus.ClearBoard;

            if (_status != GameStatus.ClearBoard)
            {
                Clear();
            }

            MovingSideColor = movingSideColor;

            for (var i = 0; i < material.Length; ++i)
            {
                if (!piecePositons[i].IsEmpty)
                {
                    throw new ArgumentException("Для двух фигур указана одна и та же позиция.");
                }

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

            Square.RenewMenaces(this);
            _gamePositions.Push(new GamePosition(this));

            if (!CheckPositionLegacy())
            {
                _status = GameStatus.IllegalPosition;
            }
            else if (IsDrawByMaterial())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
            }
            else
            {
                _status = GameStatus.GameIsNotOver;

                if (!HasLegalMoves())
                {
                    if (WhiteKing.IsMenaced())
                    {
                        _status = GameStatus.BlackWin;
                    }
                    else if (BlackKing.IsMenaced())
                    {
                        _status = GameStatus.WhiteWin;
                    }
                    else
                    {
                        _status = GameStatus.Draw;
                        DrawReason = DrawReason.Stalemate;
                    }
                }
            }

            if (!boardWasCleared)
            {
                ++ModCount;
            }

            GameStartMoment = ModCount;
        }

        public void Clear()
        {
            /*if (!_isAbleToStartNewGame)
            {
                throw new InvalidOperationException("Эту доску невозможно очистить.");
            }*/

            for (var i = 0; i < 8; ++i)
            {
                for (var j = 0; j < 8; ++j)
                {
                    _board[i, j].Clear();
                    _board[i, j].RemoveMenacesList();
                }
            }

            _gamePositions.Clear();
            _moves.Clear();
            MovesAfterCaptureOrPawnMoveCount = 0;
            PassedByPawnSquare = null;
            WhiteKing = null;
            BlackKing = null;
            _status = GameStatus.ClearBoard;
            DrawReason = DrawReason.None;
            ++ModCount;
        }

        public Square GetSquare(string squareName)
        {
            var coordinates = StringsUsing.GetChessSquareCoordinates(squareName);
            return _board[coordinates[0], coordinates[1]];
        }

        private bool CheckPositionLegacy()
        {
            if (WhiteKing == null || BlackKing == null)
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

                if (MovingSideColor == piece.Color && piece.Attacks(piece.EnemyKing))
                {
                    return false;
                }
            }

            return true;
        }

        private Square GetPassedByPawnSquare()
        {
            if (_moves.Count == 0)
            {
                return null;
            }

            var lastMove = _moves.Peek();

            if (!lastMove.IsPawnDoubleMove)
            {
                return null;
            }

            var vertical = lastMove.MoveSquare.Vertical;
            var horizontal = lastMove.MovingPiece.Color == ChessPieceColor.White ? 2 : 5;
            return _board[vertical, horizontal];
        }

        public bool HasLegalMoves() => GetMaterial(MovingSideColor).Any(piece => piece.CanMove());

        private bool IsDrawByMaterial()
        {
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

            var positionRepeatsCount = 1;
            var skipsCount = 0;
            var positionsLeftCount = MovesAfterCaptureOrPawnMoveCount - 3;

            foreach (var position in _gamePositions.Take(MovesAfterCaptureOrPawnMoveCount + 1).Skip(4))
            {
                if (positionsLeftCount < 5 && positionRepeatsCount == 1)
                {
                    return false;
                }

                --positionsLeftCount;

                if (skipsCount > 0)
                {
                    --skipsCount;
                    continue;
                }

                if (_gamePositions.Peek().IsEqualTo(position))
                {
                    ++positionRepeatsCount;

                    if (positionRepeatsCount == 3)
                    {
                        return true;
                    }

                    skipsCount = 3;
                }
            }

            return false;
        }

        public void MakeMove(Move move)
        {
            if (move.Board != this)
            {
                throw new ArgumentException("Указан ход на другой доске.");
            }

            if (move.CreationMoment != ModCount)
            {
                throw new ArgumentException("Некорректный аргумент. Ход создан для другой позиции.");
            }

            if (!move.MovingPiece.GetAccessibleSquares().Contains(move.MoveSquare))
            {
                var message = move.MovingPiece.GetIllegalMoveMessage(move.MoveSquare);
                throw new IllegalMoveException(message);
            }

            if (move.IsPawnPromotion && !move.NewPieceSelected)
            {
                throw new NewPieceNotSelectedException();
            }

            _moves.Push(move);
            MovesAfterCaptureOrPawnMoveCount = move.IsCapture || move.IsPawnMove ? 0 : MovesAfterCaptureOrPawnMoveCount + 1;

            if (move.IsPawnPromotion)
            {
                move.MovingPiece.Remove();
                move.NewPiece.PutTo(move.MoveSquare);
            }
            else
            {
                move.MovingPiece.PutTo(move.MoveSquare);

                if (move.MovingPiece.FirstMoveMoment == 0)
                {
                    move.MovingPiece.FirstMoveMoment = _moves.Count;
                }
            }

            if (move.IsEnPassantCapture)
            {
                move.CapturedPiece.Remove();
            }

            if (move.IsCastleKingside)
            {
                var rook = _board[7, move.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[5, rook.Horizontal]);
                rook.FirstMoveMoment = _moves.Count;
            }

            if (move.IsCastleQueenside)
            {
                var rook = _board[0, move.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[3, rook.Horizontal]);
                rook.FirstMoveMoment = _moves.Count;
            }

            MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            PassedByPawnSquare = GetPassedByPawnSquare();
            Square.RenewMenaces(this);
            _gamePositions.Push(new GamePosition(this));
            CheckGameResult();
            ++ModCount;
        }

        private void CheckGameResult()
        {
            if (_status != GameStatus.GameIsNotOver)
            {
                return;
            }

            if (!HasLegalMoves())
            {
                if (WhiteKing.IsMenaced())
                {
                    _status = GameStatus.BlackWin;
                    return;
                }

                if (BlackKing.IsMenaced())
                {
                    _status = GameStatus.WhiteWin;
                    return;
                }

                _status = GameStatus.Draw;
                DrawReason = DrawReason.Stalemate;
                return;
            }

            if (IsDrawByMaterial())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.NotEnoughMaterial;
                return;
            }

            if (IsDrawByThreeRepeats())
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.ThreeRepeatsRule;
                return;
            }

            if (MovesAfterCaptureOrPawnMoveCount >= 100)
            {
                _status = GameStatus.Draw;
                DrawReason = DrawReason.FiftyMovesRule;
            }
        }

        public void TakebackMove()
        {
            /*if (_moves.Count <= _indexOfMoveForbiddenToTakeback)
            {
                throw new InvalidOperationException("На этой доске сейчас невозможно взять ход обратно.");
            }*/

            var lastMove = _moves.Pop();

            MovesAfterCaptureOrPawnMoveCount = lastMove.IsCapture || lastMove.IsPawnMove ?
                _moves.TakeWhile(move => !move.IsCapture && !move.IsPawnMove).Count() : MovesAfterCaptureOrPawnMoveCount - 1;

            lastMove.MovingPiece.PutTo(lastMove.StartSquare);

            if (lastMove.MovingPiece.FirstMoveMoment > _moves.Count)
            {
                lastMove.MovingPiece.FirstMoveMoment = 0;
            }

            if (lastMove.IsCapture)
            {
                var capturedPiecePosition = !lastMove.IsEnPassantCapture ? lastMove.MoveSquare :
                _board[lastMove.MoveSquare.Vertical, lastMove.CapturedPiece.Color == ChessPieceColor.White ? 3 : 4];
                lastMove.CapturedPiece.PutTo(capturedPiecePosition);
            }

            if (lastMove.IsPawnPromotion)
            {
                lastMove.NewPiece.Remove();
            }

            if (lastMove.IsCastleKingside)
            {
                var rook = _board[5, lastMove.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[7, rook.Horizontal]);
                rook.FirstMoveMoment = 0;
            }

            if (lastMove.IsCastleQueenside)
            {
                var rook = _board[3, lastMove.MovingPiece.Horizontal].ContainedPiece;
                rook.PutTo(_board[0, rook.Horizontal]);
                rook.FirstMoveMoment = 0;
            }

            MovingSideColor = MovingSideColor == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
            PassedByPawnSquare = GetPassedByPawnSquare();
            Square.RenewMenaces(this);
            _gamePositions.Pop();
            _status = GameStatus.GameIsNotOver;
            DrawReason = DrawReason.None;
            ++ModCount;
        }        

        public IEnumerable<Move> GetLegalMoves()
        {
            var initialModCountValue = ModCount;

            foreach (var piece in GetMaterial(MovingSideColor))
            {
                foreach (var square in piece.GetAccessibleSquares())
                {
                    if (piece.Name != ChessPieceName.Pawn || !(square.Horizontal == 0 || square.Horizontal == 7))
                    {
                        if (ModCount != initialModCountValue)
                        {
                            throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                        }

                        yield return new Move(piece, square);
                    }
                    else
                    {
                        var pieceNames = new ChessPieceName[] { ChessPieceName.Queen, ChessPieceName.Rook, ChessPieceName.Knight, ChessPieceName.Bishop };

                        foreach (var newPieceName in pieceNames)
                        {
                            if (ModCount != initialModCountValue)
                            {
                                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
                            }

                            yield return new Move(piece, square, newPieceName);
                        }
                    }
                }
            }

            if (ModCount != initialModCountValue)
            {
                throw new InvalidOperationException("Изменение коллекции во время перечисления.");
            }
        }

        public Move GetLastMove()
        {
            if (_moves.Count == 0)
            {
                throw new InvalidOperationException("Невозможно получить последний ход: на доске еще не сделано ни одного хода.");
            }

            return _moves.Peek();
        }        

        public GamePosition GetCurrentPosition() => _gamePositions.Count > 0 ? _gamePositions.Peek() : new GamePosition(this);        

        public GameStatus Status
        {
            get => _status;

            set
            {
                if (_status != GameStatus.GameIsNotOver || (value != GameStatus.WhiteWin && value != GameStatus.BlackWin && value != GameStatus.Draw))
                {
                    throw new InvalidOperationException("Невозможное присвоение.");
                }

                _status = value;
            }
        }

        public int MovesCount => _moves.Count;
    }
}