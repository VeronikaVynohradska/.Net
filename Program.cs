using System;
using System.Diagnostics;       // Для Stopwatch
using System.Text;              // Для Encoding

namespace lab1
{
    class Program
    {
        // набори потоків, які тестуємо
        private static readonly int[] ThreadCountsToTest = { 2, 4, 8, 16 };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Запуск деталізованого тестування продуктивності...");
            Console.WriteLine($"Процесор має {Environment.ProcessorCount} логічних ядер.");
            Console.WriteLine(new string('-', 50));

            // Тестовий набір 1
            RunBenchmarkSet("Маленька", 500, 500);

            // Тестові набори 2 та 3
            RunBenchmarkSet("Середня 1", 4000, 4000);
            RunBenchmarkSet("Середня 2", 7000, 7000);

            // Тестовий набір 4
            RunBenchmarkSet("Велика", 10000, 10000);

            // Тестовий набір 5
            RunBenchmarkSet("Довга", 50000, 100);

            Console.WriteLine(new string('-', 50));
            Console.WriteLine("\nТестування завершено.");
            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }

        // Виконує повний набір тестів
        private static void RunBenchmarkSet(string description, int rows, int cols)
        {
            // Виводимо заголовок для цього набору тестів
            Console.WriteLine($"\n--- Тест: {description} ({rows}x{cols}) ---");

            Stopwatch stopwatch = new Stopwatch();
            int[,] matrix = GenerateMatrix(rows, cols); // генерація матриці

            // 1. Однопотоковий (базовий тест)
            MatrixGameSolver singleSolver = new MatrixGameSolver(matrix);
            stopwatch.Restart();
            int resSingle = singleSolver.FindMaximinSingleThread();
            stopwatch.Stop();
            long timeSingle = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Однопоток (1 База): {timeSingle} мс");

            // 2. Багатопотокові тести
            foreach (int threads in ThreadCountsToTest)
            {
                MatrixGameSolver multiSolver = new MatrixGameSolver(matrix);

                stopwatch.Restart();
                int resMulti = multiSolver.FindMaximinMultiThread(threads);
                stopwatch.Stop();
                long timeMulti = stopwatch.ElapsedMilliseconds;

                // аналіз
                double speedup = (timeSingle > 0 && timeMulti > 0) ? (double)timeSingle / timeMulti : 1.0;
                string speedupStr = $"{speedup:F2}x";

                string winner = "N/A";
                if (timeSingle > 10 || timeMulti > 10) // Порівнюємо, лише якщо час значущий
                {
                    winner = (timeMulti < timeSingle) ? "Багатопоток" : "Однопоток";
                }

                if (resSingle != resMulti) // перевірка коректності
                {
                    winner = "ПОМИЛКА!";
                }

                // Виводимо результат для поточної к-сті потоків
                Console.WriteLine($"Потоків: {threads,-2} | Час: {timeMulti,-4} мс | Прискорення: {speedupStr,-6} | Переможець: {winner}");
            }
        }

        // Допоміжний метод для створення матриці
        private static int[,] GenerateMatrix(int rows, int cols)
        {
            int[,] matrix = new int[rows, cols];
            Random rand = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rand.Next(-10000, 10000);
                }
            }
            return matrix;
        }
    }
}

