namespace ParallelMatrixMultiplication
{
    public class SequentialMultiplicationStrategy : MultiplicationStrategy
    {      
        /// <summary>
        /// multiplies two matrices in a sequential way
        /// </summary>
        protected override int[,] Multiply(int[,] leftMatrix, int[,] rightMatrix)
        {
            var rowsNum = leftMatrix.GetLength(0);
            var colsNum = rightMatrix.GetLength(1);
            var varNum = leftMatrix.GetLength(1);
            var resultMatrix = new int[rowsNum, colsNum];

            for (var row = 0; row < rowsNum; ++row)
            {
                for (var col = 0; col < colsNum; ++col)
                {
                    for (var i = 0; i < varNum; ++i)
                    {
                        resultMatrix[row, col] += leftMatrix[row, i] * rightMatrix[i, col];
                    }
                }
            }
            return resultMatrix;
        }
    }
}
