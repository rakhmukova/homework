using System;
using System.IO;

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// represents a class for multiplication of two matrices written in files
    /// </summary>
    public class FileMatrixMultiplicator
    {    
        /// <summary>
        /// a strategy of multiplication
        /// </summary>
        public MultiplicationStrategy Strategy { get; set; }

        /// <summary>
        /// initializes an instance of a FileMatrixMultiplier class
        /// </summary>
        /// <param name="strategy">a preferable multiplication strategy</param>
        public FileMatrixMultiplicator(MultiplicationStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException();
            }
            this.Strategy = strategy;
        }
        /// <summary>
        /// returns true if the data represented in a file is of valid format 
        /// and it is possible to convert it to matrix, otherwise - false
        /// </summary>
        public bool TryConvertToMatrix(string filePath, out int[,] matrix)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("The file doesn't exist.");
            }

            using (StreamReader toCountRowsAndCols = new(filePath), toFillMatrix = new(filePath))
            {
                string line;
                string[] tokens;
                var rowCounter = 0;
                var colCounter = 0;
                line = toCountRowsAndCols.ReadLine();
                if (line == null)
                {
                    matrix = null;
                    return false;
                }
                else
                {
                    ++rowCounter;
                    tokens = line.Split('\t', '\n');
                    colCounter = tokens.Length - 1;
                }

                while ((line = toCountRowsAndCols.ReadLine()) != null)
                {
                    ++rowCounter;
                }
                                
                matrix = new int[rowCounter, colCounter];

                for (var row = 0; row < rowCounter; ++row)
                {
                    line = toFillMatrix.ReadLine();
                    tokens = line.Split('\t', '\n');
                    if (tokens.Length - 1 != colCounter)
                    {
                        matrix = null;
                        return false;
                    }
                    for (var col = 0; col < colCounter; ++col)
                    {
                        if (!int.TryParse(tokens[col], out matrix[row, col]))
                        {
                            matrix = null;
                            return false;
                        }
                    }
                }
            }               
            
            return true;
        }        

        /// <summary>
        /// prints a matrix to a file, separating elements of the same row by tabs
        /// </summary>
        public void PrintMatrixToFile(int[,] matrix, string filePath)
        {
            if (matrix == null || filePath == null)
            {
                throw new ArgumentNullException();
            }

            using (var sw = File.CreateText(filePath))
            {
                for (var row = 0; row < matrix.GetLength(0); ++row)
                {
                    for (var col = 0; col < matrix.GetLength(1); ++col)
                    {
                        sw.Write($"{matrix[row, col]}\t");
                    }
                    sw.Write("\n");
                }
            }
        }

        /// <summary>
        /// prints a product of matrices multiplication to the result file
        /// </summary>
        public void Multiply(string leftMatrixPath, string rightMatrixPath, string resultMatrixPath)
        {
            try
            {
                int[,] left, right, result;
                if (TryConvertToMatrix(leftMatrixPath, out left) 
                    && TryConvertToMatrix(rightMatrixPath, out right))
                {                    
                    result = Strategy.Multiply(left, right);
                    PrintMatrixToFile(result, resultMatrixPath);
                }                
            }
            catch(Exception)
            {
                throw;
            }            
        }       
    }
}
