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
    



    //white wants max eval, black wants min eval
    public static int quiescence(int alpha, int beta, char[] board, char color,
        ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
        ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
        ulong allPieces, ulong empty, ulong whitePieces, ulong blackPieces, ulong enPassant
        )
    {
        if (stopSearch)
        {
            return 0;
        }
        currNodes++;
        int stand_pat = eval(board, color, wPawn, bPawn);
        List<Move> moves = new List<Move>();
        char[] tempBoard = new char[64];
        int delta = 1400;
        if (color == 'b')
        {
            
            if (beta <= stand_pat)
            {
                return beta;
            }
            //delta pruning
            if (stand_pat - delta > beta)
            {
                return alpha;
            }
            beta = Math.Min(beta, stand_pat);
            MoveGen.getCaptures(ref moves, whitePieces, bPawn, bKnight, bBishop, bRook, bQueen, bKing, 0, color, allPieces);
        }
        else
        {
            
            if (stand_pat <= alpha)
            {
                return alpha;
            }
            //delta pruning
            if (stand_pat + delta < alpha)
            {
                return beta;
            }
            alpha = Math.Max(alpha, stand_pat);
            MoveGen.getCaptures(ref moves, blackPieces, wPawn, wKnight, wBishop, wRook, wQueen, wKing, 0, color, allPieces);
        }
        orderCaptures(ref moves, board);
        int score = 0;
        //int minEval = 60000;
        //int maxEval = -60000;
        bool moved = false;
        for (int i = 0; i < moves.Count; i++)
        {
            ulong bPawn2 = bPawn; ulong bRook2 = bRook; ulong bKnight2 = bKnight; ulong bBishop2 = bBishop; ulong bQueen2 = bQueen; ulong bKing2 = bKing;
            ulong wPawn2 = wPawn; ulong wRook2 = wRook; ulong wKnight2 = wKnight; ulong wBishop2 = wBishop; ulong wQueen2 = wQueen; ulong wKing2 = wKing;
            board.CopyTo(tempBoard, 0);
            tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source]; //add piece to dest square
            tempBoard[63 - moves[i].source] = ' '; //remove piece from source square
            Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                    ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].source); //remove piece from original square
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
                Board.removeBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                        ref wQueen2, ref wKing2, board[63 - moves[i].dest], moves[i].dest); //remove captured piece
                Board.addBitboardPiece(ref bPawn2, ref bRook2, ref bKnight2, ref bBishop2, ref bQueen2, ref bKing2, ref wPawn2, ref wRook2, ref wKnight2, ref wBishop2,
                ref wQueen2, ref wKing2, board[63 - moves[i].source], moves[i].dest); //add piece to dest square
            }
            
            allPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2 | wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
            if (color == 'b')
            {
                if (!isSquareAttacked(BitOperations.TrailingZeroCount(bKing2), wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b'))
                {
                    moved = true;
                    empty = ~allPieces;
                    whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                    blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;
                    //score = -quiescence(-beta, -alpha, tempBoard, 'w', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth);
                    score = quiescence(alpha, beta, tempBoard, 'w', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant);
                    /*
                    if(isSquareAttacked(BitOperations.TrailingZeroCount(wKing2), bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
                    {
                        score = -quiescence(-beta, -alpha, tempBoard, 'w', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth);
                    }
                    else
                    {
                        score = -quiescence(-beta, -alpha, tempBoard, 'w', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth - 1);
                    }
                    */
                    //minEval = Math.Min(score, minEval);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                    {
                        return beta;
                    }
                }   
            }
            else
            {
                if (!isSquareAttacked(BitOperations.TrailingZeroCount(wKing2), bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
                {
                    moved = true;
                    empty = ~allPieces;
                    whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                    blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;
                    //score = -quiescence(-beta, -alpha, tempBoard, 'b', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth);
                    score = quiescence(alpha, beta, tempBoard, 'b', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant);
                    /*
                    if (isSquareAttacked(BitOperations.TrailingZeroCount(bKing2), wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b'))
                    {
                        score = -quiescence(-beta, -alpha, tempBoard, 'b', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth);
                    }
                    else
                    {
                        score = -quiescence(-beta, -alpha, tempBoard, 'b', bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, whitePieces, blackPieces, enPassant, depth - 1);
                    }
                    */
                    //maxEval = Math.Max(score, maxEval);
                    alpha = Math.Max(score, alpha);
                    if (beta <= alpha)
                    {
                        return alpha;
                    }
                }
            }
            /*
            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
            }*/
        }
        if (color == 'b')
        {
            return beta;
        }
        return alpha;
    }

    public static int minimax(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
                                         ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
                                         ulong allPieces, ulong empty, char[] board, ulong whitePieces, ulong blackPieces,
                                         ulong castleRights, ulong enPassant, char color, int alpha, int beta, ulong currHash, int age, bool isNull)
    {
        if (stopSearch)
        {
            return 0;
        }
        currNodes++;
        if (depth == 0)
        {
            //return eval(board, color, wPawn, bPawn);
            return quiescence(alpha, beta, board, color, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, whitePieces, blackPieces, enPassant);
        }
        if (isRepetition(currHash)) //3 move repetition
        {
            return 0;
        }
        repetitionInc(currHash);
        char[] tempBoard = new char[64];
        List<Move> moves = new List<Move>();
        bool moved = false;
        int bestIdx = -1;
        Move pv = null;
        //Board.printBoard(board);
        //Console.WriteLine(bKing);
        if (color == 'b') //minimizing, black to move
        {
            int minEval = 60000; //arbitrary win number
            if (blackTable.ContainsKey(currHash))
            {
                
                if (age == blackTable[currHash].age && blackTable[currHash].depth >= depth)
                {
                    beta = Math.Min(beta, blackTable[currHash].value);
                    if (beta <= alpha)
                    {
                        //blackTable[currHash] = new Entry(beta, blackTable[currHash].depth, blackTable[currHash].mv, age);
                        repetitionDec(currHash);
                        return beta;
                    }
                }
                pv = blackTable[currHash].mv;
                //Board.printBoard(board);
                //Console.WriteLine(notation[pv.source] + notation[pv.dest] + " depth: " + blackTable[currHash].depth + " age: " + blackTable[currHash].age);
            }
            int kingSquare = BitOperations.TrailingZeroCount(bKing);
            //null move pruning
            
            if (!isNull && !isSquareAttacked(kingSquare, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b') && depth >= 4 && hasPieces(wKnight, wBishop, wRook, wQueen))
            {
                board.CopyTo(tempBoard, 0);
                int temp = minimax(depth - 4, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, castleRights, 0, 'w', alpha, beta, currHash, age, true);
                if(temp <= alpha)
                {
                    repetitionDec(currHash);
                    return alpha;
                }
            }

            
            MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
            MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
            MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
            MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);
            ulong enemyPawnAttacks = wPawn << 7 & notAFile;
            enemyPawnAttacks |= wPawn << 9 & notHFile;
            orderMoves(ref moves, enemyPawnAttacks, board, killers[depth], pv);
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
                int kingSource = BitOperations.TrailingZeroCount(bKing2);
                if (!isSquareAttacked(kingSource, wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b')) //FOR SOME REASON BLACK CASTLING MOVES DONT PASS THIS
                {
                    empty = ~allPieces;
                    whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                    blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;
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
                        newHash ^= newCastleRights;
                    }
                    int temp;
                    
                    if (isSquareAttacked(BitOperations.TrailingZeroCount(wKing2), bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w')) //check extension
                    {
                        temp = minimax(depth, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', alpha, beta, newHash, age, false);
                    }
                    else
                    {
                        temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', alpha, beta, newHash, age, false);
                    }
                    //temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', alpha, beta, newHash, age, false);
                    //minEval = Math.Min(temp, minEval);
                    if (temp <= minEval)
                    {
                        minEval = temp;
                        //bestMv = moves[i];
                        bestIdx = i;
                    }
                    beta = Math.Min(beta, minEval);

                    if (beta <= alpha)
                    {
                        //if black table has entry or entry depth is below current depth
                        if (!blackTable.ContainsKey(currHash) || blackTable[currHash].depth <= depth || blackTable[currHash].age < age)
                        {   
                            blackTable[currHash] = new Entry(minEval, depth, moves[i], age);
                        }
                        if (board[63 - moves[i].dest] == ' ')
                        {
                            killers[depth] = moves[i];
                        }
                        repetitionDec(currHash);
                        return minEval;
                    }
                }
            }

            if (!moved) //no valid moves 
            {
                //int kingSource = BitOperations.TrailingZeroCount(bKing);
                if (isSquareAttacked(kingSquare, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b')) //checkmate
                {
                    minEval = 50000 + depth; //50000 is arbitrary number to represent win, addition/subtraction for depth is to give inclination for faster wins
                }
                else //stalemate
                {
                    repetitionDec(currHash);
                    return 0;
                }
            }
            
            if (!blackTable.ContainsKey(currHash) || blackTable[currHash].age < age && bestIdx != -1 || blackTable[currHash].depth <= depth && bestIdx != -1)
            {
                blackTable[currHash] = new Entry(minEval, depth, null, age);
            }
            repetitionDec(currHash);
            //Board.printBoard(board);
            return minEval;
        }
        else //maximizing, white to move
        {
            int maxEval = -60000; //arbitrary win number
            if (whiteTable.ContainsKey(currHash))
            {
                if(age == whiteTable[currHash].age && whiteTable[currHash].depth >= depth)
                {
                    alpha = Math.Max(alpha, whiteTable[currHash].value);

                    if (beta <= alpha)
                    {
                        //whiteTable[currHash] = new Entry(alpha, whiteTable[currHash].depth, whiteTable[currHash].mv, age);
                        repetitionDec(currHash);
                        return alpha;
                    }
                }
                pv = whiteTable[currHash].mv;
                //Board.printBoard(board);
                //Console.WriteLine(notation[pv.source] + notation[pv.dest] + " depth: " + whiteTable[currHash].depth);
            }
            int kingSquare = BitOperations.TrailingZeroCount(wKing);
            //null move pruning
            
            if (!isNull && !isSquareAttacked(kingSquare, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w') && depth >= 4 && hasPieces(wKnight, wBishop, wRook, wQueen))
            {
                board.CopyTo(tempBoard, 0);
                int temp = minimax(depth - 4, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, castleRights, 0, 'b', alpha, beta, currHash, age, true);
                if(beta <= temp)
                {
                    repetitionDec(currHash);
                    return beta;
                }
            }
            currNodes++;
            MoveGen.getPawnMoves(wPawn, empty, ref moves, blackPieces, enPassant, color);
            MoveGen.getKnightMoves(ref moves, blackPieces, wKnight, empty);
            MoveGen.getBishopMoves(ref moves, blackPieces, wBishop, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wRook, allPieces);
            MoveGen.getBishopMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getRookMoves(ref moves, blackPieces, wQueen, allPieces);
            MoveGen.getKingMoves(ref moves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);
            
            ulong enemyPawnAttacks = bPawn >> 7 & notHFile;
            enemyPawnAttacks |= bPawn >> 9 & notAFile;
            orderMoves(ref moves, enemyPawnAttacks, board, killers[depth], pv);
            board.CopyTo(tempBoard, 0);
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
                
                int kingSource = BitOperations.TrailingZeroCount(wKing2);
                if (!isSquareAttacked(kingSource, bBishop2, bRook2, bKnight2, bQueen2, bPawn2, bKing2, allPieces, 'w'))
                {
                    empty = ~allPieces;
                    whitePieces = wPawn2 | wRook2 | wKnight2 | wBishop2 | wQueen2 | wKing2;
                    blackPieces = bPawn2 | bRook2 | bKnight2 | bBishop2 | bQueen2 | bKing2;
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
                        newHash ^= newCastleRights;
                    }
                    int temp;
                    
                    if (isSquareAttacked(BitOperations.TrailingZeroCount(bKing2), wBishop2, wRook2, wKnight2, wQueen2, wPawn2, wKing2, allPieces, 'b')) //check extension
                    {
                        temp = minimax(depth, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', alpha, beta, newHash, age, false);
                    }
                    else
                    {
                        temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', alpha, beta, newHash, age, false);
                    }
                    //temp = minimax(depth - 1, bPawn2, bRook2, bKnight2, bBishop2, bQueen2, bKing2, wPawn2, wRook2, wKnight2, wBishop2, wQueen2, wKing2, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', alpha, beta, newHash, age, false);
                    if (temp >= maxEval)
                    {
                        maxEval = temp;
                        //bestMv = moves[i];
                    }
                    //maxEval = Math.Max(maxEval, temp);
                    alpha = Math.Max(maxEval, alpha);
                    if (beta <= alpha)
                    {
                        //if black table has entry or entry depth is below current depth
                        if (!whiteTable.ContainsKey(currHash) || whiteTable[currHash].depth <= depth || age > whiteTable[currHash].age)
                        {
                            //Board.printBoard(board);
                            //Console.WriteLine("---------------------------------------------");
                            //whiteTable.Remove(currHash);
                            //whiteTable.Add(currHash, new Entry(maxEval, depth, moves[i], currHash, board));
                            whiteTable[currHash] = new Entry(maxEval, depth, moves[i], age);
                            /*
                            Board.printBoard(board);
                            Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest]);*/
                        }
                        if (board[63 - moves[i].dest] == ' ')
                        {
                            killers[depth] = moves[i];
                        }
                        repetitionDec(currHash);
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
                }
                else //stalemate
                {
                    //Board.printBoard(board);
                    repetitionDec(currHash);
                    return 0;
                }

            }
            
            if (!whiteTable.ContainsKey(currHash) ||  age > whiteTable[currHash].age && bestIdx != -1 || whiteTable[currHash].depth <= depth && bestIdx != -1)
            {
                whiteTable[currHash] = new Entry(maxEval, depth, null, age);
            }
            repetitionDec(currHash);
            return maxEval;
        }
    }


    public static int eval(char[] board, char color, ulong wPawn, ulong bPawn)
    {
        int pawn = 100; //P/p
        int knight = 420; //N/n
        int bishop = 470; //B/b
        int rook = 750; //R/r
        int queen = 1400; //Q/q
        int king = 0; //K/k
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
        int isoPenalty = 25;
        int doublePenalty = 30;
        int wPawnFiles = 0b_00000000;
        int bPawnFiles = 0b_00000000;
        for (int i = 0; i < board.Length; i++)
        {
            switch (board[i])
            {
                case 'P':
                    whiteEval += pawn + pawnSquaresW[i];
                    int currFileW = 1 << (8 - file);
                    
                    if ((currFileW & wPawnFiles) > 0) //checking for doubled pawns
                    {
                        whiteEval -= doublePenalty;
                    }
                    
                    wPawnFiles |= currFileW;
                    if ((isolated[file] & wPawn) == 0) //checking if isolated pawn
                    {
                        whiteEval -= isoPenalty;
                    }
                    break;
                case 'N':
                    whiteEval += knight + knightSquares[i];
                    whiteMaterial += knight;
                    break;
                case 'B':
                    whiteEval += bishop + bishopSquaresW[i];
                    whiteMaterial += bishop;
                    break;
                case 'R':
                    whiteEval += rook + rookSquaresW[i];
                    whiteMaterial += rook;
                    break;
                case 'Q':
                    whiteEval += queen + queenSquares[i];
                    whiteMaterial += queen;
                    break;
                case 'K':
                    wKingIdx = i;
                    wFile = file;
                    wRank = rank;
                    break;
                case 'p':
                    blackEval += pawn + pawnSquaresB[i];
                    int currFileB = 1 << (8 - file);
                    
                    if ((currFileB & bPawnFiles) > 0) //checking for doubled pawns
                    {
                        blackEval -= doublePenalty;
                    }
                    bPawnFiles |= currFileB;
                    if ((isolated[file] & bPawn) == 0) //checking if isolated pawn
                    {
                        blackEval -= isoPenalty;
                    }
                    break;
                case 'n':
                    blackEval += knight + knightSquares[i];
                    blackMaterial += knight;
                    break;
                case 'b':
                    blackEval += bishop + bishopSquaresB[i];
                    blackMaterial += bishop;
                    break;
                case 'r':
                    blackEval += rook + rookSquaresB[i];
                    blackMaterial += rook;
                    break;
                case 'q':
                    blackEval += queen + queenSquares[i];
                    blackMaterial += queen;
                    break;
                case 'k':
                    bKingIdx = i;
                    bFile = file;
                    bRank = rank;
                    break;
            }
            file++;
            if (file == 8)
            {
                rank++;
                file = 0;
            }
        }
        
        if (blackMaterial > 1300 || whiteMaterial > 1300)
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

    static void orderMoves(ref List<Move> moves, ulong enemyPawnAttacks, char[] board, Move killer, Move hashMove)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            char src = board[63 - moves[i].source];
            int dest = moves[i].dest;
            int pieceVal = getPieceValue(src);
            int captureVal = getPieceValue(board[63 - dest]);
            if (hashMove != null && hashMove.source == moves[i].source && hashMove.dest == moves[i].dest)
            {
                //Board.printBoard(board);
                //Console.WriteLine(notation[hashMove.source] + notation[hashMove.dest]);
                hashMove = null;
                moves[i].moveVal = 10000;
                //Console.WriteLine(123);
                continue;
            }
            if (killer != null && killer.source == moves[i].source && killer.dest == moves[i].dest)
            {
                killer = null;
                moves[i].moveVal = 4000;
                continue;
            }
            if (captureVal > 0) //if capturing piece
            {
                moves[i].moveVal += captureVal - pieceVal + 1000; 
                if(captureVal >= pieceVal)
                {
                    moves[i].moveVal += 4500;
                }
            }
            else
            {
                moves[i].moveVal += (pieceTables[src][63 - dest]) - (pieceTables[src][63 - moves[i].source]);
            }
            if (moves[i].promotion != ' ')
            {
                moves[i].moveVal += pieceValueOrder(moves[i].promotion);
            }

            if ((((ulong)1 << dest) & enemyPawnAttacks) > 0) //moving piece to enemy pawn attack range, generally not a good move
            {
                moves[i].moveVal -= pieceVal;
            }
            //make sure to account for king squares
        }
        //order after 11 26 97
        //Console.WriteLine();
        moves.Sort((x, y) => y.moveVal.CompareTo(x.moveVal));
    }

    static void orderCaptures(ref List<Move> moves, char[] board)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            char src = board[63 - moves[i].source];
            int dest = moves[i].dest;
            int pieceVal = getPieceValue(src);
            int captureVal = pieceValueOrder(board[63 - dest]);
            moves[i].moveVal += captureVal - pieceVal;
        }
        moves.Sort((x, y) => y.moveVal.CompareTo(x.moveVal));
    }

    public static int getPieceValue(char piece)
    {
        switch (piece)
        {
            case 'P': case 'p':
                return 100;
            case 'N': case 'n':
                return 420;
            case 'B': case 'b':
                return 480;
            case 'R': case 'r':
                return 750;
            case 'Q': case 'q':
                return 1400;
            default:
                return 0;
        }
    }

    public static int pieceValueOrder(char piece)
    {
        switch (piece)
        {
            case 'P':
            case 'p':
                return 500;
            case 'N':
            case 'n':
                return 2100;
            case 'B':
            case 'b':
                return 2400;
            case 'R':
            case 'r':
                return 3750;
            case 'Q':
            case 'q':
                return 7000;
            default:
                return 0;
        }
    }

    public static bool hasPieces(ulong knight, ulong bishop, ulong rook, ulong queen)
    {
        if((knight |= bishop |= rook |= queen) > 0)
        {
            return true;
        }
        return false;
    }

    public static void repetitionInc(ulong hash)
    {
        if (repetitionSearch.ContainsKey(hash))
        {
            repetitionSearch[hash]++;
        }
        else
        {
            repetitionSearch.Add(hash, 1);
        }
    }

    public static void repetitionDec(ulong hash)
    {
        if (repetitionSearch.ContainsKey(hash))
        {
            repetitionSearch[hash]--;
        }
    }

    public static bool isRepetition(ulong hash)
    {
        if (repetitionSearch.ContainsKey(hash) && repetitionSearch[hash] == 2)
        {
            return true;
        }
        return false;
    }
}