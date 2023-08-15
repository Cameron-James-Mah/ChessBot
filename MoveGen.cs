using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static ChessBot.Program;
using static Globals;

class MoveGen
{
    
    public static void getPawnMoves(char color, ulong bPawns, ulong wPawns, ulong empty, ref List<Move> moves, ulong whitePieces, ulong blackPieces)
    {
        ulong pawnsDest = 0;
        if (color == 'b')
        {
            //single pushes
            pawnsDest = bPawns >> 8;
            pawnsDest &= empty;
            ulong pawnsDest2 = pawnsDest;
            while (pawnsDest != 0)
            {
                Move newMove = new Move();
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                newMove.source = lsb + 8;
                newMove.dest = lsb;
                moves.Add(newMove);
            }
            //double pushes
            pawnsDest2 = pawnsDest2 >> 8;
            pawnsDest2 &= row4;
            pawnsDest2 &= empty;
            while (pawnsDest2 != 0)
            {
                Move newMove = new Move();
                int lsb = BitOperations.TrailingZeroCount(pawnsDest2);
                pawnsDest2 = ((ulong)1 << lsb) ^ pawnsDest2;
                newMove.source = lsb + 16;
                newMove.dest = lsb;
                moves.Add(newMove);
            }
            //pawn captures
            //east captures
            pawnsDest = bPawns >> 9;
            pawnsDest &= whitePieces;
            pawnsDest &= notAFile;
            while (pawnsDest != 0)
            {
                Move newMove = new Move();
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                newMove.source = lsb + 9;
                newMove.dest = lsb;
                moves.Add(newMove);
            }
            //west captures
            pawnsDest = bPawns >> 7;
            pawnsDest &= whitePieces;
            pawnsDest &= notHFile;
            while (pawnsDest != 0)
            {
                Move newMove = new Move();
                int lsb = BitOperations.TrailingZeroCount(pawnsDest);
                pawnsDest = ((ulong)1 << lsb) ^ pawnsDest;
                newMove.source = lsb + 7;
                newMove.dest = lsb;
                moves.Add(newMove);
            }
        }
        
    }
    public static void getKnightMoves(ref List<Move> moves, char color, ulong whitePieces, ulong blackPieces, ulong blackKnights, ulong whiteKnights, ulong empty)
    {
        if(color == 'b')
        {
            while (blackKnights != 0)
            {
                int lsb = BitOperations.TrailingZeroCount(blackKnights);
                blackKnights = ((ulong)1 << lsb) ^ blackKnights;
                //printBitBoard(blackKnights);
                ulong knightMoves = knightAttacks[lsb]&(empty | whitePieces);
                
                //printBitBoard(blackKnights);
                
                while (knightMoves != 0)
                {
                    Move newMove = new Move();
                    int kb = BitOperations.TrailingZeroCount(knightMoves);
                    knightMoves = ((ulong)1 << kb) ^ knightMoves;
                    newMove.source = lsb;
                    newMove.dest = kb;
                    moves.Add(newMove);
                }
            }
        }
        
    }
}
