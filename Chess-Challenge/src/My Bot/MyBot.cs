﻿using ChessChallenge.API;
using ChessChallenge.Chess;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;
using PieceList = ChessChallenge.Chess.PieceList;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 325, 500, 900, 10000 };
    private int maxDepth = 3;
    private Random rnd = new Random();
    private int[][][] blackValueBoard = new[]
    {
        //none
        new []
        {
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  0,  0,  0,  0,  0,  0,  0,  0}
        },
        //Pawn
        new []
        {
            new []{ 60, 60, 60, 60, 60, 60, 60, 60},
            new []{ 50, 50, 50, 50, 50, 50, 50, 50},
            new []{ 10, 10, 20, 30, 30, 20, 10, 10},
            new []{  5,  5, 10, 25, 25, 10,  5,  5},
            new []{  0,  0,  0, 20, 20,  0,  0,  0},
            new []{  5, -5,-10,  0,  0,-10, -5,  5},
            new []{  5, 10, 10,-20,-20, 10, 10,  5},
            new []{  0,  0,  0,  0,  0,  0,  0,  0}
        },
        //Knight
        new []
        {
            new []{-50,-40,-30,-30,-30,-30,-40,-50},
            new []{-40,-20,  0,  0,  0,  0,-20,-40},
            new []{-30,  0, 10, 15, 15, 10,  0,-30},
            new []{-30,  5, 15, 20, 20, 15,  5,-30},
            new []{-30,  0, 15, 20, 20, 15,  0,-30},
            new []{-30,  5, 10, 15, 15, 10,  5,-30},
            new []{-40,-20,  0,  5,  5,  0,-20,-40},
            new []{-50,-40,-30,-30,-30,-30,-40,-50}
        },
        //Bishop
        new []
        {
            new []{-20,-10,-10,-10,-10,-10,-10,-20},
            new []{-10,  0,  0,  0,  0,  0,  0,-10},
            new []{-10,  0,  5, 10, 10,  5,  0,-10},
            new []{-10,  5,  5, 10, 10,  5,  5,-10},
            new []{-10,  0, 10, 10, 10, 10,  0,-10},
            new []{-10, 10, 10, 10, 10, 10, 10,-10},
            new []{-10,  5,  0,  0,  0,  0,  5,-10},
            new []{-20,-10,-10,-10,-10,-10,-10,-20}
        },
        //Rook
        new[]
        {
            new []{  0,  0,  0,  0,  0,  0,  0,  0},
            new []{  5, 10, 10, 10, 10, 10, 10,  5},
            new []{ -5,  0,  0,  0,  0,  0,  0, -5},
            new []{ -5,  0,  0,  0,  0,  0,  0, -5},
            new []{ -5,  0,  0,  0,  0,  0,  0, -5},
            new []{ -5,  0,  0,  0,  0,  0,  0, -5},
            new []{ -5,  0,  0,  0,  0,  0,  0, -5},
            new []{  0,  0,  0,  5,  5,  0,  0,  0}
        },
        //Queen
        new[]
        {
            new []{-20,-10,-10, -5, -5,-10,-10,-20},
            new []{-10,  0,  0,  0,  0,  0,  0,-10},
            new []{-10,  0,  5,  5,  5,  5,  0,-10},
            new []{ -5,  0,  5,  5,  5,  5,  0, -5},
            new []{  0,  0,  5,  5,  5,  5,  0,  0},
            new []{-10,  5,  5,  5,  5,  5,  0,-10},
            new []{-10,  0,  5,  0,  0,  0,  0,-10},
            new []{-20,-10,-10, -5, -5,-10,-10,-20}
        },
        //King
        new[]
        {
            new []{-30,-40,-40,-50,-50,-40,-40,-30},
            new []{-30,-40,-40,-50,-50,-40,-40,-30},
            new []{-30,-40,-40,-50,-50,-40,-40,-30},
            new []{-30,-40,-40,-50,-50,-40,-40,-30},
            new []{-20,-30,-30,-40,-40,-30,-30,-20},
            new []{-10,-20,-20,-20,-20,-20,-20,-10},
            new []{ 20, 20,  0,  0,  0,  0, 20, 20},
            new []{ 20, 30, 10,  0,  0, 10, 30, 20}
        }
    };
    

    private int[][][] whiteValueBoard;

    public MyBot()
    {
        whiteValueBoard = new[]
        {
            blackValueBoard[0],
            blackValueBoard[1].Reverse().ToArray(),
            blackValueBoard[2],
            blackValueBoard[3].Reverse().ToArray(),
            blackValueBoard[4].Reverse().ToArray(),
            blackValueBoard[5],
            blackValueBoard[6].Reverse().ToArray(),
        };
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Move moveToPlay = allMoves[rnd.Next(allMoves.Length-1)];
        int highestValue = 1337420;
        /*
        double pieceCounter = 0;

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
                    pieceCounter += pieces.Count*1.5;
                    break;
                case PieceType.Rook:
                    pieceCounter += pieces.Count*2;
                    break;
                case PieceType.Queen:
                    pieceCounter += pieces.Count*3;
                    break;
                case PieceType.King:
                    break;
            }
        }
        if (pieceCounter <= 4)
        {
            maxDepth = maxDepth+1;
        }
        if(pieceCounter <= 2)
        {
            maxDepth = maxDepth+2;
        }
        */
        Parallel.ForEach(allMoves, move =>
        {
            Board tmpBoard = Board.CreateBoardFromFEN(board.GetFenString());
            tmpBoard.MakeMove(move);
            int tmp = CalcMoveAlphaBeta(tmpBoard, -100000, 100000, 0);
            tmpBoard.UndoMove(move);
            if ((board.IsWhiteToMove && tmp > highestValue) || (!board.IsWhiteToMove && tmp < highestValue) || highestValue == 1337420)
            {
                moveToPlay = move;
                highestValue = tmp;
            }
        });
        Console.WriteLine(highestValue);
        return moveToPlay;
    }

    private int CalcMoveAlphaBeta(Board board, int alpha, int beta, int depthCounter)
    {
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? -66666 : 66666;
        }

        if (board.IsDraw())
        {
            return 0;
        }

        if (depthCounter == maxDepth)
        {
            return EvaluateBoard(board);
        }

        Move[] newGameMoves = board.GetLegalMoves();

        if (board.IsWhiteToMove)
        {
            int bestMove = int.MinValue;
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
            var bestMove = int.MaxValue;
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
        int eval = 0;
        ChessChallenge.API.PieceList[] pieces = board.GetAllPieceLists();

        for (int i = 0; i < pieces.Length; i++)
        {
            int factor = pieces[i].IsWhitePieceList ? 1 : -1;
            int[][] pieceValueBoard = pieces[i].IsWhitePieceList ? whiteValueBoard[(int)pieces[i].TypeOfPieceInList] : blackValueBoard[(int)pieces[i].TypeOfPieceInList];
            for (int j = 0; j < pieces[i].Count; j++)
            {
                Square square = pieces[i][j].Square;
                eval += (pieceValueBoard[square.Rank][square.File] + pieceValues[(int)pieces[i][j].PieceType]) * factor;
            }
        }
        return eval;
    }
}
                                                                                                                                                                                                                                                        