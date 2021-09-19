using System.Diagnostics;
using System.IO;

namespace ParallelMatrixMultiplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = "..\\..\\..\\statistics.txt";
            using (var sw = File.CreateText(result))
            {
                var sizes = 10;
                var numOfCases = 20;
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
                int[,] first, second;
                MultiplicationStrategy strategy;
                var degTwo = 1;
                long time;
                sw.WriteLine($"Max value of element: {maxValue}\n");
                sw.WriteLine($"Threads: {threadsNum}\n");
                for (var i = 0; i < sizes; ++i)
                {
                    sw.WriteLine($"Size: {degTwo}\n");                    
                    for (var j = 0; j < numOfCases; ++j)
                    {                       
                        first = MatrixGenerator.GenerateMatrix(degTwo, degTwo, maxValue);
                        second = MatrixGenerator.GenerateMatrix(degTwo, degTwo, maxValue);

                        for (var k = 0; k < 2; ++k)
                        {
                            strategy = strategies[k];
                            stopwatch.Restart();
                            strategy.Multiply(first, second);
                            stopwatch.Stop();
                            time = stopwatch.ElapsedMilliseconds;
                            sw.Write($"{(k == 0? "P" : "S") }: {time}\t");
                            sums[i, k] += time;
                            squareSums[i, k] += time * time;
                        }
                        sw.WriteLine("\n");
                    }
                    degTwo *= 2;
                    sw.WriteLine("\n-------------------\n");
                }

                degTwo = 1;
                for (var i = 0; i < sizes; ++i)
                {
                    sw.WriteLine($"Size: {degTwo}\n");
                    for (var j = 0; j < 2; ++j)
                    {
                        sums[i, j] /= numOfCases;
                        squareSums[i, j] /= numOfCases;
                        sw.WriteLine($"{(j == 0 ? "P" : "S") }: " +
                            $"Math expectation: {sums[i, j]}, " +
                            $"Dispersion: {squareSums[i, j] - sums[i, j] * sums[i, j]}\n");
                    }                                     
                    degTwo *= 2;
                }
            }            
        }       
    }
}
