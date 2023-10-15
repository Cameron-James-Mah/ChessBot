﻿using System;
using System.Collections;
using static ChessBot.Program;

//0b_11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111;
class Globals
{
    //row bitmasks
    public const ulong row0 = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111;
    public const ulong row1 = 0b_00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000;
    public const ulong row2 = 0b_00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000;
    public const ulong row3 = 0b_00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000;
    public const ulong row4 = 0b_00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000;
    public const ulong row5 = 0b_00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000;
    public const ulong row6 = 0b_00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000;
    public const ulong row7 = 0b_11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

    //column bitmasks
    public const ulong notAFile = 0b_01111111_01111111_01111111_01111111_01111111_01111111_01111111_01111111;
    public const ulong notABFile = 0b_00111111_00111111_00111111_00111111_00111111_00111111_00111111_00111111;
    public const ulong notHFile = 0b_11111110_11111110_11111110_11111110_11111110_11111110_11111110_11111110;
    public const ulong notHGFile = 0b_11111100_11111100_11111100_11111100_11111100_11111100_11111100_11111100;

    //row and column bitmasks for bishop attack table generation
    public const ulong notNEBorder = 0b_00000000_11111110_11111110_11111110_11111110_11111110_11111110_11111110;
    public const ulong notNWBorder = 0b_00000000_01111111_01111111_01111111_01111111_01111111_01111111_01111111;
    public const ulong notSWBorder = 0b_01111111_01111111_01111111_01111111_01111111_01111111_01111111_00000000;
    public const ulong notSEBorder = 0b_11111110_11111110_11111110_11111110_11111110_11111110_11111110_00000000;

    //attack tables
    public static ulong[] knightAttacks = new ulong[64];
    public static ulong[,] bishopAttacks = new ulong[64, 4]; //0 = SW, 1 = SE, 2 = NW, 3 = NE
    public static ulong[,] rookAttacks = new ulong[64, 4]; //0 = S, 1 = E, 2 = W, 3 = N
    public static ulong[] kingAttacks = new ulong[64];

    public static int perftValue = -1;
    //for converting square number(bit index from left of bitboard) to notation
    public static string[] notation = new string[64] {
                "h1", "g1", "f1", "e1", "d1", "c1", "b1", "a1",
                "h2", "g2", "f2", "e2", "d2", "c2", "b2", "a2",
                "h3", "g3", "f3", "e3", "d3", "c3", "b3", "a3",
                "h4", "g4", "f4", "e4", "d4", "c4", "b4", "a4",
                "h5", "g5", "f5", "e5", "d5", "c5", "b5", "a5",
                "h6", "g6", "f6", "e6", "d6", "c6", "b6", "a6",
                "h7", "g7", "f7", "e7", "d7", "c7", "b7", "a7",
                "h8", "g8", "f8", "e8", "d8", "c8", "b8", "a8",
            };
    public struct Entry
    {
        public Entry(int value, int depth, Move mv, bool trueValue)
        {
            //Do i need true value? just did alpha/beta for checkers
            //this.alpha = alpha;
            //this.beta = beta;
            this.depth = depth;
            this.value = value;
            this.mv = mv;
            this.trueValue = trueValue;
            //this.key = key;
            //this.board = board;
        }
        
        //public int alpha { get; set; }
        //public int beta { get; set; }
        public int depth { get; set; } 
        public int value { get; set; }

        public Move mv { get; set; }

        public bool trueValue { get; set; }

    }

    public static ulong[,] zobristTable = new ulong[12, 64]; //zobrist table
    public static Dictionary<ulong, Entry> whiteTable = new Dictionary<ulong, Entry>(); //transposition table for white, should i maybe init with size and load factor later?
    public static Dictionary<ulong, Entry> blackTable = new Dictionary<ulong, Entry>(); //transposition table for black, should i maybe init with size and load factor later?
    //public static ulong whiteHash = Zobrist.NextUInt64(new Random());
    //public static ulong blackHash = Zobrist.NextUInt64(new Random());

    public static Dictionary<ulong, int> repetition = new Dictionary<ulong, int>(); //<board hash, occurences>for checking threefold repetition as i am iterative over moves of given position increment occurence of hash position

    /*
     * 
     * 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
     * */


    //piece evaluation squares 
    //Values from https://www.chessprogramming.org/Simplified_Evaluation_Function
    //queen and knight values dont care about color
    public static int[] pawnSquaresW = new int[64] {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 10, 25, 25, 10,  5,  5,
         0,  0,  0, 20, 20,  0,  0,  0,
         5, -5,-10,  0,  0,-10, -5,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    public static int[] pawnSquaresB = new int[64] {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10,-20,-20, 10, 10,  5,
        5, -5,-10,  0,  0,-10, -5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5,  5, 10, 25, 25, 10,  5,  5,
        10, 10, 20, 30, 30, 20, 10, 10,
        50, 50, 50, 50, 50, 50, 50, 50,
        0,  0,  0,  0,  0,  0,  0,  0
    };
    


    public static int[] knightSquares = new int[64] {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    };

    public static int[] bishopSquaresW = new int[64]
    {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    };

    public static int[] bishopSquaresB = new int[64]
    {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    };



    public static int[] rookSquaresW = new int[64]
    {
        0,  0,  0,  0,  0,  0,  0,  0,
          5, 10, 10, 10, 10, 10, 10,  5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
         -5,  0,  0,  0,  0,  0,  0, -5,
          0,  2,  3,  5,  5,  3,  2,  0
    };

    public static int[] rookSquaresB = new int[64]
    {
        0,  2,  3,  5,  5,  3,  2,  0,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        5, 10, 10, 10, 10, 10, 10,  5,
        0,  0,  0,  0,  0,  0,  0,  0,
    };

    public static int[] queenSquares = new int[64]
    {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
         -5,  0,  5,  5,  5,  5,  0, -5,
          0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };

    public static int[] kingSquaresMiddleW = new int[64]
    {
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
         20, 20,  0,  0,  0,  0, 20, 20,
         20, 30, 10,  0,  0, 10, 30, 20
    };

    public static int[] kingSquaresMiddleB = new int[64] {
        20, 30, 10,  0,  0, 10, 30, 20,
        20, 20,  0,  0,  0,  0, 20, 20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
    };


    public static int[] kingSquaresEnd = new int[64]
    {
        -50,-40,-30,-20,-20,-30,-40,-50,
        -40,-30, 0,  0,  0, 0, -30, -40,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -40,-30,  0,  0,  0,  0,-30,-40,
        -50,-40,-30,-20,-20,-30,-40,-50
    };

    public static int[] cornerKingSquares = new int[64]
    {
        -100,-80,-60,-20,-20,-60,-80,-100,
        -80,-60, 0,  0,  0, 0, -60, -80,
        -60,-10, 20, 30, 30, 20,-10,-60,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -60,-10, 20, 30, 30, 20,-10,-60,
        -80,-60,  0,  0,  0,  0,-60,-80,
        -100,-80,-60,-20,-20,-60,-80,-100
    };


    public static Dictionary<char, int[]> pieceTables = new Dictionary<char, int[]>(); //piece square tables for move ordering, p will map to black pawn piece table, etc
    
}