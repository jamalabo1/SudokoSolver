using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sudoku_V3
{
    public class Board
    {
        private readonly ColBlocks[] _cols;

        public Board(ColBlocks[] cols)
        {
            _cols = cols;
        }

        private ColBlocks[] GetCols()
        {
            return _cols;
        }

        private IEnumerable<Position> GetPositions()
        {
            return _cols.SelectMany(col =>
                col.GetCols().SelectMany(block =>
                    block.GetBlockCols().SelectMany(colBlock => colBlock.GetPositions())));
        }

        private static bool CheckIfValidNumberInCol(Col activeBlock, int number)
        {
            return activeBlock.GetBlockCols().SelectMany(col => col.GetPositions())
                .All(position => position.GetNumber() != number);
        }

        private static bool CheckIfValidNumberInLine(IEnumerable<Position> positions, int number)
        {
            return positions.All(position => position.GetNumber() != number);
        }

        private static bool CheckIfValidNumberInBlock(IEnumerable<BlockCol> blockCols, int number)
        {
            return blockCols
                .SelectMany(cols => cols.GetPositions()).All(position => position.GetNumber() != number);
        }

        private bool CheckIfValidNumberInPosition(Position position, int number)
        {
            if (position.HasNumber) return false;

            var col = GetCol(position.GetPosition().Item2);
            var line = GetLine(position.GetPosition().Item1);
            var block = GetBlock(position.GetBlockId());

            return CheckIfValidNumberInLine(line, number) &&
                   CheckIfValidNumberInCol(col, number) &&
                   CheckIfValidNumberInBlock(block, number);
        }

        // gets available positions in col
        private IEnumerable<Position> GetPossibilitiesInCol(Col col, int number)
        {
            return col.GetBlockCols()
                .SelectMany(block => block.GetPositions())
                .Where(position => CheckIfValidNumberInPosition(position, number))
                .ToList();
        }

        private IEnumerable<Position> GetPossibilitiesInLine(IEnumerable<Position> positions, int number)
        {
            return positions
                .Where(position => !position.HasNumber)
                .Where(position => CheckIfValidNumberInPosition(position, number))
                .ToList();
        }

        private IEnumerable<Position> GetPossibilitiesInBlock(IEnumerable<Position> positions, int number)
        {
            return positions
                .Where(position => !position.HasNumber)
                .Where(position => CheckIfValidNumberInPosition(position, number))
                .ToList();
        }

        private IEnumerable<Position> GetPossibilitiesInBlock(IEnumerable<BlockCol> blockCols, int number)
        {
            return GetPossibilitiesInBlock(blockCols.SelectMany(col => col.GetPositions()), number);
        }

        // get block by id, range: 1-9
        private IEnumerable<BlockCol> GetBlock(int id)
        {
            if (id < 1 || id > 9)
                throw new Exception("Block Id Out Of Range, range: 1-9 (ordered vertically start from top left)");
            var colBlockId = id <= 3 ? 0 : id <= 6 ? 1 : 2;
            var colBlockBlower = colBlockId == 0 ? 0 : colBlockId == 1 ? 3 : 6;
            return GetCols()[colBlockId].GetCols()
                .Select(col => col.GetBlockCols()[id - 1 - colBlockBlower]);
        }

        // gets col by id, range: 1-9
        private Col GetCol(int id)
        {
            if (id < 1 || id > 9)
                throw new Exception("Col Id Out Of Range, range: 1-9 (ordered horizontally from left)");

            return GetCols().SelectMany(blocks => blocks.GetCols()).ToArray()[id - 1];
        }

        // gets line by id, range: 1-9
        public IEnumerable<Position> GetLine(int id)
        {
            if (id < 1 || id > 9)
                throw new Exception("Line Id Out Of Range, range: 1-9 (ordered horizontally from left)");

            var blockColId = id <= 3 ? 0 : id <= 6 ? 1 : 2;
            var lineBlower = blockColId == 0 ? 0 : blockColId == 1 ? 3 : 6;


            return GetCols().SelectMany(colBlocks => colBlocks.GetCols()).Select(block =>
                block.GetBlockCols()[blockColId].GetPositions()[id - 1 - lineBlower]);
        }

        private static void SetPossibilities(IEnumerable<Position> positions, int number)
        {
            foreach (var position in positions)
                position.AddPossibility(number);
        }

        public void Compute()
        {
            foreach (var position in GetPositions()) position.ClearPossibilities();
            for (var number = 1; number <= 9; number++)
            for (var yIndex = 1; yIndex <= 9; yIndex++)
            {
                var colResult = GetPossibilitiesInCol(GetCol(yIndex), number).ToArray();
                var lineResult = GetPossibilitiesInLine(GetLine(yIndex), number).ToArray();
                var blockResult = GetPossibilitiesInBlock(GetBlock(yIndex), number).ToArray();

                SetPossibilities(colResult, number);

                if (colResult.Length == 1)
                    colResult[0].SetNumber(number);
                else if (lineResult.Length == 1)
                    lineResult[0].SetNumber(number);
                else if (blockResult.Length == 1) blockResult[0].SetNumber(number);
            }

            for (var lineIndex = 1; lineIndex <= 9; lineIndex++)
                foreach (var position in GetLine(lineIndex))
                    if (position.GetPossibilities().Count() == 1)
                        position.SetNumber(position.GetPossibilities().ToArray()[0]);
        }

        public bool Compute(bool returnValue)
        {
            var clonePositions = GetPositions().ToList();
            foreach (var position in clonePositions) position.ClearPossibilities();
            for (var number = 1; number <= 9; number++)
            for (var yIndex = 1; yIndex <= 9; yIndex++)
            {
                var colResult = GetPossibilitiesInCol(GetCol(yIndex), number).ToArray();
                var lineResult = GetPossibilitiesInLine(GetLine(yIndex), number).ToArray();
                var blockResult = GetPossibilitiesInBlock(GetBlock(yIndex), number).ToArray();

                SetPossibilities(colResult, number);

                if (colResult.Length == 1)
                    colResult[0].SetNumber(number);
                else if (lineResult.Length == 1)
                    lineResult[0].SetNumber(number);
                else if (blockResult.Length == 1) blockResult[0].SetNumber(number);
            }

            for (var lineIndex = 1; lineIndex <= 9; lineIndex++)
                foreach (var position in GetLine(lineIndex))
                    if (position.GetPossibilities().Count() == 1)
                        position.SetNumber(position.GetPossibilities().ToArray()[0]);

            return clonePositions.All(p => p.HasNumber);
        }

        public bool Compute(bool returnValue, bool setPossibilities, bool breakOnChange)
        {
            foreach (var position in GetPositions()) position.ClearPossibilities();
            for (var number = 1; number <= 9; number++)
            for (var yIndex = 1; yIndex <= 9; yIndex++)
            {
                var state = BoardState();

                var colResult = GetPossibilitiesInCol(GetCol(yIndex), number).ToArray();
                var lineResult = GetPossibilitiesInLine(GetLine(yIndex), number).ToArray();
                var blockResult = GetPossibilitiesInBlock(GetBlock(yIndex), number).ToArray();

                if (setPossibilities) SetPossibilities(colResult, number);

                if (colResult.Length == 1)
                    colResult[0].SetNumber(number);
                else if (lineResult.Length == 1)
                    lineResult[0].SetNumber(number);
                else if (blockResult.Length == 1) blockResult[0].SetNumber(number);

                if (state != BoardState() && breakOnChange) return GetPositions().All(x => x.HasNumber);
            }

            for (var lineIndex = 1; lineIndex <= 9; lineIndex++)
                foreach (var position in GetLine(lineIndex))
                    if (position.GetPossibilities().Count() == 1)
                        position.SetNumber(position.GetPossibilities().ToArray()[0]);

            return GetPositions().All(x => x.HasNumber);
        }

        public IEnumerable<Position> Compute(bool returnValue, bool setPossibilities, bool breakOnChange, bool steps)
        {
            var stepsList = new List<Position>();
            foreach (var position in GetPositions()) position.ClearPossibilities();
            for (var number = 1; number <= 9; number++)
            for (var yIndex = 1; yIndex <= 9; yIndex++)
            {
                var state = BoardState();

                var colResult = GetPossibilitiesInCol(GetCol(yIndex), number).ToArray();
                var lineResult = GetPossibilitiesInLine(GetLine(yIndex), number).ToArray();
                var blockResult = GetPossibilitiesInBlock(GetBlock(yIndex), number).ToArray();

                if (setPossibilities) SetPossibilities(colResult, number);

                if (colResult.Length == 1)
                {
                    colResult[0].SetNumber(number);
                    stepsList.Add(colResult[0]);
                }
                else if (lineResult.Length == 1)
                {
                    lineResult[0].SetNumber(number);
                    stepsList.Add(lineResult[0]);
                }
                else if (blockResult.Length == 1)
                {
                    blockResult[0].SetNumber(number);
                    stepsList.Add(blockResult[0]);
                }

//                if (state != BoardState() && breakOnChange) return stepsList;
            }

            for (var lineIndex = 1; lineIndex <= 9; lineIndex++)
                foreach (var position in GetLine(lineIndex))
                    if (position.GetPossibilities().Count() == 1)
                    {
                        position.SetNumber(position.GetPossibilities().ToArray()[0]);
                        stepsList.Add(position);
                    }

            return stepsList;
        }


        public void Possibilities()
        {
            foreach (var position in GetPositions()) position.ClearPossibilities();
            for (var number = 1; number <= 9; number++)
            for (var yIndex = 1; yIndex <= 9; yIndex++)
            {
                var colResult = GetPossibilitiesInCol(GetCol(yIndex), number).ToArray();
                SetPossibilities(colResult, number);
            }
        }

        public bool IsBoardFull()
        {
            return GetPositions().All(position => position.HasNumber);
        }

        public string BoardState()
        {
            var sb = new StringBuilder();
            foreach (var position in GetPositions())
                sb.Append(position.GetNumber(0));
            return sb.ToString();
        }

        public static ColBlocks[] JsonToBoard(string path)
        {
            var jsonObject = JObject.Parse(File.ReadAllText(path, Encoding.UTF8));
            var data = jsonObject["data"];
            var selectedData = data
                .Select(token => ((int) token["number"],
                        (int) token["Position"]["Row"], // Line
                        (int) token["Position"]["Line"] // Col,
                    ))
                .GroupBy(token => token.Item3); // group by col (Col) 


            return ToBlockCols(selectedData);
        }

        public static IEnumerable<ColBlocks> StringToBoard(string token)
        {
            if (token.Length != 9 * 9) return Array.Empty<ColBlocks>();
            var x = 1;
            var y = 1;
            var cols = Split(token.Select(number =>
            {
                if (x != 10) return new Position(int.Parse(number.ToString()), (x++, y));
                x = 1;
                y++;
                return new Position(int.Parse(number.ToString()), (x++, y));
            }).GroupBy(p => p.GetPosition().Item2).Select(grouping =>
            {
                return new Col(Split(grouping, 3).Select(positions => new BlockCol(positions)).ToArray());
            }), 3);

            return cols.Select(col => new ColBlocks(col));
        }

        private static ColBlocks[] ToBlockCols(IEnumerable<IGrouping<int, (int, int, int)>> collection)
        {
            //FUCK THIS FUNCTION I'LL WORKING ON IT TOMORROW AND THE WHOLE CODE
            var elms = new Position[9, 9];
            var currentState = 0;
            var fuckingIndex = 0;
            foreach (var item in collection)
            {
                foreach (var (item1, item2, item3) in item.ToList())
                {
                    elms[currentState, fuckingIndex] = new Position(item1, (item2, item3));
                    fuckingIndex += 1;
                }

                currentState++;
                fuckingIndex = 0;
            }

            var cols = new Col[9];
            var activeBlockIndexer = 0;

            // down is the smart code, up idk why it's still here but anyway...
            for (var i = 0; i < elms.GetLength(0); i++)
            {
                var element = new Position[9];
                for (var q = 0; q < elms.GetLength(1); q++) element[q] = elms[i, q];

                var results = Split(element, 3);

                var elementBlocks = new BlockCol[3];
                for (var r = 0; r < results.Length; r++) elementBlocks[r] = new BlockCol(results[r]);
                var activeBlock = new Col(elementBlocks);
                cols[activeBlockIndexer++] = activeBlock;
            }

            var colBlocksElements = Split(cols, 3);

            var colBlocks = new ColBlocks[3];

            for (var q = 0; q < colBlocksElements.Length; q++) colBlocks[q] = new ColBlocks(colBlocksElements[q]);


            return colBlocks;
        }

        /// <summary>
        ///     Prints Board in Console, (Deprecated)
        /// </summary>
        public void PrintBoard()
        {
            var cols = GetCols().SelectMany(col => col.GetCols()).ToArray();
            for (var i = 0; i < cols.Length; i++)
            {
                var stack = cols[i];
                var positions = stack.GetBlockCols().SelectMany(col => col.GetPositions()).ToArray();
                foreach (var n in positions)
                {
                    var (y, x) = n.GetPosition();

                    Console.SetCursorPosition(x > 3 ? x * 2 + 1 : x * 2, y * 2);
                    Console.WriteLine(n.ToString());


                    for (var w = 0; w < 9 * 2; w++)
                    {
                        Console.SetCursorPosition(w + 2,
                            i >= 0 && i <= 2 ? 0 : i >= 3 && i <= 5 ? 7 : i != 8 ? 14 - 1 : 14 + 7);
                        Console.WriteLine("-");
                    }
                }

                for (var x = 0; x < 9 * 2; x++)
                {
                    Console.SetCursorPosition(i >= 0 && i <= 2 ? 0 : i >= 3 && i <= 5 ? 7 : i != 8 ? 14 : 14 + 8,
                        x + 2);
                    Console.WriteLine("|");
                }
            }
        }

        public void BoardToJson(string path)
        {
            var data = GetPositions().Select(position => new
            {
                number = position.GetNumber(),
                position = new
                {
                    Col = position.GetPosition().Item2,
                    Row = position.GetPosition().Item1
                }
            });
            var json = JsonConvert.SerializeObject(data);
            if (path == null)
                path = $"{AppDomain.CurrentDomain.BaseDirectory}\\data\\Board-{Guid.NewGuid()}.json";
            File.WriteAllText(path, json);
        }

        public IEnumerable<object> BoardToJson()
        {
            return GetPositions().Select(position => new
            {
                number = position.GetNumber(),
                position = new
                {
                    Col = position.GetPosition().Item2,
                    Row = position.GetPosition().Item1,
                    Block = position.GetBlockId()
                },
                possibilities = position.GetPossibilities()
            });
        }

        public void OpenBoardInBrowserWithInterface()
        {
            var filePath =
                $"{AppDomain.CurrentDomain.BaseDirectory}\\data\\{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{Guid.NewGuid()}.html";

            using (var file =
                new StreamWriter(filePath, true))
            {
                file.WriteLine(Html.GenerateHtmlFileFromBoard(this));
            }

            Process.Start(filePath);
        }


        public static Board GenerateBoard(string stringBoard)
        {
            var rnd = new Random();
            var boardResults = StringToBoard(stringBoard).ToList();
            var board = new Board(boardResults.ToArray());
            var tempBoardResults = StringToBoard(stringBoard).ToList();
            var tempBoard = new Board(tempBoardResults.ToArray());

            var randomCols = Enumerable.Range(1, 9).OrderBy(x => rnd.Next(1, 9 + 1)).ToList();
            var randomRows = Enumerable.Range(1, 9).OrderBy(x => rnd.Next(1, 9 + 1)).ToList();

            var removedNumbers = new List<Position>();

            foreach (var colId in randomCols)
            foreach (var rowId in randomRows)
            {
                var position = board.GetCol(colId).GetPositions().First(x => x.GetPosition().Item1 == rowId);
                var tempPosition = tempBoard.GetCol(colId).GetPositions().First(x => x.GetPosition().Item1 == rowId);

                var positionNumber = position.GetNumber();
                position.RemoveNumber();
                tempPosition.RemoveNumber();
                removedNumbers.Add(tempPosition);
                foreach (var removedPosition in removedNumbers) removedPosition.RemoveNumber();


                var result = tempBoard.Compute(true);

                if (result) continue;
                position.SetNumber(positionNumber);
                tempPosition.SetNumber(positionNumber);
                removedNumbers.RemoveAt(removedNumbers.Count - 1);
            }

            return board;
        }


        private static T[][] Split<T>(IEnumerable<T> arr, int count)
        {
            var z = 0;
            var query = from s in arr
                let num = z++
                group s by num / count
                into g
                select g.ToArray();
            return query.ToArray();
        }
    }
}