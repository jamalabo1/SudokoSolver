using System.Collections.Generic;
using System.Linq;

namespace Sudoku_V3
{
    public class Position
    {
        private readonly (int, int) _position;
        private readonly List<int> _possibilities = new List<int>();
        private int _number;

        public Position(int? number, (int, int) position)
        {
            if (number.HasValue && number != 0)
            {
                _number = (int) number;
                HasNumber = true;
            }
            else
            {
                _number = -1;
                HasNumber = false;
            }

            _position = position;
        }

        public bool HasNumber { get; private set; }

        // (x, y) = (col, col)
        public (int, int) GetPosition()
        {
            return _position;
        }

        public void SetNumber(int number)
        {
            _possibilities.Clear();
            _number = number;
            HasNumber = true;
        }

        public void RemoveNumber()
        {
            _possibilities.Clear();
            _number = -1;
            HasNumber = false;
        }

        public void AddPossibility(int number)
        {
            _possibilities.Add(number);
        }

        public void ClearPossibilities()
        {
            _possibilities.Clear();
        }

        public IEnumerable<int> GetPossibilities()
        {
            return _possibilities;
        }

        public int GetBlockId()
        {
            var colPossibility =
                _position.Item2 <= 3 ? new[] {1, 2, 3} :
                _position.Item2 <= 6 ? new[] {4, 5, 6} :
                new[] {7, 8, 9};


            var linePossibility =
                _position.Item1 <= 3 ? new[] {1, 4, 7} :
                _position.Item1 <= 6 ? new[] {2, 5, 8} :
                new[] {3, 6, 9};


            return colPossibility
                .FirstOrDefault(col => { return linePossibility.FirstOrDefault(line => line == col) != 0; });
        }


        public int GetNumber()
        {
            return _number;
        }

        public int GetNumber(int emptyReplace)
        {
            return HasNumber ? _number : emptyReplace;
        }

        public override string ToString()
        {
            return HasNumber ? GetNumber().ToString() : "";
        }
    }
}