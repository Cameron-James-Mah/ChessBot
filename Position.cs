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
                /*
                Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                    ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);*/
                /*
                Board.updateBoardsFromMove(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, ref allPieces, ref empty, ref whitePieces, ref blackPieces, board[63 - moves[i].source], board[63 - moves[i].dest], moves[i].source, moves[i].dest);*/
                int kingSource = BitOperations.TrailingZeroCount(bKing2);
                if (!isSquareAttacked(kingSource, wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
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
                if (!isSquareAttacked(kingSource, bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
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
<<<<<<< Updated upstream
    
   
=======


>>>>>>> Stashed changes

    //white wants max eval, black wants min eval
    public static int quiescence(int alpha, int beta, char[] board)
    {

        return 0;
    }
    public static int minimax(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
                                         ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
                                         ulong allPieces, ulong empty, char[] board, ulong whitePieces, ulong blackPieces,
                                         ulong castleRights, ulong enPassant, char color, int alpha, int beta, ulong currHash)
    {
        if (depth == 0)
        {
            return eval(board, color);
        }
<<<<<<< Updated upstream
        if(repetition.ContainsKey(currHash) && repetition[currHash] == 2) //3 move repetition
=======
        if (repetition.ContainsKey(currHash) && repetition[currHash] == 2) //3 move repetition
>>>>>>> Stashed changes
        {
            return 0;
        }
        List<Move> moves = new List<Move>();
        if (color == 'b') //minimizing, black to move
        {
            //Console.WriteLine("---------------------------------------------");
            //Console.WriteLine(currHash);
            //Board.printBoard(board);
            if (blackTable.ContainsKey(currHash))
            {
                //if true value return true value
                if (blackTable[currHash].depth >= depth)
                {
                    if (blackTable[currHash].trueValue)
                    {
                        return blackTable[currHash].value;
                    }
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
            int minEval = 60000; //arbitrary number
            int kingSquare = BitOperations.TrailingZeroCount(bKing);
            MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
            MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
            MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);
            ulong enemyPawnAttacks = wPawn << 7 & notAFile;
            enemyPawnAttacks |= wPawn << 9 & notHFile;
            orderMoves(ref moves, enemyPawnAttacks, ref board);
            //char[] tempBoard = new char[64];
            //board.CopyTo(tempBoard, 0);
            bool moved = false;
            char[] tempBoard = new char[64];
            for (int i = 0; i < moves.Count; i++)
            {
                ulong bPawn2 = bPawn; ulong bRook2 = bRook; ulong bKnight2 = bKnight; ulong bBishop2 = bBishop; ulong bQueen2 = bQueen; ulong bKing2 = bKing;
                ulong wPawn2 = wPawn; ulong wRook2 = wRook; ulong wKnight2 = wKnight; ulong wBishop2 = wBishop; ulong wQueen2 = wQueen; ulong wKing2 = wKing;
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
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, moves[i].promotion);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, moves[i].promotion, moves[i].dest);
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].source]);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].dest);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].capPassant, board[63 - moves[i].capPassant]);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].capPassant], moves[i].capPassant);
                }
                if (moves[i].castleFrom >= 0) //update rook position when castling
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].castleTo, board[63 - moves[i].castleFrom]);
                    newHash ^= Zobrist.getHash(63 - moves[i].castleFrom, board[63 - moves[i].castleFrom]);
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
                if (!isSquareAttacked(kingSource, wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
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
                    int temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', alpha, beta, newHash);
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
                            blackTable[currHash] = new Entry(minEval, depth, moves[i], false);
                            //Board.printBoard(blackTable[currHash].board);
                            //Console.WriteLine("---------------------------------------------");
                        }
                        return minEval;
                    }
                }

                //Console.WriteLine(newHash);
            }
            //Console.WriteLine("\n\n");
            if (!moved) //no valid moves 
            {
                //int kingSource = BitOperations.TrailingZeroCount(bKing);
                if (isSquareAttacked(kingSquare, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b')) //checkmate
                {
                    minEval = 50000 + depth; //50000 is arbitrary number to represent win, addition/subtraction for depth is to give inclination for faster wins
                }
                else //stalemate
                {
                    return 0;
                }
            }
            //true value node here?
            if (!blackTable.ContainsKey(currHash) || blackTable[currHash].depth < depth)
            {
                blackTable[currHash] = new Entry(minEval, depth, new Move(), true);
            }
            //Board.printBoard(board);
            return minEval;
        }
        else //maximizing, white to move
        {
            //Board.printBoard(board);
            if (whiteTable.ContainsKey(currHash) && whiteTable[currHash].depth >= depth)
            {

                if (whiteTable[currHash].trueValue)
                {
                    return whiteTable[currHash].value;
                }
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
            int maxEval = -60000; //arbitrary number
            int kingSquare = BitOperations.TrailingZeroCount(wKing);
            MoveGen.getPawnMoves(wPawn, empty, ref moves, blackPieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, blackPieces, wKnight, empty);
            MoveGen.getBishopMoves(ref moves, blackPieces, wBishop, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wRook, allPieces);
            MoveGen.getBishopMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getKingMoves(ref moves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);
            //char[] tempBoard = new char[64];
            //board.CopyTo(tempBoard, 0);
            ulong enemyPawnAttacks = bPawn >> 7 & notHFile;
            enemyPawnAttacks |= bPawn >> 9 & notAFile;
            orderMoves(ref moves, enemyPawnAttacks, ref board);
            bool moved = false;

            char[] tempBoard = new char[64];
            for (int i = 0; i < moves.Count; i++)
            {
                ulong bPawn2 = bPawn; ulong bRook2 = bRook; ulong bKnight2 = bKnight; ulong bBishop2 = bBishop; ulong bQueen2 = bQueen; ulong bKing2 = bKing;
                ulong wPawn2 = wPawn; ulong wRook2 = wRook; ulong wKnight2 = wKnight; ulong wBishop2 = wBishop; ulong wQueen2 = wQueen; ulong wKing2 = wKing;
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
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, moves[i].promotion, moves[i].dest);
                }
                else
                {
                    tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                    newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].source]);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest);
                    Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].dest);
                }
                if (moves[i].capPassant >= 0)
                {
                    tempBoard[63 - moves[i].capPassant] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].capPassant, board[63 - moves[i].capPassant]);
                    Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].capPassant], moves[i].capPassant);
                }
                if (moves[i].castleFrom >= 0)
                {
                    tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                    tempBoard[63 - moves[i].castleFrom] = ' ';
                    newHash ^= Zobrist.getHash(63 - moves[i].castleTo, board[63 - moves[i].castleFrom]);
                    newHash ^= Zobrist.getHash(63 - moves[i].castleFrom, board[63 - moves[i].castleFrom]);
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
                if (!isSquareAttacked(kingSource, bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
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

                    int temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', alpha, beta, newHash);
                    maxEval = Math.Max(maxEval, temp);
                    alpha = Math.Max(maxEval, alpha);
                    if (beta <= alpha)
                    {
                        //if black table has entry or entry depth is below current depth
                        if (!whiteTable.ContainsKey(currHash) || whiteTable[currHash].depth < depth)
                        {
                            //Board.printBoard(board);
                            //Console.WriteLine("---------------------------------------------");
                            //whiteTable.Remove(currHash);
                            //whiteTable.Add(currHash, new Entry(maxEval, depth, moves[i], currHash, board));
                            whiteTable[currHash] = new Entry(maxEval, depth, moves[i], false);
                        }
                        return maxEval;
                    }
                }

            }

            if (!moved) //no valid moves 
            {
                //int kingSource = BitOperations.TrailingZeroCount(wKing); //THIS IS WHERE THE ISSUE IS
                if (isSquareAttacked(kingSquare, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w')) //black must have checkmated white
                {
                    maxEval = -50000 - depth; //50000 is arbitrary number to represent win, addition/subtraction for depth is to give inclination for faster wins
                    //Board.printBoard(board);
                    //Console.WriteLine(kingSquare);
                    /*
                    Console.WriteLine("Queen: ");
                    printBitBoard(bQueen);*/
                    //Console.WriteLine(isSquareAttacked(kingSource, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w'));
                }
                else //stalemate
                {
                    //Board.printBoard(board);
                    return 0;
                }

            }
            if (!whiteTable.ContainsKey(currHash) || whiteTable[currHash].depth < depth)
            {
                whiteTable[currHash] = new Entry(maxEval, depth, new Move(), true); //true value 
            }

            return maxEval;
        }
    }


    public static int eval(char[] board, char color)
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
        int whiteMaterial = 0; //material not including pawns/king
        int blackMaterial = 0; //material not including pawns/king
        int wKingIdx = 0; //index of white king
        int bKingIdx = 0; //index of black king
        //rank and file of kings, bit inconvinient since 1d array and i also dont want to use modulus operation
        int wRank = 0;
        int wFile = 0;
        int bRank = 0;
        int bFile = 0;
        int rank = 0;
        int file = 0;
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == ' ')
            {
                continue;
            }
            //white pieces
            if (board[i] == 'P')
            {
                whiteEval += pawn + pawnSquaresW[i];
            }
            else if (board[i] == 'N')
            {
                whiteEval += knight + knightSquares[i];
                whiteMaterial += knight;
            }
            else if (board[i] == 'B')
            {
                whiteEval += bishop + bishopSquaresW[i];
                whiteMaterial += bishop;
            }
            else if (board[i] == 'R')
            {
                whiteEval += rook + rookSquaresW[i];
                whiteMaterial += rook;
            }
            else if (board[i] == 'Q')
            {
                whiteEval += queen + queenSquares[i];
                whiteMaterial += queen;
            }
            else if (board[i] == 'K')
            {
                wKingIdx = i;
                wFile = file;
                wRank = rank;
            }

            //black pices
            else if (board[i] == 'p')
            {
                blackEval += pawn + pawnSquaresB[i];
            }
            else if (board[i] == 'n')
            {
                blackEval += knight + knightSquares[i];
                blackMaterial += knight;
            }
            else if (board[i] == 'b')
            {
                blackEval += bishop + bishopSquaresB[i];
                blackMaterial += bishop;
            }
            else if (board[i] == 'r')
            {
                blackEval += rook + rookSquaresB[i];
                blackMaterial += rook;
            }
            else if (board[i] == 'q')
            {
                blackEval += queen + queenSquares[i];
                blackMaterial += queen;
            }

            else if (board[i] == 'k')
            {
                bKingIdx = i;
                bFile = file;
                bRank = rank;
            }
            file++;
            if (file == 8)
            {
                rank++;
                file = 0;
            }
        }
        if (blackMaterial > 900 || whiteMaterial > 900)
        {
            blackEval += king + kingSquaresMiddleB[bKingIdx];
            whiteEval += king + kingSquaresMiddleW[wKingIdx];
        }
        else
        {
            blackEval += king + kingSquaresEnd[bKingIdx];
            whiteEval += king + kingSquaresEnd[wKingIdx];
            int dist = Math.Abs((wFile - bFile)) + (Math.Abs((wRank - bRank)));
            dist *= 12;
            if (color == 'b') //white just moved so advantage to white i think
            {
                //whiteEval += cornerKingSquares[wKingIdx];
                return whiteEval - (blackEval + dist);
            }

            return (whiteEval + dist) - blackEval;
        }

        return whiteEval - blackEval;
    }

    static void orderMoves(ref List<Move> moves, ulong enemyPawnAttacks, ref char[] board)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            int moveScore = 0;
<<<<<<< Updated upstream
            int pieceVal = getPieceValue(board[63-moves[i].source]);
            int captureVal = getPieceValue(board[63 - moves[i].dest]);
            if(captureVal > 0) //if capturing piece
            {
                moveScore = 10*captureVal - pieceVal; 
            }
            else
            {
                moveScore = pieceTables[board[63 - moves[i].source]][63 - moves[i].dest] - (pieceTables[board[63 - moves[i].source]][63 - moves[i].source]+100);
            }
            if (moves[i].promotion != ' ')
            {
                moveScore += getPieceValue(moves[i].promotion)*10;
            }
            if ((((ulong)1 << moves[i].dest) & enemyPawnAttacks) > 1) //moving piece to enemy pawn attack range, generally not a good move
=======
            int pieceVal = getPieceValue(board[63 - moves[i].source]);
            int captureVal = pieceValueOrder(board[63 - moves[i].dest]);
            if (captureVal > 0) //if capturing piece
            {
                moveScore = captureVal - pieceVal; 
            }
            if (moves[i].promotion != ' ')
            {
                moveScore += pieceValueOrder(moves[i].promotion);
            }

            if ((((ulong)1 << moves[i].dest) & enemyPawnAttacks) > 0) //moving piece to enemy pawn attack range, generally not a good move
>>>>>>> Stashed changes
            {
                moveScore -= pieceVal;
            }
            //make sure to account for king squares
<<<<<<< Updated upstream

=======
            moveScore += pieceTables[board[63 - moves[i].source]][63 - moves[i].dest] - (pieceTables[board[63 - moves[i].source]][63 - moves[i].source] + 100);
>>>>>>> Stashed changes
            moves[i].moveVal = moveScore;
        }
        //order after
        moves.Sort((x, y) => y.moveVal.CompareTo(x.moveVal));
    }

    static int getPieceValue(char piece)
    {
        /*
         int pawn = 100; //P/p
        int knight = 320; //N/n
        int bishop = 330; //B/b
        int rook = 500; //R/r
        int queen = 900; //Q/q
        int king = 2000; //K/k
         */
        switch (piece)
        {
            case 'P': case 'p':
                return 100;
            case 'N': case 'n':
                return 320;
            case 'B': case 'b':
                return 330;
            case 'R': case 'r':
                return 500;
            case 'Q': case 'q':
                return 900;
            default:
                return 0;
        }
        /*
        if (piece == 'P' || piece == 'p')
        {
            return 100;
        }
        else if (piece == 'N' || piece == 'n')
        {
            return 320;
        }
        else if (piece == 'B' || piece == 'b')
        {
            return 330;
        }
        else if (piece == 'R' || piece == 'r')
        {
            return 500;
        }
        else if (piece == 'Q' || piece == 'q')
        {
            return 900;
        }
        return 0;*/

    }

    static int pieceValueOrder(char piece)
    {
        switch (piece)
        {
            case 'P':
            case 'p':
                return 500;
            case 'N':
            case 'n':
                return 1600;
            case 'B':
            case 'b':
                return 1650;
            case 'R':
            case 'r':
                return 2500;
            case 'Q':
            case 'q':
                return 4500;
            default:
                return 0;
        }
        /*
        if (piece == 'P' || piece == 'p')
        {
            return 500;
        }
        else if (piece == 'N' || piece == 'n')
        {
            return 1600;
        }
        else if (piece == 'B' || piece == 'b')
        {
            return 1650;
        }
        else if (piece == 'R' || piece == 'r')
        {
            return 2500;
        }
        else if (piece == 'Q' || piece == 'q')
        {
            return 4500;
        }
        return 0;*/
    }
}