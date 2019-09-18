using System.Linq;

namespace Sudoku_V3
{
    public class Program
    {
        public static void Main()
        {



//            const string boardString = "000020000000785000305000408040000020170403065090000030704000106000638000000040000";
//            const string boardString = "801300000000209070004800100010000430090060080036000020009001200080602000000008709";
            const string boardString =
                "080090006000001004020743090000000470901000305072000000090076050700900000500020040";
//            const string boardString =
//                "507100030002084000000000960100020006000900050008000000000000300076000508800041000";

            var boardResults = Board.StringToBoard(boardString);
            var board = new Board(boardResults.ToArray());
            
            board.OpenBoardInBrowserWithInterface();

            var lastBoardState = "";
            while (!board.IsBoardFull() && board.BoardState() != lastBoardState)
            {
                lastBoardState = board.BoardState();
                board.Compute();
            }
            board.BoardToJson(null);

            board.OpenBoardInBrowserWithInterface();
        }
    }
}
