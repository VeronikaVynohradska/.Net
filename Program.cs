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

            // шапка таблиці
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine(
                $"| {"Опис",-20} | {"Розмір (RxC)",-15} | {"Потоки",-6} | {"Час (Однопоток)",-17} | {"Час (Багатопоток)",-18} | {"Прискорення",-12} | {"Переможець",-10} |");
            Console.WriteLine(
                "|----------------------|-----------------|---------|-------------------|--------------------|--------------|------------|");

            // Тестовий набір 1: Маленька матриця
            // Очікування: Однопотоковий виграє через накладні витрати (overhead)
            RunBenchmarkSet("Маленька", 500, 500);

            // Тестові набори 2 та 3: Середня матриця
            // Очікування: Пошук оптимальної кількості потоків
            RunBenchmarkSet("Середня 1", 4000, 4000);
            RunBenchmarkSet("Середня 2", 7000, 7000);

            // Тестовий набір 4: Велика матриця
            // Очікування: Багатопотоковий режим показує максимальну ефективність
            RunBenchmarkSet("Велика", 10000, 10000);

            // Тестовий набір 5: "Довга" матриця (багато рядків)
            // Очікування: Дуже хороше прискорення, оскільки рядки - наша одиниця паралелізму
            RunBenchmarkSet("Довга", 50000, 100);

            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("\nТестування завершено.");
            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }

        // Виконує повний набір тестів (1 однопотоковий + N багатопотокових) для ОДНІЄЇ матриці вказаного розміру.
        private static void RunBenchmarkSet(string description, int rows, int cols)
        {
            Stopwatch stopwatch = new Stopwatch();

            int[,] matrix = GenerateMatrix(rows, cols); // генерація матриці

            // однопотоковий (базовий тест)
            MatrixGameSolver singleSolver = new MatrixGameSolver(matrix);
            stopwatch.Restart();
            int resSingle = singleSolver.FindMaximinSingleThread();
            stopwatch.Stop();
            long timeSingle = stopwatch.ElapsedMilliseconds;

            Console.WriteLine(
                $"| {description,-20} | {rows + "x" + cols,-15} | {"1 (База)",-6} | {timeSingle + " мс",-17} | {"-",-18} | {"1.00x",-12} | {"-",-10} |");

            // тестування різної кількості потоків для однієї й тої самої матриці
            foreach (int threads in ThreadCountsToTest)
            {
                // створення нового екземпляру, щоб скинути внутрішній стан (масив _rowMinimums)
                MatrixGameSolver multiSolver = new MatrixGameSolver(matrix);

                stopwatch.Restart();
                int resMulti = multiSolver.FindMaximinMultiThread(threads);
                stopwatch.Stop();
                long timeMulti = stopwatch.ElapsedMilliseconds;

                // аналіз
                double speedup = (timeSingle > 0 && timeMulti > 0) ? (double)timeSingle / timeMulti : 1.0;

                string winner;
                if (timeSingle <= 10 && timeMulti <= 10) // якщо час занадто малий
                {
                    winner = "N/A";
                }
                else
                {
                    winner = (timeMulti < timeSingle) ? "Багатопоток" : "Однопоток";
                }

                // перевірка коректності
                if (resSingle != resMulti)
                {
                    winner = "ПОМИЛКА!";
                }

                // виведення рядку таблиці
                Console.WriteLine(
                    $"| {description,-20} | {rows + "x" + cols,-15} | {threads,-6} | {timeSingle + " мс",-17} | {timeMulti + " мс",-18} | {speedup:F2}x {null,-7} | {winner,-10} |");
            }
        }

        // Допоміжний метод для створення великої матриці з випадковими числами.
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