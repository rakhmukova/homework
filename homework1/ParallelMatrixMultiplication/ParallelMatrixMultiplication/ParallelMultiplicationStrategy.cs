﻿using System;
using System.Threading;

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// A class that represents a strategy for parallel matrix multiplication
    /// </summary>
    public class ParallelMultiplicationStrategy : MultiplicationStrategy
    {
        private int threadsNum;
        /// <summary>
        /// a number of threads engaged while multiplying matrices
        /// </summary>
        public int ThreadsNum 
        {
            get => threadsNum;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The number of threads should be a positive integer.");
                }
                threadsNum = value;
            } 
        }

        /// <summary>
        /// initializes an instance of ParallelMultiplicationStrategy class
        /// </summary>
        /// <param name="threadsNum">a preferable number of threads to engage</param>
        public ParallelMultiplicationStrategy(int threadsNum)
        {
            this.ThreadsNum = threadsNum;
        }

        /// <summary>
        /// multiplies two matrices in a parallel way
        /// </summary>
        protected override int[,] Multiply(int[,] leftMatrix, int[,] rightMatrix)
        {
            var rowsNum = leftMatrix.GetLength(0);
            var colsNum = rightMatrix.GetLength(1);
            var varNum = leftMatrix.GetLength(1);
            var threads = new Thread[ThreadsNum];

            var resultMatrix = new int[rowsNum, colsNum];

            var rowsChunk = rowsNum / ThreadsNum + 1;

            for (var i = 0; i < ThreadsNum; ++i)
            {
                var localI = i;
                threads[i] = new Thread(() =>
                {                    
                    for (var row = localI * rowsChunk;
                        row < (localI + 1) * rowsChunk && row < rowsNum; ++row)
                    {
                        for (var col = 0; col < colsNum ; ++col)
                        {
                            for (var num = 0; num < varNum; ++num)
                            {
                                resultMatrix[row, col] += leftMatrix[row, num] * rightMatrix[num, col];
                            }
                        }
                    }
                });                
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return resultMatrix;
        }
    }
}
