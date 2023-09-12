using ChessChallenge.API;
using System;
using System.Threading.Tasks;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        /*
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestValueCapture = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }
            }

            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }
        */
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 10, 30, 33, 50, 90, 1000 };
        private byte maxDepth = 3;
        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();
            Move moveToPlay = allMoves[0];
            int highestValue = 1337420;

            /*
            foreach (Move move in allMoves)
            {
                int tmp = CalcMove(board, move, 0);
                if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
                {
                    moveToPlay = move;
                    highestValue = tmp;
                }
            */
            Parallel.ForEach(allMoves, move =>
            {
                int tmp = CalcMove(Board.CreateBoardFromFEN(board.GetFenString()), move, 0);
                if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
                {
                    moveToPlay = move;
                    highestValue = tmp;
                }
            });

            return moveToPlay;
        }


        private int CalcMove(Board board, Move move, byte depthCounter)
        {
            board.MakeMove(move);
            if (depthCounter < maxDepth && !board.IsInCheckmate())
            {
                Move[] allMoves = board.GetLegalMoves();
                int highestValue = 1337420;

                foreach (Move nextMove in allMoves)
                {

                    int tmp = CalcMove(board, nextMove, (byte)(1 + depthCounter));
                    if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
                    {
                        highestValue = tmp;
                    }
                }
                board.UndoMove(move);
                return highestValue;
            }

            int output = EvaluateBoard(board);
            board.UndoMove(move);
            return output;

        }

        private int EvaluateBoard(Board board)
        {
            if (board.IsDraw())
            {
                return 0;
            }

            if (board.IsInCheckmate())
            {
                int factor = board.IsWhiteToMove ? 1 : -1;
                return -1000 * factor;

            }
            int eval = 0;
            foreach (ChessChallenge.API.PieceList pieces in board.GetAllPieceLists())
            {
                int factor = pieces.IsWhitePieceList ? 1 : -1;
                eval += pieceValues[(int)pieces.TypeOfPieceInList] * factor * pieces.Count;
            }
            return eval;
        }
    }
}