using System;
using static Globals;
//TRANSPOSITION TABLE RELATED FUNCTIONS
public static class Zobrist
{
    //init zobrist table 
    public static void initialise()
    {
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                zobristTable[j, i] = 0;
            }
        }
    }

    //init zobrist table with random uint64
    public static void initTable()
    {   
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                Random rnd = new Random();
                zobristTable[j, i] = NextUInt64(rnd);
            }
        }
    }

    public static void printZobristTable()
    {
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                Console.WriteLine(zobristTable[j, i]);
            }
        }
    }

    //compute hash of given board, only used once for every new position, in minimax just xor the move to update
    public static ulong computeHash(char[] board)
    {
        ulong h = 0;
        for (int i = 0; i < 64; i++)
        {
            if (board[i] != ' ')
            {
                int piece = indexOf(board[i]);
                h ^= zobristTable[piece, i];
            }
        }
        return h;
    }

    public static UInt64 NextUInt64(this Random rnd)
    {
        var buffer = new byte[sizeof(UInt64)];
        rnd.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public static ulong getHash(int idx, char piece)
    {
        return zobristTable[indexOf(piece), idx];
    }

    //matches piece to corresponding index of a cell in zobrist table
    public static int indexOf(char piece)
    {
        switch (piece)
        {
            case 'P':
                return 0;
            case 'N':
                return 1;
            case 'B':
                return 2;
            case 'R':
                return 3;
            case 'Q':
                return 4;
            case 'K':
                return 5;
            case 'p':
                return 6;
            case 'n':
                return 7;
            case 'b':
                return 8;
            case 'r':
                return 9;
            case 'q':
                return 10;
            case 'k':
                return 11;
            default: 
                return -1;
        }
    }
}