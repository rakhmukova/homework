using NUnit.Framework;
using ParallelMatrixMultiplication;

namespace ParallelMatrixMultuplication.Tests
{
    public class SequentialMatrixMultiplicationTests
    {
        [Test]
        public void TestMultiplyMatrices()
        {
            var left = new int[,]
            {
                {9, 13},
                {7, 5}
            };
            var right = new int[,]
            {
                {1, 0},
                {0, 1}
            };
            var result = new SequentialMultiplicationStrategy().MultiplyIfPossible(left, right);
            Assert.AreEqual(2, result.GetLength(1));
            Assert.AreEqual(2, result.GetLength(0));
            CollectionAssert.AreEqual(new int[,]
            {
                {9, 13},
                {7, 5}
            }, result);
        }
    }
}
