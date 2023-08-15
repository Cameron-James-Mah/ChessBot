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
            generateTables();
            
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
                        MoveGen.getPawnMoves(color, bPawn, wPawn, empty, ref moves, whitePieces, blackPieces);
                        MoveGen.getKnightMoves(ref moves, color, whitePieces, blackPieces, bKnight, wKnight, empty);
                        /*
                        for (int i = 0; i < moves.Count; i++)
                        {
                            Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest);
                        }*/
                        if(moves.Count == 0)
                        {
                            Console.WriteLine("No moves");
                        }
                        int rnd = new Random().Next(0, moves.Count-1);
                        Move bestMove = new Move();
                        bestMove = moves[rnd];
                        
                        Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest]);
                        moves.Clear();
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
                        for (int i = 0; i < 64; i++)
                        {
                            printBitBoard(bishopAttacks[i]);
                            Console.Write("Bitboard Source: " + i);
                            Console.WriteLine("\n");
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
        public static void generateTables()
        {
            for (int i = 0; i < 64; i++)
            {
                knightAttacks[i] = generateKnightAttack(i);
                bishopAttacks[i] = generateBishopAttacks(i);
            }

        }

        public static void setBit(ref ulong b, int source)
        {
            ulong temp = 1;
            temp <<= source;
            b |= temp;
        }

        //piece attack boards
        public static ulong generateKnightAttack(int source)
        {
            ulong attacks = 0;
            ulong bitboard = 0;
            setBit(ref bitboard, source);
            //north jumps
            if (((bitboard << 6) & notABFile) > 0)
            {
                attacks |= bitboard << 6;
            }
            if (((bitboard << 15) & notAFile) > 0)
            {
                attacks |= bitboard << 15;
            }
            if (((bitboard << 17) & notHFile) > 0)
            {
                attacks |= bitboard << 17;
            }
            if (((bitboard << 10) & notHGFile) > 0)
            {
                attacks |= bitboard << 10;
            }
            //south jumps
            if (((bitboard >> 6) & notHGFile) > 0)
            {
                attacks |= bitboard >> 6;
            }
            if (((bitboard >> 15) & notHFile) > 0)
            {
                attacks |= bitboard >> 15;
            }
            if (((bitboard >> 17) & notAFile) > 0)
            {
                attacks |= bitboard >> 17;
            }
            if (((bitboard >> 10) & notABFile) > 0)
            {
                attacks |= bitboard >> 10;
            }

            ;
            return attacks;
        }
        public static ulong generateBishopAttacks(int source)
        {
            ulong attacks = 0;
            ulong bitboard = 0;
            setBit(ref bitboard, source);
            //SW ray
            int offset = 7;
            while (((bitboard >> offset) & notHFile) > 0)
            {
                bitboard = 0;
                setBit(ref bitboard, source);
                attacks |= bitboard >> offset;
                offset += 7;
            }
            //SE ray
            offset = 9;
            while (((bitboard >> offset) & notAFile) > 0)
            {
                bitboard = 0;
                setBit(ref bitboard, source);
                attacks |= bitboard >> offset;
                offset += 9;
            }
            //NW ray
            offset = 9;
            while (((bitboard << offset) & notHFile) > 0)
            {
                bitboard = 0;
                setBit(ref bitboard, source);
                attacks |= bitboard << offset;
                offset += 9;
            }
            //NE ray
            offset = 7;
            while (((bitboard << offset) & notAFile) > 0)
            {
                bitboard = 0;
                setBit(ref bitboard, source);
                attacks |= bitboard << offset;
                offset += 7;
            }

            return attacks;
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