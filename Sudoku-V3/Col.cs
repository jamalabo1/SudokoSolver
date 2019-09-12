using System.Collections.Generic;
using System.Linq;

namespace Sudoku_V3
{
    public class Col
    {
        private readonly BlockCol[] _cols;

        public Col(BlockCol[] cols)
        {
            _cols = cols;
        }

        public BlockCol[] GetBlockCols()
        {
            return _cols;
        }

        public IEnumerable<Position> GetPositions()
        {
            return _cols.SelectMany(col => col.GetPositions());
        }
    }
}