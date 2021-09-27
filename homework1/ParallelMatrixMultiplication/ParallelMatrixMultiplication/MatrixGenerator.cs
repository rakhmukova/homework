using System;

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// a class to generate matrices
    /// </summary>
    public class MatrixGenerator
    {
        /// <summary>
        /// returns a matrix filled with random numbers
        /// </summary>
        /// <param name="rowNum">a preferable number of rows</param>
        /// <param name="colNum">a preferable number of columns</param>
        /// /// <param name="maxValue">maximum value of an element</param>
        public static int[,] GenerateMatrix(int rowNum, int colNum, int maxValue = 100)
        {
            var matrix = new int[rowNum, colNum];
            var random = new Random();
            for (var row = 0; row < rowNum; ++row)
            {
                for (var col = 0; col < colNum; ++col)
                {
                    matrix[row, col] = random.Next(maxValue);
                }
            }
            return matrix;
        }
    }
}
