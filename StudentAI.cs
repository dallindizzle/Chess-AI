using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;
using System.Linq;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "Group 12"; }
#else
            get { return "Group 12"; }
#endif
        }

        int movesCount = 0;

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {

            if (myColor == ChessColor.White)
            {
                if (movesCount == 0 && board[1, 7] == ChessPiece.WhiteKnight)
                {
                    movesCount++;
                    return new ChessMove(new ChessLocation(1, 7), new ChessLocation(2, 5));
                }
                if (movesCount == 1 && board[6, 7] == ChessPiece.WhiteKnight)
                {
                    movesCount++;
                    return new ChessMove(new ChessLocation(6, 7), new ChessLocation(5, 5));
                }
            }
            else
            {
                if (movesCount == 0 && board[1, 0] == ChessPiece.BlackKnight)
                {
                    movesCount++;
                    return new ChessMove(new ChessLocation(1, 0), new ChessLocation(2, 2));
                }
                if (movesCount == 1 && board[6, 0] == ChessPiece.BlackKnight)
                {
                    movesCount++;
                    return new ChessMove(new ChessLocation(6, 0), new ChessLocation(5, 2));
                }
            }

            List<ChessMove> moves = GetMoves(board, myColor);


            if (moves.Count == 0)
            {
                return new ChessMove(new ChessLocation(99, 99), new ChessLocation(99, 99), ChessFlag.Stalemate);
            }

            var bestMove = MiniMaxRoot(board, myColor, 3);

            //Check for Checkmate
            var tempBoard = board.Clone();
            tempBoard.MakeMove(bestMove);
            if (myColor == ChessColor.White)
            {
                if (GetMoves(tempBoard, ChessColor.Black).Count == 0 && bestMove.Flag == ChessFlag.Check) bestMove = new ChessMove(bestMove.From, bestMove.To, ChessFlag.Checkmate);
            }
            else if (GetMoves(tempBoard, ChessColor.White).Count == 0 && bestMove.Flag == ChessFlag.Check) bestMove = new ChessMove(bestMove.From, bestMove.To, ChessFlag.Checkmate);

            movesCount++;

            return bestMove;

            //throw (new NotImplementedException());
        }

        private ChessMove GreedyMoves(List<ChessMove> moves, ChessBoard board, ChessColor myColor)
        {

            foreach (var move in moves)
            {
                move.ValueOfMove = HeuristicBoardValue(board);
            }

            // Check for checkmate
            for (int i = 0; i < moves.Count; i++)
            {
                var bestMove = moves[i];
                var fboard = board.Clone();
                fboard.MakeMove(bestMove);
                if (myColor == ChessColor.White)
                {
                    if (GetMoves(fboard, ChessColor.Black).Count == 0 && moves[i].Flag == ChessFlag.Check)
                        moves[i] = new ChessMove(bestMove.From, bestMove.To, ChessFlag.Checkmate);
                }
                else
                {
                    if (GetMoves(fboard, ChessColor.White).Count == 0 && moves[i].Flag == ChessFlag.Check)
                        moves[i] = new ChessMove(bestMove.From, bestMove.To, ChessFlag.Checkmate);
                }
            }

            List<ChessMove> bestMoves = moves;

            // See if there is a checkmate move
            var tempMoves = moves.GroupBy(move => move.Flag == ChessFlag.Checkmate).ToList();
            if (tempMoves.Count > 1)
            {
                bestMoves = tempMoves[tempMoves.Count - 1].ToList();
                return bestMoves.First(); // Immediatly return because WE WON BABY!
            }

            // Sort by value of move
            var temp2Moves = bestMoves.GroupBy(move => move.ValueOfMove).OrderBy(group => group.Key).ToList();


            //Sort by check moves
            //var bbestMoves = bestMoves.GroupBy(move => move.Flag == ChessFlag.Check).ToList();
            //if (bbestMoves.Count > 1)
            //    bestMoves = bbestMoves[bbestMoves.Count - 1].ToList();

            movesCount++;
            return bestMoves[random.Next(bestMoves.Count)];
        }


        private ChessMove MiniMaxRoot(ChessBoard board, ChessColor myColor, int depth)
        {
            var moves = GetMoves(board, myColor);
            var bestMove = new ChessMove(new ChessLocation(99, 99), new ChessLocation(99, 99));
            //var fboard = board.Clone();
            //fboard.MakeMove(bestMove);
            if (myColor == ChessColor.White)
                bestMove.ValueOfMove = int.MinValue;
            else
                bestMove.ValueOfMove = int.MaxValue;

            foreach (ChessMove move in moves)
            {
                var tempBoard = board.Clone();
                tempBoard.MakeMove(move);
                if (myColor == ChessColor.White)
                {
                    int value = MiniMax(tempBoard, depth - 1, int.MinValue, int.MaxValue, false);

                    if (value >= bestMove.ValueOfMove)
                    {
                        bestMove = move;
                        bestMove.ValueOfMove = value;
                    }
                }
                else
                {
                    int value = MiniMax(tempBoard, depth - 1, int.MinValue, int.MaxValue, true);

                    if (value <= bestMove.ValueOfMove)
                    {
                        bestMove = move;
                        bestMove.ValueOfMove = value;
                    }
                }
            }

            return bestMove;

        }

        private int MiniMax(ChessBoard board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0) return HeuristicBoardValue(board);

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;

                var moves = GetMoves(board, ChessColor.White);

                foreach (var move in moves)
                {
                    var child = board.Clone();
                    child.MakeMove(move);
                    int eval = MiniMax(child, depth - 1, alpha, beta, false);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                var moves = GetMoves(board, ChessColor.Black);

                foreach (var move in moves)
                {
                    var child = board.Clone();
                    child.MakeMove(move);
                    int eval = MiniMax(child, depth - 1, alpha, beta, true);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            int x = moveToCheck.From.X;
            int y = moveToCheck.From.Y;
            int xc = moveToCheck.To.X;
            int yc = moveToCheck.To.Y;

            if (colorOfPlayerMoving == ChessColor.White)
            {
                if (boardBeforeMove[xc, yc] > ChessPiece.Empty) return false;

                if (boardBeforeMove[x, y] == ChessPiece.WhitePawn)
                {
                    if (xc == x + 1 && yc == y - 1 || xc == x - 1 && yc == y - 1 && boardBeforeMove[xc, yc] < ChessPiece.Empty) return true;

                    if (y == 6 && yc == y - 2 && xc == x && boardBeforeMove[xc, yc + 1] == ChessPiece.Empty && boardBeforeMove[xc, yc] == ChessPiece.Empty) return true;
                    else if (x == xc && yc == y - 1 && boardBeforeMove[xc, yc] == ChessPiece.Empty) return true;
                    else if (xc == x - 1 && yc == y - 1 && boardBeforeMove[xc, yc] < ChessPiece.Empty) return true; // If curTile < BlankTile then black. Opposite of slides
                    else if (xc == x + 1 && yc == y - 1 && boardBeforeMove[xc, yc] < ChessPiece.Empty) return true;
                    return false;
                }

            }
            else
            {
                if (boardBeforeMove[xc, yc] < ChessPiece.Empty) return false;

                if (boardBeforeMove[x, y] == ChessPiece.BlackPawn)
                {
                    if (xc == x + 1 && yc == y + 1 || xc == x - 1 && yc == y + 1 && boardBeforeMove[xc, yc] > ChessPiece.Empty) return true;

                    if (y == 1 && yc == y + 2 && x == xc && boardBeforeMove[xc, yc - 1] == ChessPiece.Empty && boardBeforeMove[xc, yc] == ChessPiece.Empty) return true;
                    else if (x == xc && yc == y + 1 && boardBeforeMove[xc, yc] == ChessPiece.Empty) return true;
                    else if (xc == x - 1 && yc == y + 1 && boardBeforeMove[xc, yc] > ChessPiece.Empty) return true; // If curTile > BlankTile then white. Opposite of slides
                    else if (xc == x + 1 && yc == y + 1 && boardBeforeMove[xc, yc] > ChessPiece.Empty) return true;
                    return false;
                }
            }

            if (boardBeforeMove[x, y] == ChessPiece.WhiteRook || boardBeforeMove[x, y] == ChessPiece.BlackRook)
            {
                if (yc != y && xc != x) return false;

                if (xc == x)
                {
                    if (yc > y)
                    {
                        for (int i = y + 1; i < yc; i++)
                        {
                            if (boardBeforeMove[x, i] != ChessPiece.Empty) return false;
                        }
                    }
                    else
                    {
                        for (int i = y - 1; i > yc; i--)
                        {
                            if (boardBeforeMove[x, i] != ChessPiece.Empty) return false;
                        }
                    }
                }
                else
                {
                    if (xc > x)
                    {
                        for (int i = x + 1; i < xc; i++)
                        {
                            if (boardBeforeMove[i, y] != ChessPiece.Empty) return false;
                        }
                    }
                    else
                    {
                        for (int i = x - 1; i > xc; i--)
                        {
                            if (boardBeforeMove[i, y] != ChessPiece.Empty) return false;
                        }
                    }
                }
                return true;
            }

            else if (boardBeforeMove[x, y] == ChessPiece.WhiteBishop || boardBeforeMove[x, y] == ChessPiece.BlackBishop)
            {
                if (Math.Abs(x - xc) != Math.Abs(y - yc)) return false;

                if (xc > x && yc < y)
                {
                    int i = x + 1;
                    int j = y - 1;
                    while (i != xc && j != yc)
                    {
                        if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                        i++;
                        j--;
                    }
                }
                else if (xc < x && yc < y)
                {
                    int i = x - 1;
                    int j = y - 1;
                    while (i != xc && j != yc)
                    {
                        if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                        i--;
                        j--;
                    }
                }
                else if (xc < x && yc > y)
                {
                    int i = x - 1;
                    int j = y + 1;
                    while (i != xc && j != yc)
                    {
                        if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                        i--;
                        j++;
                    }
                }
                else if (xc > x && yc > y)
                {
                    int i = x + 1;
                    int j = y + 1;
                    while (i != xc && j != yc)
                    {
                        if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                        i++;
                        j++;
                    }
                }

                return true;
            }

            else if (boardBeforeMove[x, y] == ChessPiece.WhiteKnight || boardBeforeMove[x, y] == ChessPiece.BlackKnight)
            {
                if (xc == x - 1 && yc == y - 2) return true;
                else if (xc == x + 1 && yc == y - 2) return true;

                else if (xc == x + 2 && yc == y - 1) return true;
                else if (xc == x + 2 && yc == y + 1) return true;

                else if (xc == x + 1 && yc == y + 2) return true;
                else if (xc == x - 1 && yc == y + 2) return true;

                else if (xc == x - 2 && yc == y - 1) return true;
                else if (xc == x - 2 && yc == y + 1) return true;

                return false;
            }

            else if (boardBeforeMove[x, y] == ChessPiece.WhiteQueen || boardBeforeMove[x, y] == ChessPiece.BlackQueen)
            {
                if (Math.Abs(x - xc) == Math.Abs(y - yc))
                {
                    if (xc > x && yc < y)
                    {
                        int i = x + 1;
                        int j = y - 1;
                        while (i != xc && j != yc)
                        {
                            if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                            i++;
                            j--;
                        }
                    }
                    else if (xc < x && yc < y)
                    {
                        int i = x - 1;
                        int j = y - 1;
                        while (i != xc && j != yc)
                        {
                            if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                            i--;
                            j--;
                        }
                    }
                    else if (xc < x && yc > y)
                    {
                        int i = x - 1;
                        int j = y + 1;
                        while (i != xc && j != yc)
                        {
                            if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                            i--;
                            j++;
                        }
                    }
                    else if (xc > x && yc > y)
                    {
                        int i = x + 1;
                        int j = y + 1;
                        while (i != xc && j != yc)
                        {
                            if (boardBeforeMove[i, j] != ChessPiece.Empty) return false;
                            i++;
                            j++;
                        }
                    }
                    return true;
                }

                else if (yc == y || xc == x)
                {
                    if (xc == x)
                    {
                        if (yc > y)
                        {
                            for (int i = y + 1; i < yc; i++)
                            {
                                if (boardBeforeMove[x, i] != ChessPiece.Empty) return false;
                            }
                        }
                        else
                        {
                            for (int i = y - 1; i > yc; i--)
                            {
                                if (boardBeforeMove[x, i] != ChessPiece.Empty) return false;
                            }
                        }
                    }
                    else
                    {
                        if (xc > x)
                        {
                            for (int i = x + 1; i < xc; i++)
                            {
                                if (boardBeforeMove[i, y] != ChessPiece.Empty) return false;
                            }
                        }
                        else
                        {
                            for (int i = x - 1; i > xc; i--)
                            {
                                if (boardBeforeMove[i, y] != ChessPiece.Empty) return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }

            else if (boardBeforeMove[x, y] == ChessPiece.WhiteKing || boardBeforeMove[x, y] == ChessPiece.BlackKing)
            {
                if (xc == x && yc == y + 1) return true;
                if (xc == x && yc == y - 1) return true;
                if (xc == x + 1 && yc == y) return true;
                if (xc == x - 1 && yc == y) return true;
                if (xc == x + 1 && yc == y + 1) return true;
                if (xc == x - 1 && yc == y + 1) return true;
                if (xc == x + 1 && yc == y - 1) return true;
                if (xc == x - 1 && yc == y - 1) return true;
                return false;
            }


            throw (new NotImplementedException());
        }

        #endregion


        private Random random = new Random();

        private List<ChessLocation> GetPieces(ChessBoard board, ChessColor myColor)
        {
            List<ChessLocation> pieces = new List<ChessLocation>();

            if (myColor == ChessColor.White)
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    for (int y = 0; y < ChessBoard.NumberOfRows; y++)
                    {
                        if (board[x, y] == ChessPiece.WhiteBishop || board[x, y] == ChessPiece.WhiteKing || board[x, y] == ChessPiece.WhiteKnight || board[x, y] == ChessPiece.WhitePawn || board[x, y] == ChessPiece.WhiteQueen || board[x, y] == ChessPiece.WhiteRook)
                            pieces.Add(new ChessLocation(x, y));
                    }
                }
            }
            else
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    for (int y = 0; y < ChessBoard.NumberOfRows; y++)
                    {
                        if (board[x, y] == ChessPiece.BlackBishop || board[x, y] == ChessPiece.BlackKing || board[x, y] == ChessPiece.BlackKnight || board[x, y] == ChessPiece.BlackPawn || board[x, y] == ChessPiece.BlackQueen || board[x, y] == ChessPiece.BlackRook)
                            pieces.Add(new ChessLocation(x, y));
                    }
                }
            }

            return pieces;
        }

        private List<ChessMove> GetMoves(ChessBoard board, ChessColor myColor)
        {
            var pieces = GetPieces(board, myColor);
            List<ChessMove> moves = new List<ChessMove>();

            foreach (var piece in pieces)
            {
                moves.AddRange(GenMoves(board, piece, myColor));
            }

            var validMoves = moves.Where(move => !InCheck(board, move, myColor)).ToList(); // Filters moves that put us into check

            // Checking if enemy king is in check
            for (int i = 0; i < validMoves.Count; i++)
            {
                if (myColor == ChessColor.White)
                {
                    if (InCheck(board, validMoves[i], ChessColor.Black)) validMoves[i] = new ChessMove(validMoves[i].From, validMoves[i].To, ChessFlag.Check);
                }
                else
                {
                    if (InCheck(board, validMoves[i], ChessColor.White)) validMoves[i] = new ChessMove(validMoves[i].From, validMoves[i].To, ChessFlag.Check);
                }
            }

            return validMoves;
        }

        private List<ChessMove> GenMoves(ChessBoard board, ChessLocation location, ChessColor myColor)
        {
            List<ChessMove> potMoves = new List<ChessMove>();

            int x = location.X;
            int y = location.Y;

            if (board[x, y] == ChessPiece.WhitePawn)
            {
                if (y - 1 > -1 && board[x, y - 1] == ChessPiece.Empty)
                {
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 1)));
                    if (y == 6 && board[x, y - 2] == ChessPiece.Empty && board[x, y - 1] == ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 2)));
                }
                if (x - 1 > -1 && y - 1 > -1 && board[x - 1, y - 1] < ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y - 1)));
                if (x + 1 < 8 && y - 1 > -1 && board[x + 1, y - 1] < ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y - 1)));
            }
            else if (board[x, y] == ChessPiece.BlackPawn)
            {
                if (y + 1 < 8 && board[x, y + 1] == ChessPiece.Empty)
                {
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + 1)));
                    if (y == 1 && board[x, y + 2] == ChessPiece.Empty && board[x, y + 1] == ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + 2)));
                }
                if (x - 1 > -1 && y + 1 < 8 && board[x - 1, y + 1] > ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + 1)));
                if (x + 1 < 8 && y + 1 < 8 && board[x + 1, y + 1] > ChessPiece.Empty) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + 1)));
            }
            else if (board[x, y] == ChessPiece.WhiteRook || board[x, y] == ChessPiece.BlackRook)
            {

                for (int i = 0; i < 8; i++)
                {
                    if (i == x) continue;
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(i, y)));
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i == y) continue;
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, i)));
                }

            }
            else if (board[x, y] == ChessPiece.WhiteBishop || board[x, y] == ChessPiece.BlackBishop)
            {

                int bx = x + 1;
                int by = y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx++, by++)));

                bx = x - 1;
                by = y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx--, by--)));

                bx = x - 1;
                by = y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx--, by++)));

                bx = x + 1;
                by = y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx++, by--)));

            }
            else if (board[x, y] == ChessPiece.WhiteKnight || board[x, y] == ChessPiece.BlackKnight)
            {
                if (x + 2 < 8)
                {
                    if (y + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 2, y + 1)));
                    if (y - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 2, y - 1)));
                }
                if (y - 2 > -1)
                {
                    if (x - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y - 2)));
                    if (x + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y - 2)));
                }
                if (x - 2 > -1)
                {
                    if (y - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 2, y - 1)));
                    if (y + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 2, y + 1)));
                }
                if (y + 2 < 8)
                {
                    if (x - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + 2)));
                    if (x + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + 2)));
                }
            }
            else if (board[x, y] == ChessPiece.WhiteQueen || board[x, y] == ChessPiece.BlackQueen)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i == x) continue;
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(i, y)));
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i == y) continue;
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, i)));
                }

                int bx = x + 1;
                int by = y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx++, by++)));

                bx = x - 1;
                by = y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx--, by--)));

                bx = x - 1;
                by = y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx--, by++)));

                bx = x + 1;
                by = y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                    potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(bx++, by--)));
            }
            else if (board[x, y] == ChessPiece.WhiteKing || board[x, y] == ChessPiece.BlackKing)
            {
                if (x + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y)));
                if (x - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y)));
                if (y + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + 1)));
                if (y - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 1)));
                if (x + 1 < 8 && y + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + 1)));
                if (x - 1 > -1 && y - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y - 1)));
                if (x + 1 < 8 && y - 1 > -1) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y - 1)));
                if (x - 1 > -1 && y + 1 < 8) potMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + 1)));
            }

            List<ChessMove> validMoves = potMoves.Where(move => IsValidMove(board, move, myColor)).ToList();

            return validMoves;
        }


        // Returns true if with the move made, the king will be in check
        private bool InCheck(ChessBoard board, ChessMove move, ChessColor myColor, ChessLocation piece = null)
        {
            var fboard = board.Clone(); // Future board. board if the move is made. MAKE SURE TO USE THIS BOARD IN THIS METHOD!!!
            fboard.MakeMove(move);

            ChessLocation kingLoc;

            if (piece == null)
            {
                kingLoc = FindKing(fboard, myColor);
            }
            else
            {
                kingLoc = piece;
            }

            if (myColor == ChessColor.White)
            {

                // Check for black pawns
                if (kingLoc.X - 1 > -1 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X - 1, kingLoc.Y - 1] == ChessPiece.BlackPawn) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X + 1, kingLoc.Y - 1] == ChessPiece.BlackPawn) return true;

                // Check for Enemy Queen and Rook
                for (int x = kingLoc.X + 1; x < 8; x++)
                {
                    if (fboard[x, kingLoc.Y] == ChessPiece.BlackRook || fboard[x, kingLoc.Y] == ChessPiece.BlackQueen) return true;
                    if (fboard[x, kingLoc.Y] != ChessPiece.Empty) break; // If its any other piece then we're good
                }

                for (int x = kingLoc.X - 1; x > -1; x--)
                {
                    if (fboard[x, kingLoc.Y] == ChessPiece.BlackRook || fboard[x, kingLoc.Y] == ChessPiece.BlackQueen) return true;
                    if (fboard[x, kingLoc.Y] != ChessPiece.Empty) break; // If its any other piece then we're good
                }

                for (int y = kingLoc.Y + 1; y < 8; y++)
                {
                    if (fboard[kingLoc.X, y] == ChessPiece.BlackRook || fboard[kingLoc.X, y] == ChessPiece.BlackQueen) return true;
                    if (fboard[kingLoc.X, y] != ChessPiece.Empty) break;
                }

                for (int y = kingLoc.Y - 1; y > -1; y--)
                {
                    if (fboard[kingLoc.X, y] == ChessPiece.BlackRook || fboard[kingLoc.X, y] == ChessPiece.BlackQueen) return true;
                    if (fboard[kingLoc.X, y] != ChessPiece.Empty) break;
                }

                // Check for Enemy Queen and Bishop
                int bx = kingLoc.X + 1;
                int by = kingLoc.Y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.BlackQueen || fboard[bx, by] == ChessPiece.BlackBishop) return true;
                    if (fboard[bx++, by++] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X - 1;
                by = kingLoc.Y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.BlackQueen || fboard[bx, by] == ChessPiece.BlackBishop) return true;
                    if (fboard[bx--, by--] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X - 1;
                by = kingLoc.Y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.BlackQueen || fboard[bx, by] == ChessPiece.BlackBishop) return true;
                    if (fboard[bx--, by++] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X + 1;
                by = kingLoc.Y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.BlackQueen || fboard[bx, by] == ChessPiece.BlackBishop) return true;
                    if (fboard[bx++, by--] != ChessPiece.Empty) break;
                }

                //Check for Knight
                if (kingLoc.X + 2 < 8 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X + 2, kingLoc.Y + 1] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X + 2 < 8 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X + 2, kingLoc.Y - 1] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X - 2 > -1 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X - 2, kingLoc.Y + 1] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X - 2 > -1 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X - 2, kingLoc.Y - 1] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y - 2 > -1 && fboard[kingLoc.X - 1, kingLoc.Y - 2] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y - 2 > -1 && fboard[kingLoc.X + 1, kingLoc.Y - 2] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y + 2 < 8 && fboard[kingLoc.X - 1, kingLoc.Y + 2] == ChessPiece.BlackKnight) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y + 2 < 8 && fboard[kingLoc.X + 1, kingLoc.Y + 2] == ChessPiece.BlackKnight) return true;

                // Check for King
                if (kingLoc.X + 1 < 8 && fboard[kingLoc.X + 1, kingLoc.Y] == ChessPiece.BlackKing) return true;
                if (kingLoc.X - 1 > -1 && fboard[kingLoc.X - 1, kingLoc.Y] == ChessPiece.BlackKing) return true;
                if (kingLoc.Y + 1 < 8 && fboard[kingLoc.X, kingLoc.Y + 1] == ChessPiece.BlackKing) return true;
                if (kingLoc.Y - 1 > -1 && fboard[kingLoc.X, kingLoc.Y - 1] == ChessPiece.BlackKing) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X + 1, kingLoc.Y + 1] == ChessPiece.BlackKing) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X + 1, kingLoc.Y - 1] == ChessPiece.BlackKing) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X - 1, kingLoc.Y + 1] == ChessPiece.BlackKing) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X - 1, kingLoc.Y - 1] == ChessPiece.BlackKing) return true;
            }
            else
            {
                // Check for White pawns
                if (kingLoc.X - 1 > -1 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X - 1, kingLoc.Y + 1] == ChessPiece.WhitePawn) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X + 1, kingLoc.Y + 1] == ChessPiece.WhitePawn) return true;

                // Check for Enemy Queen and Rook
                for (int x = kingLoc.X + 1; x < 8; x++)
                {
                    if (fboard[x, kingLoc.Y] == ChessPiece.WhiteRook || fboard[x, kingLoc.Y] == ChessPiece.WhiteQueen) return true;
                    if (fboard[x, kingLoc.Y] != ChessPiece.Empty) break; // If its any other piece then we're good
                }

                for (int x = kingLoc.X - 1; x > -1; x--)
                {
                    if (fboard[x, kingLoc.Y] == ChessPiece.WhiteRook || fboard[x, kingLoc.Y] == ChessPiece.WhiteQueen) return true;
                    if (fboard[x, kingLoc.Y] != ChessPiece.Empty) break; // If its any other piece then we're good
                }

                for (int y = kingLoc.Y + 1; y < 8; y++)
                {
                    if (fboard[kingLoc.X, y] == ChessPiece.WhiteRook || fboard[kingLoc.X, y] == ChessPiece.WhiteQueen) return true;
                    if (fboard[kingLoc.X, y] != ChessPiece.Empty) break;
                }

                for (int y = kingLoc.Y - 1; y > -1; y--)
                {
                    if (fboard[kingLoc.X, y] == ChessPiece.WhiteRook || fboard[kingLoc.X, y] == ChessPiece.WhiteQueen) return true;
                    if (fboard[kingLoc.X, y] != ChessPiece.Empty) break;
                }

                // Check for Enemy Queen and Bishop
                int bx = kingLoc.X + 1;
                int by = kingLoc.Y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.WhiteQueen || fboard[bx, by] == ChessPiece.WhiteBishop) return true;
                    if (fboard[bx++, by++] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X - 1;
                by = kingLoc.Y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.WhiteQueen || fboard[bx, by] == ChessPiece.WhiteBishop) return true;
                    if (fboard[bx--, by--] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X - 1;
                by = kingLoc.Y + 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.WhiteQueen || fboard[bx, by] == ChessPiece.WhiteBishop) return true;
                    if (fboard[bx--, by++] != ChessPiece.Empty) break;
                }

                bx = kingLoc.X + 1;
                by = kingLoc.Y - 1;

                while (bx > -1 && by > -1 && bx < 8 && by < 8)
                {
                    if (fboard[bx, by] == ChessPiece.WhiteQueen || fboard[bx, by] == ChessPiece.WhiteBishop) return true;
                    if (fboard[bx++, by--] != ChessPiece.Empty) break;
                }

                //Check for Knight
                if (kingLoc.X + 2 < 8 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X + 2, kingLoc.Y + 1] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X + 2 < 8 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X + 2, kingLoc.Y - 1] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X - 2 > -1 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X - 2, kingLoc.Y + 1] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X - 2 > -1 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X - 2, kingLoc.Y - 1] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y - 2 > -1 && fboard[kingLoc.X - 1, kingLoc.Y - 2] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y - 2 > -1 && fboard[kingLoc.X + 1, kingLoc.Y - 2] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y + 2 < 8 && fboard[kingLoc.X - 1, kingLoc.Y + 2] == ChessPiece.WhiteKnight) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y + 2 < 8 && fboard[kingLoc.X + 1, kingLoc.Y + 2] == ChessPiece.WhiteKnight) return true;

                // Check for King
                if (kingLoc.X + 1 < 8 && fboard[kingLoc.X + 1, kingLoc.Y] == ChessPiece.WhiteKing) return true;
                if (kingLoc.X - 1 > -1 && fboard[kingLoc.X - 1, kingLoc.Y] == ChessPiece.WhiteKing) return true;
                if (kingLoc.Y + 1 < 8 && fboard[kingLoc.X, kingLoc.Y + 1] == ChessPiece.WhiteKing) return true;
                if (kingLoc.Y - 1 > -1 && fboard[kingLoc.X, kingLoc.Y - 1] == ChessPiece.WhiteKing) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X + 1, kingLoc.Y + 1] == ChessPiece.WhiteKing) return true;
                if (kingLoc.X + 1 < 8 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X + 1, kingLoc.Y - 1] == ChessPiece.WhiteKing) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y + 1 < 8 && fboard[kingLoc.X - 1, kingLoc.Y + 1] == ChessPiece.WhiteKing) return true;
                if (kingLoc.X - 1 > -1 && kingLoc.Y - 1 > -1 && fboard[kingLoc.X - 1, kingLoc.Y - 1] == ChessPiece.WhiteKing) return true;
            }

            return false;
        }

        private ChessLocation FindKing(ChessBoard board, ChessColor myColor)
        {
            ChessLocation kingLoc = new ChessLocation(0, 0);
            if (myColor == ChessColor.White)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (board[x, y] == ChessPiece.WhiteKing)
                        {
                            kingLoc.X = x;
                            kingLoc.Y = y;
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (board[x, y] == ChessPiece.BlackKing)
                        {
                            kingLoc.X = x;
                            kingLoc.Y = y;
                        }
                    }
                }
            }

            return kingLoc;
        }

        //Returns the value of the board
        private int HeuristicBoardValue(ChessBoard board)
        {
            int val = 0;

            int bx = 7;
            for (int x = 0; x < 8; x++)
            {
                int by = 7;
                for (int y = 0; y < 8; y++)
                {
                    switch(board[x,y])
                    {
                        case ChessPiece.BlackPawn:
                            val += -100;
                            val += -PawnTable[by, x];
                            break;
                        case ChessPiece.BlackKnight:
                            val += -320;
                            val += -KnightTable[by, x];
                            break;
                        case ChessPiece.BlackBishop:
                            val += -330;
                            val += -BishopTable[by, x];
                            break;
                        case ChessPiece.BlackRook:
                            val += -500;
                            val += -RookTable[by, x];
                            break;
                        case ChessPiece.BlackQueen:
                            val += -900;
                            val += -QueenTable[by, x];
                            break;
                        case ChessPiece.BlackKing:
                            val += -20000;
                            val += -KingTable[by, x];
                            break;
                        case ChessPiece.WhitePawn:
                            val += 100;
                            val += PawnTable[y, x];
                            break;
                        case ChessPiece.WhiteKnight:
                            val += 320;
                            val += KnightTable[y, x];
                            break;
                        case ChessPiece.WhiteBishop:
                            val += 330;
                            val += BishopTable[y, x];
                            break;
                        case ChessPiece.WhiteRook:
                            val += 500;
                            val += RookTable[y, x];
                            break;
                        case ChessPiece.WhiteQueen:
                            val += 900;
                            val += QueenTable[y, x];
                            break;
                        case ChessPiece.WhiteKing:
                            val += 20000;
                            val += KingTable[y, x];
                            break;
                    }
                    by--;
                }
                bx--;
            }

            return val;
        }

        private static readonly short[,] PawnTable = new short[,]
        {
             { 0,  0,  0,  0,  0,  0, 0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
             { 5,  5, 10, 27, 27, 10,  5,  5 },
             { 0,  0,  0, 25, 25,  0,  0,  0 },
             { 5, -5,-10,  0,  0,-10, -5,  5 },
             { 5, 10, 10,-25,-25, 10, 10,  5 },
             { 0,  0,  0,  0,  0,  0,  0,  0 }
        };

        private static readonly short[,] KnightTable = new short[,]
        {
            { -50,-40,-30,-30,-30,-30,-40,-50 },
            { -40,-20,  0,  0,  0,  0,-20,-40},
            { -30,  0, 10, 15, 15, 10,  0,-30 },
            { -30,  5, 15, 20, 20, 15,  5,-30 },
            { -30,  0, 15, 20, 20, 15,  0,-30 },
            { -30,  5, 10, 15, 15, 10,  5,-30 },
            { -40,-20,  0,  5,  5,  0,-20,-40 },
            { -50,-40,-20,-30,-30,-20,-40,-50 }
        };

        private static readonly short[,] BishopTable = new short[,]
        {
            { -20,-10,-10,-10,-10,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5, 10, 10,  5,  0,-10 },
            { -10,  5,  5, 10, 10,  5,  5,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10, 10, 10, 10, 10, 10, 10,-10 },
            { -10,  5,  0,  0,  0,  0,  5,-10 },
            { -20,-10,-40,-10,-10,-40,-10,-20 },
        };

        private static readonly short[,] RookTable = new short[,]
        {
            { 0 ,0, 0, 0, 0, 0, 0, 0 },
            { 5,  10,  10,  10,  10,  10,  10, 5 },
            { -5,  0,  0, 0, 0,  0,  0, -5 },
            { -5,  0,  0, 0, 0,  0,  0, -5 },
            { -5,  0, 0, 0, 0, 10,  0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5,  0,  0,  0,  0,  0,  5, -5 },
            { 0, 0, 0, 5, 5, 5, 0, 0 },
        };

        private static readonly short[,] KingTable = new short[,]
        {
          { -30, -40, -40, -50, -50, -40, -40, -30 },
          { -30, -40, -40, -50, -50, -40, -40, -30 },
          { -30, -40, -40, -50, -50, -40, -40, -30 },
          { -30, -40, -40, -50, -50, -40, -40, -30 },
          { -20, -30, -30, -40, -40, -30, -30, -20 },
          { -10, -20, -20, -20, -20, -20, -20, -10 },
           { 20,  20,   0,   0,   0,   0,  20,  20 },
           { 20,  30,  10,   0,   0,  10,  30,  20 }
        };

        private static readonly short[,] QueenTable = new short[,]
        {
            {-20, -10, -10, -5, -5, -10, -10, -20 },
            {-10, 0, 0, 0, 0, 0, 0, -10 },
            {-10, 0, 5, 5, 5, 5, 0, -10 },
            {-5, 0, 5, 5, 5, 5, 0, -5 },
            {0, 0, 5, 5, 5, 5, 0, -5 },
            {-10, 5, 5, 5, 5, 5, 0, -10 },
            {-10, 0, 5, 0, 0, 0, 0, -10 },
            {-10, -10, -10, -5, -5, -10, -10, -20 }
        };






        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}