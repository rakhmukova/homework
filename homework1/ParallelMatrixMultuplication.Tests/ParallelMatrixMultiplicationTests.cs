using NUnit.Framework;
using ParallelMatrixMultiplication;
using System;

namespace ParallelMatrixMultuplication.Tests
{
    public class ParallelMatrixMultiplicationTests
    {
        static private readonly int threadsNum = Environment.ProcessorCount;

        [Test]
        public void TestNullMatrixIsPassed()
        {
            var leftMatrix = new int[,]
            {
                {1}
            };
            Assert.Throws<ArgumentNullException>(()
                => new ParallelMultiplicationStrategy(threadsNum).MultiplyIfPossible(leftMatrix, null));
        }

        [Test]
        public void TestMultiplyMatrices()
        {
            var leftMatrix = new int[,]
            {
                {1, 2, 3},
                {4, 5, 6}
            };
            var rightMatrix = new int[,]
            {
                {1},
                {2},
                {3}
            };
            var result = new ParallelMultiplicationStrategy(threadsNum).MultiplyIfPossible(leftMatrix, rightMatrix);
            Assert.AreEqual(1, result.GetLength(1));
            Assert.AreEqual(2, result.GetLength(0));
            CollectionAssert.AreEqual(new int[,]
            {
                {14},
                {32}
            }, result);
        }

        [Test]
        public void TestTryMultiplyIncompatibleMatrices()
        {
            var leftMatrix = new int[,]
            {
                {1, 2, 3},
                {4, 5, 6}
            };
            var rightMatrix = new int[,]
            {
                {1},
                {2}
            };
            var strategy = new ParallelMultiplicationStrategy(threadsNum);
            Assert.Throws<ArgumentException>(() => strategy.MultiplyIfPossible(leftMatrix, rightMatrix));
        }
    }
}
