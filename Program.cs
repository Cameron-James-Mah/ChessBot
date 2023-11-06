using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using static Globals;
using static Position;


//0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;


//maybe bitmask for en passent


//MAYBE TRY GEN PAWN MOVES FIRST

namespace ChessBot
{
    class Program
    {
        static readonly Stopwatch timer = new Stopwatch(); //used for measuring perft performance, also minimax performance

        public class Move //probably refactor thid to globals
        {
            public Move(int s, int d, char p)
            {
                source = s; //source square
                dest = d;  //destination square
                promotion = p; //promotion piece
                enPassant = false; //piece can be captured via en passant
                capPassant = -1; //sqaure of captured en passant piece
                castleFrom = -1; //rook square before castle
                castleTo = -1; //rook square after castle
                moveVal = 0;
            }

            public Move()
            {

            }
            public bool enPassant { get; set; }
            public int castleFrom { get; set; }
            public int castleTo { get; set; }
            public int capPassant { get; set; }
            public int source { get; }
            public int dest { get; }
            public char promotion { get; }

            public int moveVal { get; set; } //move value for move ordering

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
            //board representation used for generating/updating bitboards
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
            char[] board2 = new char[64] {
                'r','n','b','q','k','b','n','r',
                'p','p','p','p','p','p','p','p',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ','P',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                'P','P','P','P',' ','P','P','P',
                'R','N','B','Q','K','B','N','R'
            };

            AttackTables.generateTables(); //generate bitboard attack tables
                                           //piece square table for move ordering

            pieceTables.Add('p', pawnSquaresB);
            pieceTables.Add('P', pawnSquaresW);
            pieceTables.Add('n', knightSquares);
            pieceTables.Add('N', knightSquares);
            pieceTables.Add('b', bishopSquaresB);
            pieceTables.Add('B', bishopSquaresW);
            pieceTables.Add('r', rookSquaresB);
            pieceTables.Add('R', rookSquaresW);
            pieceTables.Add('q', queenSquares);
            pieceTables.Add('Q', queenSquares);
            pieceTables.Add('k', kingSquaresMiddleB);
            pieceTables.Add('K', kingSquaresMiddleW);


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
                        ulong currH = Zobrist.computeHash(board);
                        /*
                        ulong newHash = currHash;
                        Console.WriteLine(newHash);
                        newHash ^= Zobrist.getHash(63 - 11, board[63 - 11]);
                        newHash ^= Zobrist.getHash(63 - 27, board[63 - 11]);
                        Console.WriteLine(newHash);
                        newHash ^= Zobrist.getHash(63 - 27, board[63 - 11]);
                        newHash ^= Zobrist.getHash(63 - 11, board[63 - 11]);
                        Console.WriteLine(newHash);*/
                        /*
                        ulong newH = currH;
                        newH ^= Zobrist.getHash(63 - 11, board[63 - 11]);
                        newH ^= Zobrist.getHash(63 - 27, board[63 - 11]);
                        Console.WriteLine(newH);
                        Console.WriteLine(Zobrist.computeHash(board2));
                        Console.WriteLine();*/
                        //int depth = 6; //temp hardcoded value
                        int age = 0;
                        int time = 5;
                        timer.Start();

                        //DateTime t1 = DateTime.Now;
                        //DateTime t2 = DateTime.Now;
                        Move bestMove = new Move();
                        //Console.WriteLine(depth);
                        int depth;
                        //set a cap of 21 for now
                        for (depth = 2; depth < 7; depth++)
                        {
                            Move currBest = new Move();
                            List<Move> moves = new List<Move>();
                            if (color == 'b') //minimizing
                            {
                                int minEval = 60000; //arbitrary max
                                MoveGen.getPawnMoves(bPawn, empty, ref moves, whitePieces, enPassant, color);
                                MoveGen.getKnightMoves(ref moves, whitePieces, bKnight, empty);
                                MoveGen.getBishopMoves(ref moves, whitePieces, bBishop, allPieces);
                                MoveGen.getRookMoves(ref moves, whitePieces, bRook, allPieces);
                                MoveGen.getBishopMoves(ref moves, whitePieces, bQueen, allPieces);
                                MoveGen.getRookMoves(ref moves, whitePieces, bQueen, allPieces);
                                MoveGen.getKingMoves(ref moves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);

                                for (int i = 0; i < moves.Count && timer.Elapsed.Seconds < time; i++)
                                {
                                    char[] tempBoard = new char[64];
                                    board.CopyTo(tempBoard, 0);
                                    ulong newHash = currH;
                                    newHash ^= Zobrist.getHash(63 - moves[i].source, board[63 - moves[i].source]);
                                    if (moves[i].promotion != ' ')
                                    {
                                        tempBoard[63 - moves[i].dest] = moves[i].promotion;
                                        newHash ^= Zobrist.getHash(63 - moves[i].dest, moves[i].promotion);
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
                                    if (moves[i].castleFrom >= 0) //update rook position when castling
                                    {
                                        tempBoard[63 - moves[i].castleTo] = tempBoard[63 - moves[i].castleFrom];
                                        tempBoard[63 - moves[i].castleFrom] = ' ';
                                        newHash ^= Zobrist.getHash(63 - moves[i].castleTo, board[63 - moves[i].castleFrom]);
                                        newHash ^= Zobrist.getHash(63 - moves[i].castleFrom, board[63 - moves[i].castleFrom]);
                                    }
                                    if (board[63 - moves[i].dest] != ' ') //if capturing then update hash
                                    {
                                        newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].dest]);
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
                                        newHash = Zobrist.computeHash(tempBoard);
                                        if ((castleRights & ((ulong)1 << moves[i].source)) > 0)
                                        {
                                            newCastleRights = castleRights ^ ((ulong)1 << moves[i].source);
                                            newHash ^= newCastleRights;
                                        }
                                        //validMoves.Add(moves[i]); 
                                        int temp = minimax(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'w', int.MinValue, int.MaxValue, newHash, age);
                                        if (temp < minEval)
                                        {
                                            currBest = moves[i];
                                            minEval = Math.Min(temp, minEval);
                                            blackTable[currH] = new Entry(minEval, depth, moves[i], age);
                                        }
                                        
                                    }
                                    Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                                        ref wQueen, ref wKing, ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                                }
                                //Console.WriteLine(minEval);
                                bPVTable[currH] = new Entry(minEval, depth, currBest, age);
                            }
                            else //maximizing
                            {
                                int maxEval = -60000; //arbitrary min
                                MoveGen.getPawnMoves(wPawn, empty, ref moves, blackPieces, enPassant, color);
                                MoveGen.getKnightMoves(ref moves, blackPieces, wKnight, empty);
                                MoveGen.getBishopMoves(ref moves, blackPieces, wBishop, allPieces);
                                MoveGen.getRookMoves(ref moves, blackPieces, wRook, allPieces);
                                MoveGen.getBishopMoves(ref moves, blackPieces, wQueen, allPieces);
                                MoveGen.getRookMoves(ref moves, blackPieces, wQueen, allPieces);
                                MoveGen.getKingMoves(ref moves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);

                                for (int i = 0; i < moves.Count && timer.Elapsed.Seconds < time; i++)
                                {
                                    char[] tempBoard = new char[64];
                                    board.CopyTo(tempBoard, 0);
                                    ulong newHash = currH;
                                    newHash ^= Zobrist.getHash(63 - moves[i].source, board[63 - moves[i].source]);
                                    if (moves[i].promotion != ' ')
                                    {
                                        tempBoard[63 - moves[i].dest] = moves[i].promotion;
                                        newHash ^= Zobrist.getHash(63 - moves[i].dest, moves[i].promotion);
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
                                    if (board[63 - moves[i].dest] != ' ') //if capturing then update hash
                                    {
                                        newHash ^= Zobrist.getHash(63 - moves[i].dest, board[63 - moves[i].dest]);
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
                                        newHash = Zobrist.computeHash(tempBoard);
                                        if ((castleRights & ((ulong)1 << moves[i].source)) > 0) //if castling move then update castleRights, I THINK THIS IS MY ISSUE 
                                        {
                                            newCastleRights = castleRights ^ ((ulong)1 << moves[i].source);
                                            newHash ^= newCastleRights;
                                            //printBitBoard(newCastleRights);
                                        }

                                        int temp = minimax(depth - 1, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop, wQueen, wKing, allPieces, empty, tempBoard, whitePieces, blackPieces, newCastleRights, newEnPassant, 'b', int.MinValue, int.MaxValue, newHash, age);
                                        if (temp > maxEval)
                                        {
                                            currBest = moves[i];
                                            maxEval = Math.Max(temp, maxEval);
                                            whiteTable[currH] = new Entry(maxEval, depth, moves[i], age);
                                        }
                                        
                                    }
                                    Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                                        ref wQueen, ref wKing, ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                                }
                                wPVTable[currH] = new Entry(maxEval, depth, currBest, age);
                            }
                            if(timer.Elapsed.Seconds >= time)
                            {
                                break;
                            }
                            bestMove = currBest;
                            age++;

                        }

                        /*
                        for(int i = 0; i < moves.Count; i++)
                        {
                            Console.WriteLine(notation[moves[i].source] + notation[moves[i].dest]);
                        }*/
                        if (bestMove.promotion != ' ') //promotion move
                        {
                            Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest] + bestMove.promotion);
                        }
                        else //non promotion move
                        {
                            Console.WriteLine("bestmove " + notation[bestMove.source] + notation[bestMove.dest]);
                        }
                        timer.Stop();
                        Console.WriteLine("Elapsed time: " + timer.Elapsed.ToString() + " Interrupted Depth: " + depth);
                        //Console.WriteLine("Seconds elapsed: " + timer.Elapsed.Seconds.ToString() + " Depth: " + depth);
                        timer.Reset();
                        whiteTable.Clear();
                        blackTable.Clear();
                        wPVTable.Clear();
                        bPVTable.Clear();
                        break;
                    case "stop":
                        System.Environment.Exit(0);
                        break;
                    case "p":
                        Board.printBoard(board);
                        Console.WriteLine("\nHash: " + Zobrist.computeHash(board));
                        break;
                    case "t":
                        //check moves for position
                        
                        List<Move> testMoves = new List<Move>();
                        if(color == 'b')
                        {
                            MoveGen.getPawnMoves(bPawn, empty, ref testMoves, whitePieces, enPassant, color);
                            MoveGen.getKnightMoves(ref testMoves, whitePieces, bKnight, empty);
                            MoveGen.getBishopMoves(ref testMoves, whitePieces, bBishop, allPieces);
                            MoveGen.getRookMoves(ref testMoves, whitePieces, bRook, allPieces);
                            MoveGen.getBishopMoves(ref testMoves, whitePieces, bQueen, allPieces);
                            MoveGen.getRookMoves(ref testMoves, whitePieces, bQueen, allPieces);
                            MoveGen.getKingMoves(ref testMoves, whitePieces, bKing, empty, castleRights, allPieces, bRook, wPawn, wRook, wKnight, wBishop, wQueen, wKing, color);
                        }
                        else
                        {
                            MoveGen.getPawnMoves(wPawn, empty, ref testMoves, blackPieces, enPassant, color);
                            MoveGen.getKnightMoves(ref testMoves, blackPieces, wKnight, empty);
                            MoveGen.getBishopMoves(ref testMoves, blackPieces, wBishop, allPieces);
                            MoveGen.getRookMoves(ref testMoves, blackPieces, wRook, allPieces);
                            MoveGen.getBishopMoves(ref testMoves, blackPieces, wQueen, allPieces);
                            MoveGen.getRookMoves(ref testMoves, blackPieces, wQueen, allPieces);
                            MoveGen.getKingMoves(ref testMoves, blackPieces, wKing, empty, castleRights, allPieces, wRook, bPawn, bRook, bKnight, bBishop, bQueen, bKing, color);
                        }
                        
                        char[] tempBoard2 = new char[64];
                        board.CopyTo(tempBoard2, 0);
                        List<Move> validMoves = new List<Move>();
                        for (int i = 0; i < testMoves.Count; i++)
                        {
                            tempBoard2[63 - testMoves[i].dest] = tempBoard2[63 - testMoves[i].source];
                            tempBoard2[63 - testMoves[i].source] = ' ';
                            Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing, ref wPawn, ref wRook, ref wKnight, ref wBishop,
                                ref wQueen, ref wKing, ref allPieces, ref empty, tempBoard2, ref whitePieces, ref blackPieces);

                            int kingSource = 0;
                            if (color == 'b')
                            {
                                kingSource = BitOperations.TrailingZeroCount(bKing);
                            }
                            else
                            {
                                kingSource = BitOperations.TrailingZeroCount(wKing);
                            }
                            
                            if (!isSquareAttacked(kingSource, wBishop, wRook, wKnight, wQueen, wPawn, wKing, allPieces, color))
                            {
                                validMoves.Add(testMoves[i]);
                            }
                            board.CopyTo(tempBoard2, 0);
                        }

                        for (int i = 0; i < validMoves.Count; i++)
                        {
                            Console.WriteLine("From: " + notation[testMoves[i].source] + "" + notation[testMoves[i].dest] + " Promotion: " + testMoves[i].promotion);
                        }

                        if (validMoves.Count == 0)
                        {
                            Console.WriteLine("No moves");
                        }
                        
                        //checking isSquareAttacked function
                        /*
                        int kingSrc = BitOperations.TrailingZeroCount(wKing);
                        Console.WriteLine(Position.isSquareAttacked(kingSrc, bBishop, bRook, bKnight, bQueen, bPawn, bKing, allPieces, 'w'));*/

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
                        timer.Start();
                        for (int i = 0; i < tests.Count; i++)
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
                            ulong curr = Perft.perft(tests[i].depth, bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop,
                            wQueen, wKing, allPieces, empty, board, whitePieces, blackPieces, castleRights, enPassant, color);
                            res += "\n" + tests[i].fen + " Expected nodes: " + tests[i].result + " Traversed nodes: " + curr;
                            if (curr == tests[i].result)
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
                        if (testsPassed == tests.Count)
                        {
                            Console.WriteLine("\nALL TESTS PASSED\n");
                        }
                        else
                        {
                            Console.WriteLine("\nTESTING FAILED\n");
                        }
                        timer.Stop();
                        Console.WriteLine("Elapsed time: " + timer.Elapsed.ToString());
                        timer.Reset();
                        break;
                    case "position":
                        //zobritst init
                        enPassant = 0;
                        Zobrist.initialise();
                        Zobrist.initTable();
                        repetition.Clear();
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
                            int idx = Array.IndexOf(tokens, "moves") + 1;
                            color = tokens[3][0];
                            if (idx != -1) //no moves yet
                            {
                                if (tokens[idx..].Length % 2 == 1)
                                {
                                    if (color == 'w')
                                    {
                                        color = 'b';
                                    }
                                    else
                                    {
                                        color = 'w';
                                    }
                                }
                            }

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
                            if (gameMoves.Length % 2 == 1)
                            {
                                color = 'b';
                            }
                            else
                            {
                                color = 'w';
                            }
                        }
                        if (enP > 0)
                        {
                            enPassant |= ((ulong)1 << enP);
                        }
                        Board.makeBoards(ref bPawn, ref bRook, ref bKnight, ref bBishop, ref bQueen, ref bKing,
                                    ref wPawn, ref wRook, ref wKnight, ref wBishop, ref wQueen, ref wKing,
                                    ref allPieces, ref empty, board, ref whitePieces, ref blackPieces);
                        //Console.WriteLine(Convert.ToString((long)bPawn, 2));
                        /*
                        foreach(var obj in repetition)
                        {
                            Console.WriteLine(obj.Key + ": " + obj.Value);
                        }*/
                        break;
                    case "perft":
                        perftValue = int.Parse(tokens[1]);
                        timer.Start();
                        Console.WriteLine("Nodes: " + Perft.perft(Int32.Parse(tokens[1]), bPawn, bRook, bKnight, bBishop, bQueen, bKing, wPawn, wRook, wKnight, wBishop,
                            wQueen, wKing, allPieces, empty, board, whitePieces, blackPieces, castleRights, enPassant, color));
                        timer.Stop();
                        Console.WriteLine("\nElapsed time: " + timer.Elapsed.ToString());
                        timer.Reset();
                        perftValue = -1;
                        break;
                    case "attacks":
                        for (int i = 0; i < 64; i++)
                        {
                            printBitBoard(pawnAttacksW[i]);
                        }
                        break;

                    default:
                        //Debugger.Launch();
                        Console.WriteLine("No command");

                        break;
                }
            }
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