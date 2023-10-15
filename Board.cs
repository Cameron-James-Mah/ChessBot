using System;
using static ChessBot.Program;
using static Globals;
class Board
{
    //BOARD INIT FUNCTIONS
    public static void makeBoards(ref ulong bPawn, ref ulong bRook, ref ulong bKnight, ref ulong bBishop, ref ulong bQueen, ref ulong bKing,
            ref ulong wPawn, ref ulong wRook, ref ulong wKnight, ref ulong wBishop, ref ulong wQueen, ref ulong wKing,
            ref ulong allPieces, ref ulong empty, char[] board, ref ulong whitePieces, ref ulong blackPieces)
    {
        bPawn = 0; bRook = 0; bKnight = 0; bBishop = 0; bQueen = 0; bKing = 0;
        wPawn = 0; wRook = 0; wKnight = 0; wBishop = 0; wQueen = 0; wKing = 0;
        allPieces = 0; empty = 0;
        ulong idx = 1;
        int cnt = 0;
        for (int i = board.Length - 1; i >= 0; i--)
        {
            if (board[i] == 'p')
            {
                bPawn = bPawn | idx;
                cnt++;
            }
            else if (board[i] == 'r')
            {
                bRook = bRook | idx;
            }
            else if (board[i] == 'n')
            {
                bKnight = bKnight | idx;
            }
            else if (board[i] == 'b')
            {
                bBishop = bBishop | idx;
            }
            else if (board[i] == 'q')
            {
                bQueen = bQueen | idx;
            }
            else if (board[i] == 'k')
            {
                bKing = bKing | idx;
            }
            else if (board[i] == 'P')
            {
                wPawn = wPawn | idx;
            }
            else if (board[i] == 'R')
            {
                wRook = wRook | idx;
            }
            else if (board[i] == 'N')
            {
                wKnight = wKnight | idx;
            }
            else if (board[i] == 'B')
            {
                wBishop = wBishop | idx;
            }
            else if (board[i] == 'Q')
            {
                wQueen = wQueen | idx;
            }
            else if (board[i] == 'K')
            {
                wKing = wKing | idx;
            }
            idx = idx << 1;
        }
        allPieces = bPawn | bRook | bKnight | bBishop | bQueen | bKing | wPawn | wRook | wKnight | wBishop | wQueen | wKing;
        empty = ~allPieces;
        whitePieces = wPawn | wRook | wKnight | wBishop | wQueen | wKing;
        blackPieces = bPawn | bRook | bKnight | bBishop | bQueen | bKing;
        //Console.WriteLine(Convert.ToString((long)bPawn, 2));
        //Console.WriteLine(cnt);
    }

    public static void updateFromFen(string fen, char[] board)
    {
        int idx = 0;
        char[] temp = new char[64] {
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' ',
                ' ',' ',' ',' ',' ',' ',' ',' '
            };
        for (int i = 0; i < fen.Length; i++)
        {
            if (Char.IsNumber(fen[i]))
            {
                idx += fen[i] - '0';
            }
            else if (fen[i] == '/')
            {
                continue;
            }
            else //piece character
            {
                temp[idx] = fen[i];
                idx++;
            }
        }
        temp.CopyTo(board, 0);
        //Console.WriteLine(fen);
        //printBoard(temp);
    }
    public static void updateBoard(string[] moves, char[] board, ref int enPassant, ref ulong castleRights)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            //Console.WriteLine(moves[i]);
            //Console.WriteLine(getCellNumber(moves[i].Substring(0, 2)) + " to " + getCellNumber(moves[i].Substring(2, 2))); DONT DELETE THIS LINE 
            //check en passant
            enPassant = 0;
            if (board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'p' && isCapture(getCellNumber(moves[i].Substring(0, 2)), getCellNumber(moves[i].Substring(2, 2))) && board[63 - getCellNumber(moves[i].Substring(2, 2))] == ' ')
            { //if black pawn moving and move is capture and square im moving to is empty then it is en passant
                board[63 - getCellNumber(moves[i].Substring(2, 2))] = board[63 - getCellNumber(moves[i].Substring(0, 2))];
                board[63 - getCellNumber(moves[i].Substring(2, 2)) - 8] = ' ';
                board[63 - getCellNumber(moves[i].Substring(0, 2))] = ' ';
            }
            else if (board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'P' && isCapture(getCellNumber(moves[i].Substring(0, 2)), getCellNumber(moves[i].Substring(2, 2))) && board[63 - getCellNumber(moves[i].Substring(2, 2))] == ' ')
            {
                board[63 - getCellNumber(moves[i].Substring(2, 2))] = board[63 - getCellNumber(moves[i].Substring(0, 2))];
                board[63 - getCellNumber(moves[i].Substring(2, 2)) + 8] = ' ';
                board[63 - getCellNumber(moves[i].Substring(0, 2))] = ' ';
            }
            else if (board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'P' && isDouble(getCellNumber(moves[i].Substring(0, 2)), getCellNumber(moves[i].Substring(2, 2))) || board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'p' && isDouble(getCellNumber(moves[i].Substring(0, 2)), getCellNumber(moves[i].Substring(2, 2)))) //check for double pawn jump to mark for en passant
            {
                board[63 - getCellNumber(moves[i].Substring(2, 2))] = board[63 - getCellNumber(moves[i].Substring(0, 2))];
                board[63 - getCellNumber(moves[i].Substring(0, 2))] = ' ';
                //mark for en passant
                enPassant = (getCellNumber(moves[i].Substring(2, 2))+ getCellNumber(moves[i].Substring(0, 2)))/2; 
            }
            else if (moves[i].Length == 5) //promotion
            {
                if (board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'p')//black pawn promotion
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2))] = moves[i][4];
                }
                else if (board[63 - getCellNumber(moves[i].Substring(0, 2))] == 'P')//black pawn promotion
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2))] = char.ToUpper(moves[i][4]);
                }
                board[63 - getCellNumber(moves[i].Substring(0, 2))] = ' ';
            }
            else
            {
                board[63 - getCellNumber(moves[i].Substring(2, 2))] = board[63 - getCellNumber(moves[i].Substring(0, 2))];
                board[63 - getCellNumber(moves[i].Substring(0, 2))] = ' ';
                if (board[63 - getCellNumber(moves[i].Substring(2, 2))] == 'k' && getCellNumber(moves[i].Substring(0, 2)) - 2 == getCellNumber(moves[i].Substring(2, 2)))//king side castle black
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) + 1] = ' ';
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) - 1] = 'r';
                }
                else if (board[63 - getCellNumber(moves[i].Substring(2, 2))] == 'K' && getCellNumber(moves[i].Substring(0, 2)) - 2 == getCellNumber(moves[i].Substring(2, 2)))//king side castle white
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) + 1] = ' ';
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) - 1] = 'R';
                }
                else if (board[63 - getCellNumber(moves[i].Substring(2, 2))] == 'k' && getCellNumber(moves[i].Substring(0, 2)) + 2 == getCellNumber(moves[i].Substring(2, 2)))//queen side castle black
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) + 1] = 'r';
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) - 2] = ' ';
                }
                else if (board[63 - getCellNumber(moves[i].Substring(2, 2))] == 'K' && getCellNumber(moves[i].Substring(0, 2)) + 2 == getCellNumber(moves[i].Substring(2, 2)))//queen side castle white
                {
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) + 1] = 'R';
                    board[63 - getCellNumber(moves[i].Substring(2, 2)) - 2] = ' ';
                }
            }
            if(((ulong)1 << getCellNumber(moves[i].Substring(0, 2)) & castleRights) > 0) //if moved an unmoved rook or king, lose castle rights involving that piece
            {
                castleRights ^= (ulong)1 << getCellNumber(moves[i].Substring(0, 2));
            }
            if (((ulong)1 << getCellNumber(moves[i].Substring(2, 2)) & castleRights) > 0) //only relevant for fen given positions, prevent rook or king to move back into castling postion after starting fen pos starting piece off of caslting pos
            {
                castleRights ^= (ulong)1 << getCellNumber(moves[i].Substring(2, 2));
            }
            ulong currHash = Zobrist.computeHash(board);
            if (repetition.ContainsKey(currHash))
            {
                repetition[currHash]++;
            }
            else
            {
                repetition.Add(currHash, 1);
            }
            
        }
        //printBoard(board);
    }
    public static int getCellNumber(string cell)
    {
        int col = cell[0];
        if (cell[0] == 'h')
        {
            col = 0;
        }
        else if (cell[0] == 'g')
        {
            col = 1;
        }
        else if (cell[0] == 'f')
        {
            col = 2;
        }
        else if (cell[0] == 'e')
        {
            col = 3;
        }
        else if (cell[0] == 'd')
        {
            col = 4;
        }
        else if (cell[0] == 'c')
        {
            col = 5;
        }
        else if (cell[0] == 'b')
        {
            col = 6;
        }
        else if (cell[0] == 'a')
        {
            col = 7;
        }
        //int row = ((cell[1] - '0'));
        int res = ((cell[1] - '0') * 8) + (col - 8);

        return res;
    }
    public static void printBoard(char[] board)
    {
        int idx = 0;
        int row = 8;
        Console.WriteLine();
        for (int i = 0; i < 8; i++)
        {
            Console.Write(row.ToString() + "  ");
            row--;
            for (int j = 0; j < 8; j++)
            {
                Console.Write(board[idx].ToString() + ' ');
                idx++;
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.Write("   a ");
        Console.Write("b ");
        Console.Write("c ");
        Console.Write("d ");
        Console.Write("e ");
        Console.Write("f ");
        Console.Write("g ");
        Console.Write("h \n");
    }
    public static bool isCapture(int from, int to) //check for pawn capture
    {
        if (Math.Abs(from - to) == 7 || Math.Abs(from - to) == 9)
        {
            return true;
        }
        return false;
    }

    public static bool isDouble(int from, int to) //check for double jump, used so i can mark pieces available for en passant
    {
        if (Math.Abs(from - to) == 16)
        {
            return true;
        }
        return false;
    }
}
