using ChessChallenge.API;
using ChessChallenge.Chess;
using System;
using System.Linq;
using System.Threading.Tasks;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;
using PieceList = ChessChallenge.Chess.PieceList;

public class MyBot : IChessBot
{
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

                int tmp = CalcMove(board, nextMove, (byte) (1 + depthCounter));
                if ((board.IsWhiteToMove&&tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
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
