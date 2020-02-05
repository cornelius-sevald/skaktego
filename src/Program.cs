using System;

namespace skaktego
{
    class Program
    {
        static void Main(string[] args)
        {
            const string boardStr = "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR";
            var board = Board.FromString(8, boardStr);
            System.Console.WriteLine(boardStr);
            System.Console.WriteLine(board.ToString());
        }
    }
}
