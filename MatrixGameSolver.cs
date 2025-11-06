using System.Threading;

namespace lab1
{
    // клас інкапсулює всю логіку для пошуку максиміну.
    public class MatrixGameSolver
    {
        // внутрішній стан класу
        private readonly int[,] _matrix;
        private readonly int _rows;
        private readonly int _cols;

        // Масив для зберігання проміжних результатів - мінімумів кожного рядка.
        // кожен потік пише у свій унікальний індекс, тому блокування (locks) не потрібні.
        private readonly int[] _rowMinimums;

        public MatrixGameSolver(int[,] matrix)
        {
            _matrix = matrix;
            _rows = matrix.GetLength(0);
            _cols = matrix.GetLength(1);
            _rowMinimums = new int[_rows];
        }

        // 1. Реалізація однопотокового застосунку.
        public int FindMaximinSingleThread()
        {
            // 1. Знаходимо мінімуми для кожного рядка
            for (int i = 0; i < _rows; i++)
            {
                int minInRow = int.MaxValue;
                for (int j = 0; j < _cols; j++)
                {
                    if (_matrix[i, j] < minInRow)
                    {
                        minInRow = _matrix[i, j];
                    }
                }
                _rowMinimums[i] = minInRow;
            }

            // 2. Знаходимо максимум серед знайдених мінімумів
            return FindMaxOfMinimums();
        }

        // 2. Реалізація багатопотокового застосунку.
        public int FindMaximinMultiThread(int numThreads)
        {
            // обмежуємо кількість потоків, якщо рядків менше, ніж потоків.
            if (numThreads > _rows)
            {
                numThreads = _rows;
            }

            // масив для зберігання посилань на потоки
            Thread[] threads = new Thread[numThreads];

            // розрахунок, скільки рядків оброблятиме кожен потік
            int rowsPerThread = _rows / numThreads;

            // ділимо рядки матриці на 'numThreads' частин.
            for (int t = 0; t < numThreads; t++)
            {
                // визначаємо діапазон рядків для поточного потоку
                int startRow = t * rowsPerThread;
                int endRow = (t == numThreads - 1)
                    ? _rows // Останній потік забирає всі рядки, що залишилися
                    : startRow + rowsPerThread;

                // використовуємо 'new Thread' для явного створення потоків
                threads[t] = new Thread(() => ProcessRows(startRow, endRow));
                threads[t].Start(); // Запускаємо потік
            }

            // чекаємо, доки всі потоки завершать свою роботу
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            // знаходимо максимум серед проміжних результатів (мінімумів)
            return FindMaxOfMinimums();
        }

        // Допоміжний метод, який виконується в кожному потоці.
        // Обробляє лише свій діапазон рядків: [startRow, endRow).
        private void ProcessRows(int startRow, int endRow)
        {
            for (int i = startRow; i < endRow; i++)
            {
                int minInRow = int.MaxValue;
                for (int j = 0; j < _cols; j++)
                {
                    if (_matrix[i, j] < minInRow)
                    {
                        minInRow = _matrix[i, j];
                    }
                }
                // кожен потік пише у свій унікальний елемент _rowMinimums[i]
                _rowMinimums[i] = minInRow;
            }
        }

        // Допоміжний метод для пошуку максимуму серед мінімумів.
        // Використовується обома версіями (одно- та багатопотоковою).
        private int FindMaxOfMinimums()
        {
            int maximin = int.MinValue;
            for (int i = 0; i < _rows; i++)
            {
                if (_rowMinimums[i] > maximin)
                {
                    maximin = _rowMinimums[i];
                }
            }
            return maximin;
        }
    }
}