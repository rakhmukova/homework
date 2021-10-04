using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ParallelMatrixMultiplication
{
    class Program
    {    
        static private readonly int numOfCases = 10;
        static private readonly int maxValue = 100;
        static private readonly int threadsNum = Environment.ProcessorCount;

        /// <summary>
        /// measures times spent on matrices multiplication
        /// </summary>
        /// <param name="size">a size of matrices being multiplied</param>
        static private (IEnumerable<long> parallel, IEnumerable<long> sequential) 
            MeasureTime(int size)
        {
            var parTime = new List<long>();
            var seqTime = new List<long>();
            var parStrategy = new ParallelMultiplicationStrategy(threadsNum);
            var seqStrategy = new SequentialMultiplicationStrategy();
            var stopwatch = new Stopwatch();

            for (var i = 0; i < numOfCases; ++i)
            {
                var first = MatrixGenerator.GenerateMatrix(size, size, maxValue);
                var second = MatrixGenerator.GenerateMatrix(size, size, maxValue);

                MultiplicationStrategy strategy = parStrategy;
                stopwatch.Restart();
                strategy.MultiplyIfPossible(first, second);
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                parTime.Add(time);

                strategy = seqStrategy;
                stopwatch.Restart();
                strategy.MultiplyIfPossible(first, second);
                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                seqTime.Add(time);
            }
            return (parTime, seqTime);
        }

        /// <summary>
        /// calculates math expectation and deviation of the passed data 
        /// </summary>
        static private (double expectation, double deviation) 
            CalculateExpectationAndDeviation(IEnumerable<long> data)
        {
            var expectation = data.Sum() / numOfCases;
            var deviation = Math.Sqrt(data.Select(x => Math.Pow(x - expectation, 2)).Sum() / numOfCases);           
            return (expectation, deviation);
        }

        static void Main()
        {            
            var maxDegree = 11;
            var degrees = Enumerable.Range(0, maxDegree).Select(x => (int)Math.Pow(2, x)).ToList();
            var parStatistics = new List<(double expectation, double deviation)>();
            var seqStatistics = new List<(double expectation, double deviation)>();
            
            foreach (int degTwo in degrees)
            {
                var (parallel, sequential) = MeasureTime(degTwo);
                parStatistics.Add(CalculateExpectationAndDeviation(parallel));
                seqStatistics.Add(CalculateExpectationAndDeviation(sequential));
                Console.WriteLine("\n-------------------\n");
            }

            var path = "..\\..\\..\\statistics.txt";
            using var streamWriter = File.CreateText(path);   
            
            streamWriter.WriteLine($"Max value of an element: {maxValue}\n");
            streamWriter.WriteLine($"Threads: {threadsNum}\n");

            for (var i = 0; i < maxDegree; ++i)
            {
                streamWriter.WriteLine($"Size: {degrees[i]}\n");
                var (parExpectation, parDeviation) = parStatistics[i]; 
                var (seqExpectation, seqDeviation) = seqStatistics[i];
                streamWriter.WriteLine($"Math expectation:\n"
                    + $"parallel: {parExpectation: 0.##} ms\n"
                    + $"sequential: {seqExpectation: 0.##} ms\n"
                    + $"\nStandard deviation:\n"
                    + $"parallel: {parDeviation: 0.##} ms\n"
                    + $"sequential: {seqDeviation: 0.##} ms\n");                                             
            }            
        }       
    }
}
