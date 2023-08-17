using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Globals;

//0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;


//maybe bitmask for en passent


//MAYBE TRY GEN PAWN MOVES FIRST

namespace ChessBot
{
    class Program
    {
        public struct Move
        {
            public int source;
            public int dest;
        }
        
        public static void Main()
        {
            //piece bitboards
            ulong bPawn = 0b_00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong bRook = 0b_10000001_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong bKnight = 0b_01000010_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong bBishop = 0b_00100100_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong bQueen = 0b_00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong bKing = 0b_00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

            ulong wPawn = 0b_00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000;
            ulong wRook = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_10000001;
            ulong wKnight = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000010;
            ulong wBishop = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00100100;
            ulong wQueen = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000;
            ulong wKing = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000;

            ulong allPieces = 0b_11111111_11111111_00000000_00000000_00000000_00000000_11111111_11111111; //update when needed, during move gen etc
            ulong empty = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000; //just update this to the NOT of all pieces when needed

            

            ulong whitePieces = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            ulong blackPieces = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

            //attack tables
            

            ulong enPassant = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000; //pawns that can be captured via en passant
            char color = 'b';
            List<Move> moves = new List<Move>();
            //pawn = p, knight = n, bishop = b, rook = r, queen = q, king = k
            //I think board should be inverted?
            //a8 is msb and h1 is lsb
            char[] board = new char[64] {
                'r','n','b','q','k','b','n','r',
                'p','p','p','p','p','p','p','p',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                'P','P','P','P','P','P','P','P',
                'R','N','B','Q','K','B','N','R'
            };
            //for converting square number(bit index from left of bitboard) to notation
            string[] notation = new string[64] {
                "h1", "g1", "f1", "e1", "d1", "c1", "b1", "a1",
                "h2", "g2", "f2", "e2", "d2", "c2", "b2", "a2",
                "h3", "g3", "f3", "e3", "d3", "c3", "b3", "a3",
                "h4", "g4", "f4", "e4", "d4", "c4", "b4", "a4",
                "h5", "g5", "f5", "e5", "d5", "c5", "b5", "a5",
                "h6", "g6", "f6", "e6", "d6", "c6", "b6", "a6",
                "h7", "g7", "f7", "e7", "d7", "c7", "b7", "a7",
                "h8", "g8", "f8", "e8", "d8", "c8", "b8", "a8",
            };
            AttackTables.generateTables();
            
            while (true)
            {
                string command = Console.ReadLine();
                string[] tokens = command.Trim().Split();
                switch (tokens[0])
                {
                    case "uci":
                        Console.WriteLine("uciok");
                        break;
                    case "isready":
                        Console.WriteLine("readyok");
                        break;
                    case "go":
                        //Console.WriteLine("bestmove e7e5");
                        MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces);
                        MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
                        MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
                        MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
                        MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
                        MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
                        MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty);
                        //for now just iterate over all of these moves and validate them for testing purposes, only iterate through once later on
                        char[] tempBoard = new char[64];
                        board.CopyTo(tempBoard, 0);
                        List<Move> validMoves = new List<Move>();
                        for (int i = 0; i < moves.Count; i++)
                        {
                            tempBoard[63 - moves[i].dest] = tempBoard[63 - moves[i].source];
                            tempBoard[63 - moves[i].source] = ' ';
                            Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                                ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard, ref whitePieces, ref blackPieces);
                            int kingSource = BitOperations.TrailingZeroCount(bKing);
                            if (!isSquareAttacked(kingSource, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, 'b'))
                            {
                                validMoves.Add(moves[i]);
                            }
                            board.CopyTo(tempBoard, 0);
                        }
                        
                        for (int i = 0; i < moves.Count; i++)
                        {
                            Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest);
                        }
                        //
                        if (validMoves.Count == 0)
                        {
                            Console.WriteLine("No moves");
                        }
                        int rnd = new Random().Next(0, validMoves.Count-1);
                        Move bestMove = new Move();
                        bestMove = validMoves[rnd];
                        
                        Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest]);
                        moves.Clear();
                        validMoves.Clear();
                        break;
                    case "stop":
                        System.Environment.Exit(0);
                        break;
                    case "p":
                        Board.printBoard(board);
                        break;
                    case "t":
                        //printBitBoard(generateKnightAttack(0));
                        //generateTables();
                        /*
                        for (int i = 0; i < 64; i++)
                        {
                            printBitBoard(kingAttacks[i]);
                            Console.Write("Bitboard Source: " + i);
                            Console.WriteLine("\n");
                        }*/
                        for (int i = 0; i < 64; i++)
                        {
                            for(int j = 0; j < 4; j++)
                            {
                                printBitBoard(rookAttacks[i, j]);
                                Console.Write("Bitboard Source: " + i + "\nBitboard dir: " + j);
                                Console.WriteLine("\n");
                            }
                            
                        }
                        break;
                    case "position":
                        board = new char[64] {
                            'r','n','b','q','k','b','n','r',
                            'p','p','p','p','p','p','p','p',
                            ' ',' ',' ',' ',' ',' ',' ',' ',
                            ' ',' ',' ',' ',' ',' ',' ',' ',
                            ' ',' ',' ',' ',' ',' ',' ',' ',
                            ' ',' ',' ',' ',' ',' ',' ',' ',
                            'P','P','P','P','P','P','P','P',
                            'R','N','B','Q','K','B','N','R'
                        };
                        if (tokens[1] == "fen") //game from fen
                        {
                            Board.updateFromFen(tokens[2], board);
                            color = tokens[3][0];
                            if(tokens.Length > 9)
                            {
                                Board.updateBoard(tokens[9..], board);
                            }
                        }
                        else if (tokens.Length == 2) //new game
                        {
                            string[] temp = { };
                            Board.updateBoard(temp, board);
                            color = 'w';
                        }
                        else //game from position startpos moves etc
                        {
                            string[] gameMoves = tokens[3..];
                            Board.updateBoard(gameMoves, board);
                            if(gameMoves.Length % 2 == 1)
                            {
                                color = 'b';
                            }
                            else
                            {
                                color = 'w';
                            }
                        }
                        Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, 
                                    ref wPawn, ref wRook, ref wKnight, ref wBishop, ref wQueen, ref wKing,
                                    ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                        //Console.WriteLine(Convert.ToString((long)bPawn, 2));
                        break;
                    default:
                        //Debugger.Launch();
                        break;
                }
            }
        }
        
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
                if((enemy & eBishop) > 0)
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
            if((knightMoves & eKnight) > 0)
            {
                return true;
            }
            //pawn attacks
            
            if (color == 'b')
            {
                ulong pawnAttacksE = ePawn << 9;
                pawnAttacksE &= notHFile;
                ulong pawnAttacksW = ePawn << 7;
                pawnAttacksW &= notAFile;
                if(((pawnAttacksE | pawnAttacksW) & sourceBit) > 0)
                {
                    return true;
                }
            }

            //king attacks
            
            if ((kingAttacks[BitOperations.TrailingZeroCount(eKing)] & sourceBit) > 0)
            {
                return true;
            }

            return false;
        }
        
        

        public static void printBitBoard(ulong bitboard)
        {
            ulong temp = 0b_10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ulong temp2 = temp & bitboard;
                    if (temp2 > 0)
                    {
                        Console.Write(1);
                    }
                    else
                    {
                        Console.Write(0);
                    }
                    temp >>= 1;
                    Console.Write(" ");
                }
                Console.Write("\n");
            }
        }
    }

    
}