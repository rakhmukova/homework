using System;
using System.Diagnostics;
using System.IO;

namespace ParallelMatrixMultiplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var sizes = 11;
            var numOfCases = 10;
            var threadsNum = 8;
            var maxValue = 100;
            var sums = new long[sizes, 2];
            var squareSums = new long[sizes, 2];
            var strategies = new MultiplicationStrategy[]
            {
                new ParallelMultiplicationStrategy(threadsNum),
                new SequentialMultiplicationStrategy()
            };
            var stopwatch = new Stopwatch();
            MultiplicationStrategy strategy;
            var degTwo = 1;
            
            for (var i = 0; i < sizes; ++i)
            {
                Console.WriteLine($"Size: {degTwo}\n");
                for (var j = 0; j < numOfCases; ++j)
                {
                    var first = MatrixGenerator.GenerateMatrix(degTwo, degTwo, maxValue);
                    var second = MatrixGenerator.GenerateMatrix(degTwo, degTwo, maxValue);

                    for (var k = 0; k < 2; ++k)
                    {
                        strategy = strategies[k];
                        stopwatch.Restart();
                        strategy.MultiplyIfPossible(first, second);
                        stopwatch.Stop();
                        var time = stopwatch.ElapsedMilliseconds;
                        Console.Write($"{(k == 0 ? "P" : "S") }: {time}\t");
                        sums[i, k] += time;
                        squareSums[i, k] += time * time;
                    }
                    Console.WriteLine("\n");
                }
                degTwo *= 2;
                Console.WriteLine("\n-------------------\n");
            }

            var result = "..\\..\\..\\statistics.txt";
            using var streamWriter = File.CreateText(result);

            streamWriter.WriteLine($"Max value of an element: {maxValue}\n");
            streamWriter.WriteLine($"Threads: {threadsNum}\n");

            degTwo = 1;
            for (var i = 0; i < sizes; ++i)
            {
                streamWriter.WriteLine($"Size: {degTwo}\n");
                for (var j = 0; j < 2; ++j)
                {
                    sums[i, j] /= numOfCases;
                    squareSums[i, j] /= numOfCases;
                    streamWriter.WriteLine($"{(j == 0 ? "P" : "S") }: " +
                        $"Math expectation: {sums[i, j]} ms, " +
                        $"Standard deviation: {(int)Math.Sqrt(squareSums[i, j] - sums[i, j] * sums[i, j])} ms\n");
                }
                degTwo *= 2;
            }
        }       
    }
}
