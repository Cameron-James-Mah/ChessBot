using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static ChessBot.Program;
using static Globals;
using static Position;

//MOVE GENERATION FUNCTIONS
class MoveGen
{
    //Move genration functions
    public static void getPawnMoves(ulong myPawns, ulong empty, ref List<Move> moves, ulong enemyPieces, ulong enPassant, char color)
    {
        //i will still have to distinguish between colors since it will change direction i shift
        //REMEMBER I NEED TO DISTINGUISH COLORS BECAUSE IT WILL CHANGE SHIFT DIRECTIONS

        enemyPieces |= enPassant; //add en passant moves to enemy pieces list since can be treated the same
        ulong pawnsDest = 0;
        if (color == 'b')
        {
            //single pushes
            pawnsDest = myPawns >> 8;
            pawnsDest &= empty;
            ulong pawnsDest2 = pawnsDest;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                //if promoting move
                if (((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0) //made it to either end
                {
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
                newMove.enPassant = true;
                //newMove.source = lsb + 16;
                //newMove.dest = lsb;
                moves.Add(newMove);
            }
            //pawn captures, en passant bits added to enemyPieces at beginning of function
            //east captures
            
            pawnsDest = myPawns >> 9;
            pawnsDest &= enemyPieces;
            pawnsDest &= notAFile;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                if(((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0){
                    moves.Add(new Move(lsb + 9, lsb, 'r'));
                    moves.Add(new Move(lsb + 9, lsb, 'n'));
                    moves.Add(new Move(lsb + 9, lsb, 'b'));
                    moves.Add(new Move(lsb + 9, lsb, 'q'));
                }
                else
                {
                    if ((((ulong)1 << lsb) & enPassant) > 0)
                    {
                        Move newMove = new Move(lsb + 9, lsb, ' ');
                        newMove.capPassant = lsb + 8;
                        moves.Add(newMove);
                    }
                    else
                    {
                        Move newMove = new Move(lsb + 9, lsb, ' ');
                        moves.Add(newMove);
                    }
                    
                }
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                
                //newMove.source = lsb + 9;
                //newMove.dest = lsb;
                
            }
            //west captures
            pawnsDest = myPawns >> 7;
            pawnsDest &= enemyPieces;
            pawnsDest &= notHFile;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                if (((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0)
                {
                    moves.Add(new Move(lsb + 7, lsb, 'r'));
                    moves.Add(new Move(lsb + 7, lsb, 'n'));
                    moves.Add(new Move(lsb + 7, lsb, 'b'));
                    moves.Add(new Move(lsb + 7, lsb, 'q'));
                }
                else
                {
                    if((((ulong)1 << lsb) & enPassant) > 0){
                        Move newMove = new Move(lsb + 7, lsb, ' ');
                        newMove.capPassant = lsb + 8;
                        moves.Add(newMove);
                    }
                    else
                    {
                        Move newMove = new Move(lsb + 7, lsb, ' ');
                        moves.Add(newMove);
                    }
                    
                }
            }
        }
        else if (color == 'w')
        {
            //single pushes
            pawnsDest = myPawns << 8;
            pawnsDest &= empty;
            ulong pawnsDest2 = pawnsDest;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                //if promoting move
                if (((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0) //made it to either end
                {
                    moves.Add(new Move(lsb - 8, lsb, 'R'));
                    moves.Add(new Move(lsb - 8, lsb, 'N'));
                    moves.Add(new Move(lsb - 8, lsb, 'B'));
                    moves.Add(new Move(lsb - 8, lsb, 'Q'));
                }
                else
                {
                    Move newMove = new Move(lsb - 8, lsb, ' ');
                    //newMove.source = lsb + 8;
                    //newMove.dest = lsb;
                    moves.Add(newMove);
                }
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;


            }
            //double pushes
            pawnsDest2 = pawnsDest2 << 8;
            pawnsDest2 &= row3;
            pawnsDest2 &= empty;
            while (pawnsDest2 != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest2);
                pawnsDest2 = ((ulong)1 << lsb) ^ pawnsDest2;
                Move newMove = new Move(lsb - 16, lsb, ' ');
                newMove.enPassant = true;
                //newMove.source = lsb + 16;
                //newMove.dest = lsb;
                moves.Add(newMove);
            }
        
            //pawn captures, en passant bits added to enemyPieces at beginning of function
            //west captures
            pawnsDest = myPawns << 9;
            pawnsDest &= enemyPieces;
            pawnsDest &= notHFile;
            
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                if (((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0)
                {
                    moves.Add(new Move(lsb - 9, lsb, 'R'));
                    moves.Add(new Move(lsb - 9, lsb, 'N'));
                    moves.Add(new Move(lsb - 9, lsb, 'B'));
                    moves.Add(new Move(lsb - 9, lsb, 'Q'));
                }
                else
                {
                    if ((((ulong)1 << lsb) & enPassant) > 0)
                    {
                        Move newMove = new Move(lsb - 9, lsb, ' ');
                        newMove.capPassant = lsb - 8;
                        moves.Add(newMove);
                    }
                    else
                    {
                        Move newMove = new Move(lsb - 9, lsb, ' ');
                        moves.Add(newMove);
                    }
                    
                }
            }
            //east captures
            pawnsDest = myPawns << 7;
            pawnsDest &= enemyPieces;
            pawnsDest &= notAFile;
            while (pawnsDest != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                if (((ulong)1 << lsb & row0) > 0 || ((ulong)1 << lsb & row7) > 0)
                {
                    moves.Add(new Move(lsb - 7, lsb, 'R'));
                    moves.Add(new Move(lsb - 7, lsb, 'N'));
                    moves.Add(new Move(lsb - 7, lsb, 'B'));
                    moves.Add(new Move(lsb - 7, lsb, 'Q'));
                }
                else
                {
                    if ((((ulong)1 << lsb) & enPassant) > 0)
                    {
                        Move newMove = new Move(lsb - 7, lsb, ' ');
                        newMove.capPassant = lsb - 8;
                        moves.Add(newMove);
                    }
                    else
                    {
                        Move newMove = new Move(lsb - 7, lsb, ' ');
                        moves.Add(newMove);
                    }
                }
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
            ulong knightMoves = knightAttacks[lsb] & (empty | enemyPieces);

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
            while (validSquares != 0)
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

    public static void getKingMoves(ref List<Move> moves, ulong enemyPieces, ulong myKing, ulong empty, ulong castleRights, ulong allPieces,
        ulong myRook, ulong ePawn, ulong eRook, ulong eKnight, ulong eBishop, ulong eQueen, ulong eKing, char color)
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
        if ((myKing & castleRights) > 0)
        {

            ulong longSquares = myKing << 1 | myKing << 2 | myKing << 3;
            ulong shortSquares = myKing >> 1 | myKing >> 2;
            castleRights &= myRook;
            //printBitBoard(castleRights);
            if (((myKing << 4) & castleRights) > 0 && (longSquares & allPieces) == 0 && !isSquareAttacked(lsb, eBishop, eRook, eKnight, eQueen, ePawn, eKing, allPieces, color) && !isSquareAttacked(lsb + 1, eBishop, eRook, eKnight, eQueen, ePawn, eKing, allPieces, color) && !isSquareAttacked(lsb + 2, eBishop, eRook, eKnight, eQueen, ePawn, eKing, allPieces, color)) //long castle && not in check or castling through check
            {
                Move newMove = new Move(lsb, lsb + 2, ' ');
                newMove.castleFrom = lsb + 4;
                newMove.castleTo = lsb + 1;
                //newMove.source = lsb;
                //newMove.dest = lsb + 2;
                moves.Add(newMove);
            }

            if (((myKing >> 3) & castleRights) > 0 && (shortSquares & allPieces) == 0 && !isSquareAttacked(lsb, eBishop, eRook, eKnight, eQueen, ePawn, eKing, allPieces, color) && !isSquareAttacked(lsb - 1, eBishop, eRook, eKnight, eQueen, ePawn, eKing, allPieces, color)) //short castle && not in check or castling through check
            { //dont think i need to check the desination square of king for check since i am checking that later on
                Move newMove = new Move(lsb, lsb - 2, ' ');
                newMove.castleFrom = lsb - 3;
                newMove.castleTo = lsb - 1;
                //newMove.source = lsb;
                //newMove.dest = lsb - 2;
                moves.Add(newMove);
            }
        }

    }


    /*
     * CAPTURE MOVE GEN FOR QUIESCENCE
     */

    public static void getPawnCaptures(ref List<Move> moves, ulong enemyPieces, ulong myPawn)
    {

    }
}

