﻿using System;
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
}