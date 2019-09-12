using System.Collections.Generic;

namespace Sudoku_V3
{
    public class ColBlocks
    {
        private readonly Col[] _blocks;

        public ColBlocks(Col[] blocks)
        {
            _blocks = blocks;
        }

        public IEnumerable<Col> GetCols()
        {
            return _blocks;
        }
    }
}