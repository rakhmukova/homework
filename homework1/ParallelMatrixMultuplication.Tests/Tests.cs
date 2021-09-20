using NUnit.Framework;
using ParallelMatrixMultiplication;
using System;

namespace ParallelMatrixMultuplication.Tests
{
    public class ParallelMatrixMultiplicationTests
    {
        [Test]
        public void TestNullMatrixIsPassed()
        {
            var leftMatrix = new int[,]
            {
                { 1 }
            };
            int[,] result;
            Assert.Throws<ArgumentNullException>(() 
                => result = new ParallelMultiplicationStrategy().Multiply(leftMatrix, null)); 
        }

        [Test]
        public void TestMultiplyMatrices()
        {
            var leftMatrix = new int[,] 
            { 
                { 1, 2, 3 }, 
                { 4, 5, 6 } 
            };
            var rightMatrix = new int[,]
            {
                {1 },
                {2 },
                {3 }
            };
            var result = new ParallelMultiplicationStrategy().Multiply(leftMatrix, rightMatrix);
            Assert.AreEqual(1, result.GetLength(1));
            Assert.AreEqual(2, result.GetLength(0));
            CollectionAssert.AreEqual(new int[,]
            { 
                { 14}, 
                { 32} 
            }, result);
        }

        [Test]
        public void TestTryMultiplyIncompatibleMatrices()
        {
            var leftMatrix = new int[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };
            var rightMatrix = new int[,]
            {
                {1 },
                {2 }
            };
            var strategy = new ParallelMultiplicationStrategy();
            Assert.Throws<ArgumentException>(() => strategy.Multiply(leftMatrix, rightMatrix));
        }
    }

    public class SequentialMatrixMultiplicationTests
    {       
        [Test]
        public void TestMultiplyMatrices()
        {
            var left = new int[,]
            {
                { 9, 13},
                { 7, 5 }
            };
            var right = new int[,]
            {
                {1, 0 },
                {0, 1 }
            };
            var result = new SequentialMultiplicationStrategy().Multiply(left, right);
            Assert.AreEqual(2, result.GetLength(1));
            Assert.AreEqual(2, result.GetLength(0));
            CollectionAssert.AreEqual(new int[,]
            {
                { 9, 13},
                { 7, 5 }
            }, result);
        }
    }

    public class FileMatrixMultiplicatorTests
    {
        [Test]
        public void TestPrintMatrixToFileAndThenConvertBack()
        {
            var matrix = MatrixGenerator.GenerateMatrix(12, 3);
            var path = "..\\..\\..\\example.txt";
            var multiplicator = new FileMatrixMultiplicator(new SequentialMultiplicationStrategy());
            multiplicator.PrintMatrixToFile(matrix, path);
            int[,] converted;
            multiplicator.TryConvertToMatrix(path, out converted);
            CollectionAssert.AreEqual(matrix, converted);
        }

        [Test]
        public void TestTryConvertFileOfInvalidFormat()
        {
            int[,] matrix;
            var path = "..\\..\\..\\wrongFormat.txt";
            var multiplicator = new FileMatrixMultiplicator(new SequentialMultiplicationStrategy());
            Assert.IsFalse(multiplicator.TryConvertToMatrix(path, out matrix));
            Assert.IsNull(matrix);
        }

        [Test]
        public void TestNullMatrixIsPassed()
        {
            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy());
            Assert.Throws<ArgumentNullException>(() 
                => multiplicator.PrintMatrixToFile(null, ""));
        }

        [Test]
        public void TestNullFilePathIsPassed()
        {
            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy());
            Assert.Throws<ArgumentException>(()
                => multiplicator.Multiply("", null, ""));
        }

        [Test]
        public void TestCompareProductsUsingDifferentStrategies()
        {
            var left = MatrixGenerator.GenerateMatrix(15, 7);
            var right = MatrixGenerator.GenerateMatrix(7, 13);
            
            var leftPath = "..\\..\\..\\left.txt";
            var rightPath = "..\\..\\..\\right.txt";
            var result = "..\\..\\..\\result.txt";

            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy());
            multiplicator.PrintMatrixToFile(left, leftPath);
            multiplicator.PrintMatrixToFile(right, rightPath);

            multiplicator.Multiply(leftPath, rightPath, result);
            int[,] resultP;
            multiplicator.TryConvertToMatrix(result, out resultP);

            multiplicator.Strategy = new SequentialMultiplicationStrategy();
            multiplicator.Multiply(leftPath, rightPath, result);
            int[,] resultS;
            multiplicator.TryConvertToMatrix(result, out resultS);

            CollectionAssert.AreEqual(resultP, resultS);
        }
    }    
}