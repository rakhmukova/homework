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
        public bool AreCompatible(int[,] leftMatrix, int[,] rightMatrix) 
            => leftMatrix.GetLength(1) == rightMatrix.GetLength(0);
                
        /// <summary>
        /// returns a product of two matrices multiplication
        /// </summary>        
        public abstract int[,] Multiply(int[,] leftMatrix, int[,] rightMatrix);
    }
}
