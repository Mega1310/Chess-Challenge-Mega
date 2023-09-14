﻿using ChessChallenge.API;
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
    int[] pieceValues = { 0, 100, 300, 325, 500, 900, 10000 };
    private byte maxDepth = 4;
    private Random rnd = new Random();

    

    public enum GameState
    {
        Opening,
        MiddleGame,
        EndGame
    }

    private GameState currentGamestate = GameState.Opening;

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Move moveToPlay = allMoves[rnd.Next(allMoves.Length-1)];
        int highestValue = 1337420;
        int pieceCounter = 0;

        foreach (ChessChallenge.API.PieceList pieces in board.GetAllPieceLists())
        {
            switch (pieces.TypeOfPieceInList)
            {
                case PieceType.None:
                    break;
                case PieceType.Pawn:
                    break;
                case PieceType.Knight:
                    pieceCounter += pieces.Count;
                    break;
                case PieceType.Bishop:
                    pieceCounter += pieces.Count;
                    break;
                case PieceType.Rook:
                    pieceCounter += pieces.Count;
                    break;
                case PieceType.Queen:
                    pieceCounter += pieces.Count;
                    break;
                case PieceType.King:
                    break;
            }
        }
        if (pieceCounter <= 4)
        {
            maxDepth = 5;
            if(currentGamestate == GameState.Opening)
            {
                Console.WriteLine(board.IsWhiteToMove);
                Console.WriteLine("Endgame");
            }
            currentGamestate = GameState.EndGame;
        }
        if(pieceCounter <= 2)
        {
            maxDepth = 7;
        }

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
        /*
        Parallel.ForEach(allMoves, move =>
        {
            int tmp = CalcMove(Board.CreateBoardFromFEN(board.GetFenString()), move, 0);
            if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
            {
                moveToPlay = move;
                highestValue = tmp;
            }
        });
        */
        /*
        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int tmp = CalcMoveAlphaBeta(board, -10000, 10000,0);
            board.UndoMove(move);
            if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
            {
                moveToPlay = move;
                highestValue = tmp;
            }
        }
        */
        Parallel.ForEach(allMoves, move =>
        {
            Board tmpBoard = Board.CreateBoardFromFEN(board.GetFenString());
            tmpBoard.MakeMove(move);
            int tmp = CalcMoveAlphaBeta(tmpBoard, -10000, 10000, 0);
            tmpBoard.UndoMove(move);
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

    private int CalcMoveAlphaBeta(Board board, int alpha, int beta, int depthCounter) {
        if (depthCounter == maxDepth)
        {
            return EvaluateBoard(board);
        }

        Move[] newGameMoves = board.GetLegalMoves();

        if (board.IsWhiteToMove)
        {
            int bestMove = -9999;
            for (int i = 0; i < newGameMoves.Length; i++)
            {
                board.MakeMove(newGameMoves[i]);
                bestMove = Math.Max(bestMove, CalcMoveAlphaBeta(board, alpha, beta, 1 + depthCounter));
                board.UndoMove(newGameMoves[i]);
                alpha = Math.Max(alpha, bestMove);
                if (beta <= alpha)
                {
                    return bestMove;
                }
            }
            return bestMove;
        }
        else
        {
            var bestMove = 9999;
            for (var i = 0; i < newGameMoves.Length; i++)
            {
                board.MakeMove(newGameMoves[i]);
                bestMove = Math.Min(bestMove, CalcMoveAlphaBeta(board, alpha, beta, 1 + depthCounter));
                board.UndoMove(newGameMoves[i]);
                beta = Math.Min(beta, bestMove);
                if (beta <= alpha)
                {
                    return bestMove;
                }
            }
            return bestMove;
        }
    }

    private int EvaluateBoard(Board board)
    {
        if (board.IsDraw())
        {
            return 0;
        }

        if (board.IsInCheckmate())
        {
            int newFactor = board.IsWhiteToMove ? 1 : -1;
            return -10000 * newFactor;
        }
        int eval = 0;
        foreach (ChessChallenge.API.PieceList pieces in board.GetAllPieceLists())
        {
            int factor = pieces.IsWhitePieceList ? 1 : -1;
            eval += pieceValues[(int)pieces.TypeOfPieceInList] * factor * pieces.Count;
        }

        switch (currentGamestate)
        {
            case GameState.Opening:
                break;
            case GameState.MiddleGame:
                break;
            case GameState.EndGame:
                break;
        }
        return eval;
    }
}
