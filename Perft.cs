using System;
using static ChessBot.Program;
using System.Numerics;
using static Globals;

public class Perft
{
    //original perft function, made as simple as possible to make sure move generation works properly

    //perft function for testing my move generation is correct, also testing performance 
    public static ulong perft(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
                                         ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
                                         ulong allPieces, ulong empty, char[] board, ulong whitePieces, ulong blackPieces,
                                         ulong castleRights, ulong enPassant, char color)
    {

        ulong nodes = 0;
        if (depth == 0)
        {
            return 1;
        }
        if (color == 'b')
        {
            List<Move> moves = new List<Move>();
            MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
            MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
            MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);

            for (int i = 0; i < moves.Count; i++)
            {
                ulong bPawn2 = bPawn; ulong bRook2 = bRook; ulong bKnight2 = bKnight; ulong bBishop2 = bBishop; ulong bQueen2 = bQueen; ulong bKing2 = bKing;
                ulong wPawn2 = wPawn; ulong wRook2 = wRook; ulong wKnight2 = wKnight; ulong wBishop2 = wBishop; ulong wQueen2 = wQueen; ulong wKing2 = wKing;
                char[] tempBoard = (char[])board.Clone();
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, moves[i].promotion, moves[i].dest);
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].dest);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].capPassant], moves[i].capPassant);
                }
                if (moves[i].castleFrom >= 0) //update rook position when castling
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].castleFrom], moves[i].castleTo);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].castleFrom], moves[i].castleFrom);
                }
                tempBoard[63 - moves[i].source] = ' ';
                Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].source);

                allPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2 | wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                empty = ~allPieces;
                whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;

                int kingSource = BitOperations.TrailingZeroCount(bKing2);
                if (!Position.isSquareAttacked(kingSource, wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
                {

                    ulong newEnPassant = 0;
                    ulong newCastleRights = castleRights;
                    //Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest + " Promotion: " + moves[i].promotion);
                    if (moves[i].enPassant)
                    {
                        newEnPassant = (ulong)1 << ((moves[i].source + moves[i].dest) / 2);
                    }
                    else
                    {
                        newEnPassant = 0;
                    }
                    if ((castleRights & ((ulong)1 << moves[i].source)) > 0)
                    {
                        newCastleRights = castleRights ^ ((ulong)1 << moves[i].source);
                    }
                    //validMoves.Add(moves[i]); 
                    ulong temp = perft(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w');
                    nodes += temp;
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                }
            }
        }
        else
        {
            List<Move> moves = new List<Move>();
            MoveGen.getPawnMoves(wPawn, empty, ref moves, blackPieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, blackPieces, wKnight, empty);
            MoveGen.getBishopMoves(ref moves, blackPieces, wBishop, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wRook, allPieces);
            MoveGen.getBishopMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getKingMoves(ref moves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);
            //char[] tempBoard = new char[64];
            for (int i = 0; i < moves.Count; i++)
            {
                ulong bPawn2 = bPawn; ulong bRook2 = bRook; ulong bKnight2 = bKnight; ulong bBishop2 = bBishop; ulong bQueen2 = bQueen; ulong bKing2 = bKing;
                ulong wPawn2 = wPawn; ulong wRook2 = wRook; ulong wKnight2 = wKnight; ulong wBishop2 = wBishop; ulong wQueen2 = wQueen; ulong wKing2 = wKing;
                char[] tempBoard = (char[])board.Clone();
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, moves[i].promotion, moves[i].dest);
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].dest);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].capPassant], moves[i].capPassant);
                }
                if (moves[i].castleFrom >= 0)
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].castleFrom], moves[i].castleTo);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].castleFrom], moves[i].castleFrom);
                }
                tempBoard[63 - moves[i].source] = ' ';
                Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].source);

                allPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2 | wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                empty = ~allPieces;
                whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;
                int kingSource = BitOperations.TrailingZeroCount(wKing2);
                if (!Position.isSquareAttacked(kingSource, bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
                {
                    ulong newEnPassant = 0;
                    ulong newCastleRights = castleRights; //I THINK THIS WAS MY ISSUE, BEFORE I HAD CASTLERIGHTS = 0 AND I ONLY CHANGED CASTLERIGHTS IF I MYSELF CASTLED, SO A NON CASTLING MOVE WOULD EFFECTIVELY WIPE ALL CASTLERIGHTS
                                                          //Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest + " Promotion: " + moves[i].promotion);
                    if (moves[i].enPassant)
                    {
                        newEnPassant = (ulong)1 << ((moves[i].source + moves[i].dest) / 2);
                    }
                    else //RECENTLY ADDED THIS ON A WHIM, DOUBLE CHECK
                    {
                        newEnPassant = 0;
                    }
                    if ((castleRights & ((ulong)1 << moves[i].source)) > 0) //if castling move then update castleRights, I THINK THIS IS MY ISSUE 
                    {
                        newCastleRights = castleRights ^ ((ulong)1 << moves[i].source);
                        //printBitBoard(newCastleRights);
                    }

                    ulong temp = perft(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b');
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                    nodes += temp;
                }
            }
        }
        return nodes;
    }
}