using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static ChessBot.Program;
using static Globals;

class MoveGen
{
    //Move genration functions
    public static void getPawnMoves(ulong myPawns, ulong empty, ref List<Move> moves, ulong enemyPieces, ulong enPassant, char color)
    {
        //i will still have to distinguish between colors since it will change direction i shift
        //REMEMBER I NEED TO DISTINGUISH COLORS BECAUSE IT WILL CHANGE SHIFT DIRECTIONS

        enemyPieces |= enPassant; //add en passant moves to enemy pieces list since can be treated the same
        ulong pawnsDest = 0;
        if(color == 'b')
        {
            //single pushes
            pawnsDest = myPawns >> 8;
            pawnsDest &= empty;
            ulong pawnsDest2 = pawnsDest;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                //if promoting move
                if ((pawnsDest & row0) > 0 || (pawnsDest & row7) > 0) //made it to either end
                {
                    Console.WriteLine("here");
                    moves.Add(new Move(lsb + 8, lsb, 'r'));
                    moves.Add(new Move(lsb + 8, lsb, 'n'));
                    moves.Add(new Move(lsb + 8, lsb, 'b'));
                    moves.Add(new Move(lsb + 8, lsb, 'q'));
                }
                else
                {
                    Move newMove = new Move(lsb + 8, lsb, ' ');
                    //newMove.source = lsb + 8;
                    //newMove.dest = lsb;
                    moves.Add(newMove);
                }
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;


            }
            //double pushes
            pawnsDest2 = pawnsDest2 >> 8;
            pawnsDest2 &= row4;
            pawnsDest2 &= empty;
            while (pawnsDest2 != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest2);
                pawnsDest2 = ((ulong)1 << lsb) ^ pawnsDest2;
                Move newMove = new Move(lsb + 16, lsb, ' ');
                //newMove.source = lsb + 16;
                //newMove.dest = lsb;
                moves.Add(newMove);
            }
            //pawn captures, en passant bits added to enemyPieces at beginning of function
            //east captures
            pawnsDest = myPawns >> 9;
            pawnsDest &= enemyPieces;
            pawnsDest &= enPassant;
            pawnsDest &= notAFile;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                Move newMove = new Move(lsb + 9, lsb, ' ');
                //newMove.source = lsb + 9;
                //newMove.dest = lsb;
                moves.Add(newMove);
            }
            //west captures
            pawnsDest = myPawns >> 7;
            pawnsDest &= enemyPieces;
            pawnsDest &= notHFile;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                Move newMove = new Move(lsb + 7, lsb, ' ');
                //newMove.source = lsb + 7;
                //newMove.dest = lsb;
                moves.Add(newMove);
            }
        }
        
        

        
        
    }
    //(ref List<Move> moves, ulong enemyPieces, ulong myRooks, ulong empty, ulong allPieces)
    public static void getKnightMoves(ref List<Move> moves, ulong enemyPieces, ulong myKnights, ulong empty)
    {
        while (myKnights != 0)
        {
            int lsb = BitOperations.TrailingZeroCount(myKnights);
            
            //printBitBoard(blackKnights);
            ulong knightMoves = knightAttacks[lsb]&(empty | enemyPieces);
                
            //printBitBoard(blackKnights);
                
            while (knightMoves != 0)
            {
                int kb = BitOperations.TrailingZeroCount(knightMoves);
                knightMoves = ((ulong)1 << kb) ^ knightMoves;
                Move newMove = new Move(lsb, kb, ' ');
                //newMove.source = lsb;
                //newMove.dest = kb;
                moves.Add(newMove);
            }
            myKnights = ((ulong)1 << lsb) ^ myKnights;
        }
        
    }
    //(ref List<Move> moves, ulong enemyPieces, ulong myRooks, ulong empty, ulong allPieces)
    public static void getBishopMoves(ref List<Move> moves, ulong enemyPieces, ulong myBishops, ulong allPieces)
    {
        while (myBishops != 0)
        {
            int lsb = BitOperations.TrailingZeroCount(myBishops);
            myBishops = ((ulong)1 << lsb) ^ myBishops;
            //printBitBoard(blackKnights);
            //0 = SW, 1 = SE, 2 = NW, 3 = NE
            ulong SWmoves = bishopAttacks[lsb, 0];
            ulong SEmoves = bishopAttacks[lsb, 1];
            ulong NWmoves = bishopAttacks[lsb, 2];
            ulong NEmoves = bishopAttacks[lsb, 3];
            //bitwise & with allPieces, bitscan for lsb or msb depending on dir to get first blocker, everything between is a valid square, then check if blocker is enemy for additional square
            //SW
            ulong blockers = SWmoves & allPieces;
            int msb = 63 - BitOperations.LeadingZeroCount(blockers); //find first blocker
                
            ulong validSquares = 0;
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = bishopAttacks[lsb, 0] & (~bishopAttacks[msb, 0]);
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
                validSquares = ((ulong)1 << msb) ^ validSquares;
            }
            else
            {
                validSquares = bishopAttacks[lsb, 0];
            }
            while(validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }
                

            //SE
            blockers = SEmoves & allPieces;
            msb = 63 - BitOperations.LeadingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = bishopAttacks[lsb, 1] & (~bishopAttacks[msb, 1]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = bishopAttacks[lsb, 1];
            }
                
            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }
                

            //NW
            blockers = NWmoves & allPieces;
            msb = BitOperations.TrailingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = bishopAttacks[lsb, 2] & (~bishopAttacks[msb, 2]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = bishopAttacks[lsb, 2];
            }
                
            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }
                

            //NE
            blockers = NEmoves & allPieces;
            msb = BitOperations.TrailingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = bishopAttacks[lsb, 3] & (~bishopAttacks[msb, 3]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = bishopAttacks[lsb, 3];
            }
            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }
                
        }
        

    }

    public static void getRookMoves(ref List<Move> moves, ulong enemyPieces, ulong myRooks, ulong allPieces)
    {
        while (myRooks != 0)
        {
            int lsb = BitOperations.TrailingZeroCount(myRooks);
            myRooks = ((ulong)1 << lsb) ^ myRooks;
            //printBitBoard(blackKnights);
            //0 = S, 1 = E, 2 = W, 3 = N
            ulong Smoves = rookAttacks[lsb, 0];
            ulong Emoves = rookAttacks[lsb, 1];
            ulong Wmoves = rookAttacks[lsb, 2];
            ulong Nmoves = rookAttacks[lsb, 3];
            //bitwise & with allPieces, bitscan for lsb or msb depending on dir to get first blocker, everything between is a valid square, then check if blocker is enemy for additional square
            //S
            ulong blockers = Smoves & allPieces;
            int msb = 63 - BitOperations.LeadingZeroCount(blockers); //find first blocker

            ulong validSquares = 0;
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = rookAttacks[lsb, 0] & (~rookAttacks[msb, 0]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = rookAttacks[lsb, 0];
            }
            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }


            //E
            blockers = Emoves & allPieces;
            msb = 63 - BitOperations.LeadingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = rookAttacks[lsb, 1] & (~rookAttacks[msb, 1]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = rookAttacks[lsb, 1];
            }

            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }


            //W
            blockers = Wmoves & allPieces;
            msb = BitOperations.TrailingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = rookAttacks[lsb, 2] & (~rookAttacks[msb, 2]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = rookAttacks[lsb, 2];
            }

            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }


            //N
            blockers = Nmoves & allPieces;
            msb = BitOperations.TrailingZeroCount(blockers); //find first blocker
            if (msb >= 0 && msb < 64) //found blocker 
            {
                validSquares = rookAttacks[lsb, 3] & (~rookAttacks[msb, 3]);
                validSquares = ((ulong)1 << msb) ^ validSquares;
                if ((((ulong)1 << msb) & enemyPieces) > 0) //check if blocker is enemy piece(capturable)
                {
                    Move newMove = new Move(lsb, msb, ' ');
                    //newMove.source = lsb;
                    //newMove.dest = msb;
                    moves.Add(newMove);
                }
            }
            else
            {
                validSquares = rookAttacks[lsb, 3];
            }
            while (validSquares != 0)
            {
                int bb = BitOperations.TrailingZeroCount(validSquares);
                validSquares = ((ulong)1 << bb) ^ validSquares;
                Move newMove = new Move(lsb, bb, ' ');
                //newMove.source = lsb;
                //newMove.dest = bb;
                moves.Add(newMove);
            }

        }
    }

    public static void getKingMoves(ref List<Move> moves, ulong enemyPieces, ulong myKing, ulong empty, ulong castleRights, ulong allPieces)
    {
        int lsb = BitOperations.TrailingZeroCount(myKing);
        //printBitBoard(blackKnights);
        ulong kingMoves = kingAttacks[lsb] & (empty | enemyPieces);

        //printBitBoard(blackKnights);

        while (kingMoves != 0)
        {
            int kb = BitOperations.TrailingZeroCount(kingMoves);
            kingMoves = ((ulong)1 << kb) ^ kingMoves;
            Move newMove = new Move(lsb, kb, ' ');
            //newMove.source = lsb;
            //newMove.dest = kb;
            moves.Add(newMove);
        }
        
        //castling moves
        if((myKing & castleRights) > 0)
        {
            ulong longSquares = myKing << 1 | myKing << 2 | myKing << 3;
            ulong shortSquares = myKing >> 1 | myKing >> 2;
            if (((myKing << 4) & castleRights) > 0 && (longSquares & allPieces) == 0) //long castle
            {
                Move newMove = new Move(lsb, lsb+2, ' ');
                //newMove.source = lsb;
                //newMove.dest = lsb + 2;
                moves.Add(newMove);
            }
            if (((myKing >> 3) & castleRights) > 0 && (shortSquares & allPieces) == 0) //short castle
            {
                Move newMove = new Move(lsb, lsb-2, ' ');
                //newMove.source = lsb;
                //newMove.dest = lsb - 2;
                moves.Add(newMove);
            }
        }

    }

}


