namespace Sudoku_V3
{
    public class BlockCol
    {
        private readonly Position[] _numbers;

        public BlockCol(Position[] positions)
        {
            _numbers = positions;
        }

        public Position[] GetPositions()
        {
            return _numbers;
        }
    }
}