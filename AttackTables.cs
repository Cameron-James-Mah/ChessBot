using System;
using static Globals;
public class AttackTables
{
    public static void generateTables()
    {
        for (int i = 0; i < 64; i++)
        {
            knightAttacks[i] = generateKnightAttack(i);
            generateBishopAttacks(i);
            generateRookAttacks(i);
            kingAttacks[i] = generateKingAttacks(i);
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
        return attacks;
    }
    public static void generateBishopAttacks(int source)
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
        bishopAttacks[source, 0] = attacks;
        attacks = 0;
        //SE ray
        offset = 9;
        while (((bitboard >> offset) & notAFile) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard >> offset;
            offset += 9;
        }
        bishopAttacks[source, 1] = attacks;
        attacks = 0;
        //NW ray
        offset = 9;
        while (((bitboard << offset) & notHFile) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard << offset;
            offset += 9;
        }
        bishopAttacks[source, 2] = attacks;
        attacks = 0;
        //NE ray
        offset = 7;
        while (((bitboard << offset) & notAFile) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard << offset;
            offset += 7;
        }
        bishopAttacks[source, 3] = attacks;
        attacks = 0;
    }

    public static void generateRookAttacks(int source)
    {
        ulong attacks = 0;
        ulong bitboard = 0;
        setBit(ref bitboard, source);
        //South ray
        int offset = 8;
        while (((bitboard >> offset) & (~row7)) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard >> offset;
            offset += 8;
        }
        rookAttacks[source, 0] = attacks;
        attacks = 0;
        //East ray
        offset = 1;
        while (((bitboard >> offset) & notAFile) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard >> offset;
            offset += 1;
        }
        rookAttacks[source, 1] = attacks;
        attacks = 0;
        //West ray
        offset = 1;
        while (((bitboard << offset) & notHFile) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard << offset;
            offset += 1;
        }
        rookAttacks[source, 2] = attacks;
        attacks = 0;
        //North ray
        offset = 8;
        while (((bitboard << offset) & (~row0)) > 0)
        {
            bitboard = 0;
            setBit(ref bitboard, source);
            attacks |= bitboard << offset;
            offset += 8;
        }
        rookAttacks[source, 3] = attacks;
    }
    public static ulong generateKingAttacks(int source)
    {
        ulong attacks = 0;
        ulong bitboard = 0;
        setBit(ref bitboard, source);
        //W
        if (((bitboard << 1) & notHFile) > 0)
        {
            attacks |= bitboard << 1;
        }
        //NW
        if (((bitboard << 9) & notHFile) > 0)
        {
            attacks |= bitboard << 9;
        }
        //N
        if (((bitboard << 8)) > 0)
        {
            attacks |= bitboard << 8;
        }
        //NE
        if (((bitboard << 7) & notAFile) > 0)
        {
            attacks |= bitboard << 7;
        }

        //E
        if (((bitboard >> 1) & notAFile) > 0)
        {
            attacks |= bitboard >> 1;
        }
        //SE
        if (((bitboard >> 9) & notAFile) > 0)
        {
            attacks |= bitboard >> 9;
        }
        //S
        if (((bitboard >> 8)) > 0)
        {
            attacks |= bitboard >> 8;
        }
        //SW
        if (((bitboard >> 7) & notHFile) > 0)
        {
            attacks |= bitboard >> 7;
        }
        return attacks;
    }
}
