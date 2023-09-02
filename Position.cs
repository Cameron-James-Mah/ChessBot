using System;
using System.Numerics;
using System.Transactions;
using System.Xml.Linq;
using static ChessBot.Program;
using static Globals;
public class Position
{
    public static bool isSquareAttacked(int source, ulong eBishop, ulong eRook, ulong eKnight, ulong eQueen, ulong ePawn, ulong eKing, ulong allPieces, char color)
    {
        ulong blockers = 0;
        ulong sourceBit = (ulong)1 << source;
        //bishop rays
        ulong SWmoves = bishopAttacks[source, 0];
        ulong SEmoves = bishopAttacks[source, 1];
        ulong NWmoves = bishopAttacks[source, 2];
        ulong NEmoves = bishopAttacks[source, 3];
        int lsb = 0;
        eBishop |= eQueen;
        eRook |= eQueen;
        blockers = SWmoves & allPieces;
        if (blockers > 0)
        {
            lsb = 63 - BitOperations.LeadingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eBishop) > 0)
            {
                return true;
            }
        }
        blockers = SEmoves & allPieces;
        if (blockers > 0)
        {
            lsb = 63 - BitOperations.LeadingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eBishop) > 0)
            {
                return true;
            }
        }
        blockers = NWmoves & allPieces;
        if (blockers > 0)
        {
            lsb = BitOperations.TrailingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eBishop) > 0)
            {
                return true;
            }
        }
        blockers = NEmoves & allPieces;
        if (blockers > 0)
        {
            lsb = BitOperations.TrailingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eBishop) > 0)
            {
                return true;
            }
        }

        //rook rays
        ulong Smoves = rookAttacks[source, 0];
        ulong Emoves = rookAttacks[source, 1];
        ulong Wmoves = rookAttacks[source, 2];
        ulong Nmoves = rookAttacks[source, 3];
        blockers = Smoves & allPieces;
        if (blockers > 0)
        {
            lsb = 63 - BitOperations.LeadingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eRook) > 0)
            {
                return true;
            }
        }
        blockers = Emoves & allPieces;
        if (blockers > 0)
        {
            lsb = 63 - BitOperations.LeadingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eRook) > 0)
            {
                return true;
            }
        }
        blockers = Wmoves & allPieces;
        if (blockers > 0)
        {
            lsb = BitOperations.TrailingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eRook) > 0)
            {
                return true;
            }
        }
        blockers = Nmoves & allPieces;
        if (blockers > 0)
        {
            lsb = BitOperations.TrailingZeroCount(blockers);
            ulong enemy = (ulong)1 << lsb; //first blocker in ray
            if ((enemy & eRook) > 0)
            {
                return true;
            }
        }
        //knight jumps
        ulong knightMoves = knightAttacks[source];
        if ((knightMoves & eKnight) > 0)
        {
            return true;
        }
        //pawn attacks

        if (color == 'b') //white pawn attacks for black king
        {
            ulong pawnAttacksE = ePawn << 9;
            pawnAttacksE &= notHFile;
            ulong pawnAttacksW = ePawn << 7;
            pawnAttacksW &= notAFile;
            if (((pawnAttacksE | pawnAttacksW) & sourceBit) > 0)
            {
                return true;
            }
        }
        else if (color == 'w') //black pawn attacks for white king
        {
            ulong pawnAttacksE = ePawn >> 9;
            pawnAttacksE &= notAFile;
            ulong pawnAttacksW = ePawn >> 7;
            pawnAttacksW &= notHFile;
            if (((pawnAttacksE | pawnAttacksW) & sourceBit) > 0)
            {
                return true;
            }
        }

        //king attacks
        if ((kingAttacks[BitOperations.TrailingZeroCount(eKing)] & sourceBit) > (ulong)0)
        {
            return true;
        }

        return false;
    }
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
            char[] tempBoard = new char[64];
            board.CopyTo(tempBoard, 0);
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                }
                if (moves[i].castleFrom >= 0) //update rook position when castling
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                }
                tempBoard[63 - moves[i].source] = ' ';
                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(bKing);
                if (!isSquareAttacked(kingSource, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
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
                    ulong temp = perft(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w');
                    nodes += temp;
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                }
                board.CopyTo(tempBoard, 0);
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
            char[] tempBoard = new char[64];
            board.CopyTo(tempBoard, 0);
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                }
                if (moves[i].castleFrom >= 0)
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                }
                tempBoard[63 - moves[i].source] = ' ';

                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(wKing);
                if (!isSquareAttacked(kingSource, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w'))
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

                    ulong temp = perft(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b');
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                    nodes += temp;
                }
                board.CopyTo(tempBoard, 0);
            }
        }
        return nodes;
    }
    /*
    public static ulong perft2(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
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
            char[] tempBoard = new char[64];
            board.CopyTo(tempBoard, 0);
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                }
                if (moves[i].castleFrom >= 0) //update rook position when castling
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                }
                tempBoard[63 - moves[i].source] = ' ';
                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(bKing);
                if (!isSquareAttacked(kingSource, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
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
                    ulong temp = perft2(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w');
                    nodes += temp;
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                }
                board.CopyTo(tempBoard, 0);
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
            char[] tempBoard = new char[64];
            board.CopyTo(tempBoard, 0);
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                }
                if (moves[i].castleFrom >= 0)
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                }
                tempBoard[63 - moves[i].source] = ' ';

                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(wKing);
                if (!isSquareAttacked(kingSource, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w'))
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

                    ulong temp = perft2(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b');
                    if (depth == perftValue)
                    {
                        Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest] + ": " + temp);
                    }
                    nodes += temp;
                }
                board.CopyTo(tempBoard, 0);
            }
        }
        return nodes;
    }
    */
   

    //white wants max eval, black wants min eval
    //maybe at final depth, do another minimax for only captures to fully calculate trade of pieces
    public static int minimax(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
                                         ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
                                         ulong allPieces, ulong empty, char[] board, ulong whitePieces, ulong blackPieces,
                                         ulong castleRights, ulong enPassant, char color, int alpha, int beta, ulong currHash)
    {
        if (depth == 0)
        {
            return eval(board);
        }
        if (color == 'b') //minimizing, black to move
        {
            //Console.WriteLine("---------------------------------------------");
            //Console.WriteLine(currHash);
            //Board.printBoard(board);
            if(blackTable.ContainsKey(currHash))
            {
                //if true value return true value
                if(blackTable[currHash].depth >= depth)
                {
                    
                    beta = Math.Min(beta, blackTable[currHash].value);
                    //Console.WriteLine("Current key: " + currHash + "Table key: " + blackTable[currHash].key);
                    /*
                    Console.WriteLine("Current board: " + currHash + " Current Depth: " + depth);
                    Board.printBoard(board);
                    Console.WriteLine("Table board: " + blackTable[currHash].key + " Table Depth: " + blackTable[currHash].depth);
                    Board.printBoard(blackTable[currHash].board);
                    Console.WriteLine("---------------------------------------------");*/
                    
                    if (beta <= alpha)
                    {
                        return beta;
                    }
                }
                
                
            }
            int minEval = int.MaxValue;
            List<Move> moves = new List<Move>();
            MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
            MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
            MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);
            //char[] tempBoard = new char[64];
            //board.CopyTo(tempBoard, 0);
            bool moved = false;
            for (int i = 0; i < moves.Count; i++)
            {
                char[] tempBoard = new char[64];
                board.CopyTo(tempBoard, 0);
                ulong newHash = currHash;
                newHash ^= Zobrist.getHash(63 - moves[i].source, board[63 - moves[i].source]);
                if (board[63 - moves[i].dest] != ' ') //if capturing then update hash
                {
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].dest]);
                }
                if (moves[i].promotion != ' ')
                {
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                    newHash^=Zobrist.getHash(63 - moves[i].dest, moves[i].promotion);
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    newHash^=Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].source]);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    newHash^=Zobrist.getHash(63 - moves[i].capPassant, board[63 - moves[i].capPassant]);
                }
                if (moves[i].castleFrom >= 0) //update rook position when castling
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].castleTo, board[63 - moves[i].castleFrom]);
                    newHash ^= Zobrist.getHash(63 - moves[i].castleFrom, board[63 - moves[i].castleFrom]);
                }
                tempBoard[63 - moves[i].source] = ' ';
                
                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(bKing);
                if (!isSquareAttacked(kingSource, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
                {
                    moved = true;
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
                    //newHash ^= blackHash;
                    //newHash = Zobrist.computeHash(tempBoard);
                    /*
                    Console.WriteLine(Zobrist.computeHash(tempBoard));
                    Console.WriteLine(newHash);
                    Console.WriteLine();*/
                    /*
                    ulong nh = Zobrist.computeHash(tempBoard);
                    Console.WriteLine(newHash);
                    Console.WriteLine(nh);
                    Console.WriteLine("---------------------------------------------");
                    /*
                    Console.WriteLine(newHash);
                    Console.WriteLine(currHash);
                    Console.WriteLine();*/
                    int temp = minimax(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', alpha, beta, newHash);
                    minEval = Math.Min(temp, minEval);
                    beta = Math.Min(beta, minEval);

                    if (beta <= alpha)
                    {
                        //if black table has entry or entry depth is below current depth
                        if (!blackTable.ContainsKey(currHash) || blackTable[currHash].depth < depth)
                        {
                            //Board.printBoard(board);
                            /*
                            Board.printBoard(board);
                            Console.WriteLine();
                            Console.WriteLine("---------------------------------------------"); */
                            //Console.WriteLine(currHash);
                            /*
                            if (blackTable.ContainsKey(currHash))
                            {
                                Console.WriteLine("Current board: " + currHash + " Current Depth: " + depth);
                                Board.printBoard(board);
                                Console.WriteLine("Table board: " + blackTable[currHash].key + " Table Depth: " + blackTable[currHash].depth);
                                Board.printBoard(blackTable[currHash].board);
                                Console.WriteLine("---------------------------------------------");
                            }*/

                            //blackTable.Remove(currHash);
                            //blackTable.Add(currHash, new Entry(minEval, depth, moves[i], currHash, board));
                            blackTable[currHash] = new Entry(minEval, depth, moves[i]);
                            //Board.printBoard(blackTable[currHash].board);
                            //Console.WriteLine("---------------------------------------------");
                        }
                        return minEval;
                    }
                }
                
                //Console.WriteLine(newHash);
            }
            //Console.WriteLine("\n\n");
            if (!moved) //no valid moves so white must have checkmated black CHANGE THIS LATER TO ACCOUNT FOR STALEMATE
            {
                minEval = int.MaxValue;
            }
            //Board.printBoard(board);
            return minEval;
        }
        else //maximizing, white to move
        {
            //Board.printBoard(board);
            if (whiteTable.ContainsKey(currHash) && whiteTable[currHash].depth >= depth)
            {
                
                alpha = Math.Max(alpha, whiteTable[currHash].value);
                /*
                Console.WriteLine("Current Hash: " + currHash);
                Board.printBoard(board);
                Console.WriteLine("Table board: " + whiteTable[currHash].key);
                Board.printBoard(whiteTable[currHash].board);
                Console.WriteLine("---------------------------------------------");*/
                
                if (beta <= alpha)
                {
                    return alpha;
                }
            }
            int maxEval = int.MinValue;
            List<Move> moves = new List<Move>();
            MoveGen.getPawnMoves(wPawn, empty, ref moves, blackPieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, blackPieces, wKnight, empty);
            MoveGen.getBishopMoves(ref moves, blackPieces, wBishop, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wRook, allPieces);
            MoveGen.getBishopMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getKingMoves(ref moves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);
            //char[] tempBoard = new char[64];
            //board.CopyTo(tempBoard, 0);
            bool moved = false;
            for (int i = 0; i < moves.Count; i++)
            {
                char[] tempBoard = new char[64];
                board.CopyTo(tempBoard, 0);
                ulong newHash = currHash;
                newHash ^= Zobrist.getHash(63 - moves[i].source, board[63 - moves[i].source]);
                if (board[63 - moves[i].dest] != ' ') //if capturing then update hash
                {
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].dest]);
                }
                if (moves[i].promotion != ' ')
                {
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, moves[i].promotion);
                    tempBoard[63 - moves[i].dest] = moves[i].promotion;
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].source]);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].capPassant, board[63 - moves[i].capPassant]);
                }
                if (moves[i].castleFrom >= 0)
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].castleTo, board[63 - moves[i].castleFrom]);
                    newHash ^= Zobrist.getHash(63 - moves[i].castleFrom, board[63 - moves[i].castleFrom]);
                }
                
                tempBoard[63 - moves[i].source] = ' ';

                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                int kingSource = BitOperations.TrailingZeroCount(wKing);
                if (!isSquareAttacked(kingSource, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w'))
                {
                    moved = true;
                    ulong newEnPassant = 0;
                    ulong newCastleRights = castleRights; //I THINK THIS WAS MY ISSUE, BEFORE I HAD CASTLERIGHTS = 0 AND I ONLY CHANGED CASTLERIGHTS IF I MYSELF CASTLED, SO A NON CASTLING MOVE WOULD EFFECTIVELY WIPE ALL CASTLERIGHTS
                                                          //Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest + " Promotion: " + moves[i].promotion);
                    if (moves[i].enPassant)
                    {
                        newEnPassant = (ulong)1 << ((moves[i].source + moves[i].dest) / 2);
                    }
                    else 
                    {
                        newEnPassant = 0;
                    }
                    if ((castleRights & ((ulong)1 << moves[i].source)) > 0) //if castling move then update castleRights, I THINK THIS IS MY ISSUE 
                    {
                        newCastleRights = castleRights ^ ((ulong)1 << moves[i].source);
                    }
                    //newHash ^= whiteHash;
                    //newHash = Zobrist.computeHash(tempBoard);
                    /*
                    Console.WriteLine(Zobrist.computeHash(tempBoard));
                    Console.WriteLine(newHash);
                    Console.WriteLine();*/
                    /*
                    ulong nh = Zobrist.computeHash(tempBoard);
                    Console.WriteLine(newHash);
                    Console.WriteLine(nh);
                    Console.WriteLine("---------------------------------------------");*/
                    /*
                    Console.WriteLine(newHash);
                    Console.WriteLine(currHash);
                    Console.WriteLine();*/

                    int temp = minimax(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', alpha, beta, newHash);
                    maxEval = Math.Max(maxEval, temp);
                    alpha = Math.Max(maxEval, alpha);
                    if(beta <= alpha)
                    {
                        //if black table has entry or entry depth is below current depth
                        if (!whiteTable.ContainsKey(currHash) || whiteTable[currHash].depth < depth)
                        {
                            //Board.printBoard(board);
                            //Console.WriteLine("---------------------------------------------");
                            //whiteTable.Remove(currHash);
                            //whiteTable.Add(currHash, new Entry(maxEval, depth, moves[i], currHash, board));
                            whiteTable[currHash] = new Entry(maxEval, depth, moves[i]);
                        }
                        return maxEval;
                    }
                }
                
            }
            //true value
            if (!moved) //no valid moves so black must have checkmated white
            {
                return int.MinValue;
            }
            return maxEval;
        }
    }

    /*
     * Considering:
     * 
     * material
     * central pieces
     * mobility
     * king safety
     * 
     * endgame seperatve eval:
     * enemy king on corner of board
     * maybe add small bonus for having king close to enemy king to help close out endgames
     * 
     * */
    public static int eval(char[] board)
    {
        int pawn = 100; //P/p
        int knight = 320; //N/n
        int bishop = 330; //B/b
        int rook = 500; //R/r
        int queen = 900; //Q/q
        int king = 2000; //K/k
        //do i need a  king eval?
        int whiteEval = 0;
        int blackEval = 0;


        for(int i = 0; i < board.Length; i++)
        {
            //white pieces
            if (board[i] == 'P')
            {
                whiteEval += pawn + pawnSquaresW[i];
            }
            else if (board[i] == 'N')
            {
                whiteEval += knight + knightSquares[i];
            }
            else if (board[i] == 'B')
            {
                whiteEval += bishop + bishopSquaresW[i];
            }
            else if (board[i] == 'R')
            {
                whiteEval += rook + rookSquaresW[i];
            }
            else if (board[i] == 'Q')
            {
                whiteEval += queen + queenSquares[i];
            }
            else if (board[i] == 'K')
            {
                whiteEval += king + kingSquaresMiddleW[i];
            }

            //black pices
            else if (board[i] == 'p')
            {
                blackEval += pawn + pawnSquaresB[i];
            }
            else if (board[i] == 'n')
            {
                blackEval += knight + knightSquares[i];
            }
            else if (board[i] == 'b')
            {
                blackEval += bishop + bishopSquaresB[i];
            }
            else if (board[i] == 'r')
            {
                blackEval += rook + rookSquaresB[i];
            }
            else if (board[i] == 'q')
            {
                blackEval += queen + queenSquares[i];
            }
            else if (board[i] == 'k')
            {
                blackEval += king + kingSquaresMiddleB[i];
            }

        }

        return whiteEval-blackEval;
    }



}
