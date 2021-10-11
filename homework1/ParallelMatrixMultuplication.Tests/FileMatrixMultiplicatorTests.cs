using NUnit.Framework;
using ParallelMatrixMultiplication;
using System;
using System.IO;

namespace ParallelMatrixMultuplication.Tests
{
    public class FileMatrixMultiplicatorTests
    {
        static private readonly int threadsNum = Environment.ProcessorCount;
        private const string directory = "..\\..\\..\\files";

        [Test]
        public void TestPrintMatrixToFileAndThenConvertBack()
        {
            var matrix = MatrixGenerator.GenerateMatrix(12, 3);            
            var path = directory + "\\example.txt";
            FileMatrixMultiplicator.PrintMatrixToFile(matrix, path);
            var converted = FileMatrixMultiplicator.ConvertToMatrix(path);
            CollectionAssert.AreEqual(matrix, converted);
        }

        [Test]
        public void TestTryConvertFileOfInvalidFormat()
        {
            var path = directory + "\\wrongFormat.txt";
            Assert.Throws<FormatException>(() 
                => FileMatrixMultiplicator.ConvertToMatrix(path));
        }

        [Test]
        public void TestNullMatrixIsPassed()
        {
            Assert.Throws<ArgumentNullException>(() 
                => FileMatrixMultiplicator.PrintMatrixToFile(null, ""));
        }

        [Test]
        public void TestNullFilePathIsPassed()
        {
            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy(threadsNum));
            Assert.Throws<FileNotFoundException>(()
                => multiplicator.Multiply("", null, ""));
        }

        [Test]
        public void TestCompareProductsUsingDifferentStrategies()
        {
            var left = MatrixGenerator.GenerateMatrix(15, 7);
            var right = MatrixGenerator.GenerateMatrix(7, 13);
            
            var leftPath = directory + "\\left.txt";
            var rightPath = directory + "\\right.txt";
            var resultPath = directory + "\\result.txt";
                        
            FileMatrixMultiplicator.PrintMatrixToFile(left, leftPath);
            FileMatrixMultiplicator.PrintMatrixToFile(right, rightPath);

            var multiplicator = new FileMatrixMultiplicator(new ParallelMultiplicationStrategy(threadsNum));
            multiplicator.Multiply(leftPath, rightPath, resultPath);
            var resultP = FileMatrixMultiplicator.ConvertToMatrix(resultPath);

            multiplicator.Strategy = new SequentialMultiplicationStrategy();
            multiplicator.Multiply(leftPath, rightPath, resultPath);
            var resultS = FileMatrixMultiplicator.ConvertToMatrix(resultPath);

            CollectionAssert.AreEqual(resultP, resultS);
        }
    }    
}