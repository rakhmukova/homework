using NUnit.Framework;
using ParallelMatrixMultiplication;
using System;

namespace ParallelMatrixMultuplication.Tests
{
    public class FileMatrixMultiplicatorTests
    {
        static private string directory = "..\\..\\..\\files";

        [Test]
        public void TestPrintMatrixToFileAndThenConvertBack()
        {
            var matrix = MatrixGenerator.GenerateMatrix(12, 3);
            var path = directory + "\\example.txt"; 
            var multiplicator = new FileMatrixMultiplicator(new SequentialMultiplicationStrategy());
            multiplicator.PrintMatrixToFile(matrix, path);
            multiplicator.TryConvertToMatrix(path, out int[,] converted);
            CollectionAssert.AreEqual(matrix, converted);
        }

        [Test]
        public void TestTryConvertFileOfInvalidFormat()
        {
            var path = directory +"\\wrongFormat.txt";
            var multiplicator = new FileMatrixMultiplicator(new SequentialMultiplicationStrategy());
            Assert.IsFalse(multiplicator.TryConvertToMatrix(path, out int[,] matrix));
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
            
            var leftPath = directory + "\\left.txt";
            var rightPath = directory + "\\right.txt";
            var result = directory + "\\result.txt";

            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy());
            multiplicator.PrintMatrixToFile(left, leftPath);
            multiplicator.PrintMatrixToFile(right, rightPath);

            multiplicator.Multiply(leftPath, rightPath, result);
            multiplicator.TryConvertToMatrix(result, out int[,] resultP);

            multiplicator.Strategy = new SequentialMultiplicationStrategy();
            multiplicator.Multiply(leftPath, rightPath, result);
            multiplicator.TryConvertToMatrix(result, out int[,] resultS);

            CollectionAssert.AreEqual(resultP, resultS);
        }
    }    
}