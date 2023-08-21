using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
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
            public Move(int s, int d, char p)
            {
                source = s;
                dest = d;
                promotion = p;
                enPassant = false;
                capPassant = -1; //sqaure of captured en passant piece
                castleFrom = -1;
                castleTo = -1;
            }
            public bool enPassant { get; set; }
            public int castleFrom { get; set; }
            public int castleTo { get; set; }
            public int capPassant { get; set; }
            public int source { get; }
            public int dest { get; }
            public char promotion { get; }
        }

        public struct test
        {
            public string fen;
            public ulong result;
            public int depth;
            public test(string fen, ulong result, int depth)
            {
                this.fen = fen;
                this.result = result;
                this.depth = depth;
            }
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

            ulong enPassant = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000; //pawns that can be captured via en passant, 
            char color = 'b';
            
            ulong castleRights = 0b_10001001_00000000_00000000_00000000_00000000_00000000_00000000_10001001; //track if pieces have been moved

            
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
                        /*
                        List<Move> moves = new List<Move>();
                        MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
                        MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
                        MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
                        MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
                        MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
                        MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
                        MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, wRook, );
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
                        
                        for (int i = 0; i < validMoves.Count; i++)
                        {
                            Console.WriteLine("From: " + moves[i].source + " Destination: " + moves[i].dest + "Promotion: " + moves[i].promotion);
                        }
                        
                        if (validMoves.Count == 0)
                        {
                            Console.WriteLine("No moves");
                        }
                        int rnd = new Random().Next(0, validMoves.Count-1);
                        Move bestMove = new Move();
                        bestMove = validMoves[rnd];
                        Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest] + bestMove.promotion);*/
                        /*
                        if(bestMove.promotion == ' ') //not 
                        {
                            Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest] + bestMove.promotion);
                        }
                        else //non promotion move
                        {
                            Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest]);
                        }*/

                        //moves.Clear();
                        //validMoves.Clear();
                        break;
                    case "stop":
                        System.Environment.Exit(0);
                        break;
                    case "p":
                        Board.printBoard(board);
                        break;
                    case "test":
                        //run through perft tests
                        //test 1 
                        int testsPassed = 0;
                        List<test> tests = new List<test>();
                        Console.WriteLine("Test results: ");
                        tests.Add(new test("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 4865609, 5));
                        tests.Add(new test("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 97862, 3));
                        tests.Add(new test("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - ", 674624, 5));
                        tests.Add(new test("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 15833292, 5));
                        tests.Add(new test("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8 ", 2103487, 4));
                        tests.Add(new test("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10 ", 3894594, 4));
                        tests.Add(new test("n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1", 3605103, 5));
                        int en = -1;
                        for(int i = 0; i < tests.Count; i++)
                        {
                            string res = "";
                            string[] fen = tests[i].fen.Trim().Split();
                            Board.updateFromFen(fen[0], board);
                            color = char.Parse(fen[1]);
                            if (en > 0)
                            {
                                enPassant |= ((ulong)1 << en);
                            }
                            Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing,
                                        ref wPawn, ref wRook, ref wKnight, ref wBishop, ref wQueen, ref wKing,
                                        ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                            castleRights = 0b_00001000_00000000_00000000_00000000_00000000_00000000_00000000_00001000;
                            if (fen[2].Contains('k'))
                            {
                                castleRights |= (ulong)1 << 56;
                            }
                            if (fen[2].Contains('q'))
                            {
                                castleRights |= (ulong)1 << 63;
                            }
                            if (fen[2].Contains('K'))
                            {
                                castleRights |= (ulong)1 << 0;
                            }
                            if (fen[2].Contains('Q'))
                            {
                                castleRights |= (ulong)1 << 7;
                            }
                            ulong curr = perft(tests[i].depth, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop,
                            wQueen, wKing, allPieces, empty, board, whitePieces, blackPieces, castleRights, enPassant, color);
                            res += "\n"+tests[i].fen +" Expected nodes: " + tests[i].result + " Traversed nodes: " + curr;
                            if(curr == tests[i].result)
                            {
                                res += " TEST PASSED";
                                testsPassed++;
                            }
                            else
                            {
                                res += " TEST FAILED";
                            }
                            Console.WriteLine(res);
                        }
                        if(testsPassed == tests.Count)
                        {
                            Console.WriteLine("\nALL TESTS PASSED\n");
                        }
                        else
                        {
                            Console.WriteLine("\nTESTING FAILED\n");
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
                        int enP = -1;
                        if (tokens[1] == "fen") //game from fen
                        {
                            Board.updateFromFen(tokens[2], board);
                            color = tokens[3][0];
                            int idx = Array.IndexOf(tokens, "moves") + 1;
                            if (idx < tokens.Length && idx > 0) 
                            {
                                Board.updateBoard(tokens[idx..], board, ref enP, ref castleRights);
                            }
                            castleRights = 0b_00001000_00000000_00000000_00000000_00000000_00000000_00000000_00001000;
                            if (tokens[4].Contains('k'))
                            {
                                castleRights |= (ulong)1 << 56;
                            }
                            if (tokens[4].Contains('q'))
                            {
                                castleRights |= (ulong)1 << 63;
                            }
                            if (tokens[4].Contains('K'))
                            {
                                castleRights |= (ulong)1 << 0;
                            }
                            if (tokens[4].Contains('Q'))
                            {
                                castleRights |= (ulong)1 << 7;
                            }
                        }
                        else if (tokens.Length == 2) //new game
                        {
                            string[] temp = { };
                            Board.updateBoard(temp, board, ref enP, ref castleRights);
                            color = 'w';
                        }
                        else //game from position startpos moves etc
                        {
                            string[] gameMoves = tokens[3..];
                            Board.updateBoard(gameMoves, board, ref enP, ref castleRights);
                            if(gameMoves.Length % 2 == 1)
                            {
                                color = 'b';
                            }
                            else
                            {
                                color = 'w';
                            }
                        }
                        if(enP > 0)
                        {
                            enPassant |= ((ulong)1 << enP);
                        }
                        Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, 
                                    ref wPawn, ref wRook, ref wKnight, ref wBishop, ref wQueen, ref wKing,
                                    ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                        //Console.WriteLine(Convert.ToString((long)bPawn, 2));
                        break;
                    case "perft":
                        perftValue = int.Parse(tokens[1]);
                        Console.WriteLine("Nodes: " + perft(Int32.Parse(tokens[1]), bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop,
                            wQueen, wKing, allPieces, empty, board, whitePieces, blackPieces, castleRights, enPassant, color));

                        perftValue = -1;
                        break;
                    default:
                        //Debugger.Launch();
                        Console.WriteLine("No command");

                        break;
                }
            }
        }

        public static ulong perft(int depth, ulong bPawn, ulong bRook, ulong bKnight, ulong bBishop, ulong bQueen, ulong bKing,
                                             ulong wPawn, ulong wRook, ulong wKnight, ulong wBishop, ulong wQueen, ulong wKing,
                                             ulong allPieces, ulong empty, char[] board, ulong whitePieces, ulong blackPieces,
                                             ulong castleRights, ulong enPassant, char color)
        {
            
            ulong nodes = 0;
            if(depth == 0)
            {
                return 1;
            }
            if(color == 'b')
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
                        if(depth == perftValue)
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
            
            if (color == 'b') //white pawn attacks for black king
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
            Console.Write("\n");
        }
    }

    
}