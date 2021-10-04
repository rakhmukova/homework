using System;

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// An abstract class that represents a strategy for matrix multiplication
    /// </summary>
    public abstract class MultiplicationStrategy
    {
        /// <summary>
        /// check whether two matrices are compatible
        /// </summary>
        public static bool AreCompatible(int[,] leftMatrix, int[,] rightMatrix) 
            => leftMatrix.GetLength(1) == rightMatrix.GetLength(0);

        /// <summary>
        /// returns a product of two matrices multiplication if the multiplication is possible
        /// </summary>       
        public int[,] MultiplyIfPossible(int[,] leftMatrix, int[,] rightMatrix)
        {
            if (leftMatrix == null || rightMatrix == null)
            {
                throw new ArgumentNullException();
            }

            if (!AreCompatible(leftMatrix, rightMatrix))
            {
                throw new ArgumentException("The matrices are incompatible.");
            }

            return Multiply(leftMatrix, rightMatrix);
        }

        /// <summary>
        /// returns a product of two matrices multiplication
        /// </summary>        
        protected abstract int[,] Multiply(int[,] leftMatrix, int[,] rightMatrix);
    }
}
