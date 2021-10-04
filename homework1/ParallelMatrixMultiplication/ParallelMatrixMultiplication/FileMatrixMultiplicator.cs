using System;
using System.IO;

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// represents a class for multiplication of two matrices written in files
    /// </summary>
    public class FileMatrixMultiplicator
    {
        private MultiplicationStrategy strategy;

        /// <summary>
        /// a strategy of multiplication
        /// </summary>
        public MultiplicationStrategy Strategy
        {
            get => strategy;
            set => strategy = value?? throw new ArgumentNullException();
        }

        /// <summary>
        /// initializes an instance of a FileMatrixMultiplier class
        /// </summary>
        /// <param name="strategy">a preferable multiplication strategy</param>
        public FileMatrixMultiplicator(MultiplicationStrategy strategy)
        {
            this.Strategy = strategy;
        }

        /// <summary>
        /// returns true if the data represented in a file is of valid format 
        /// and it is possible to convert it to matrix, otherwise - false
        /// </summary>
        public static int[,] ConvertToMatrix(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            using var toCountRowsAndCols = new StreamReader(filePath);            
            var line = toCountRowsAndCols.ReadLine();
            if (line == null)
            {
                throw new FormatException("The file is empty.");
            }

            var delimChars = new char[] {'\n', '\t'};
            var tokens = line.Split(delimChars, StringSplitOptions.RemoveEmptyEntries);
            var colCounter = tokens.Length;
            var rowCounter = 1;
            line = toCountRowsAndCols.ReadLine();
            while (line != null)
            {
                line = toCountRowsAndCols.ReadLine();
                ++rowCounter;
            }

            var matrix = new int[rowCounter, colCounter];

            using var toFillMatrix = new StreamReader(filePath);
            for (var row = 0; row < rowCounter; ++row)
            {
                line = toFillMatrix.ReadLine();
                tokens = line.Split(delimChars, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != colCounter)
                {
                    throw new FormatException("The number of elements in a row is different.");
                }
                for (var col = 0; col < colCounter; ++col)
                {
                    if (!int.TryParse(tokens[col], out matrix[row, col]))
                    {
                        throw new FormatException("Data format is invalid.");
                    }
                }
            }
            return matrix;
        }        

        /// <summary>
        /// prints a matrix to a file, separating elements of the same row by tabs
        /// </summary>
        public static void PrintMatrixToFile(int[,] matrix, string filePath)
        {
            if (matrix == null || filePath == null)
            {
                throw new ArgumentNullException();
            }

            using var streamWriter = File.CreateText(filePath);
            for (var row = 0; row < matrix.GetLength(0); ++row)
            {
                for (var col = 0; col < matrix.GetLength(1); ++col)
                {
                    streamWriter.Write($"{matrix[row, col]}\t");
                }
                streamWriter.Write("\n");
            }
        }

        /// <summary>
        /// prints a product of matrices multiplication to the result file
        /// </summary>
        public void Multiply(string leftMatrixPath, string rightMatrixPath, string resultMatrixPath)
        {
            var left = ConvertToMatrix(leftMatrixPath);
            var right = ConvertToMatrix(rightMatrixPath);
            var result = Strategy.MultiplyIfPossible(left, right);
            PrintMatrixToFile(result, resultMatrixPath);
        }       
    }
}
