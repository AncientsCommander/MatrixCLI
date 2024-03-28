using System.Globalization;
using System.Text;
using MatrixCLI.Enums;

namespace MatrixCLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            int? rows = null;
            int? columns;
            var isRowsSet = false;

            while (true)
            {
                if (!isRowsSet)
                {
                    rows = GetMatrixDimension(MatrixDimensionEnum.Row);
                    if (rows is null) continue;

                    isRowsSet = true;
                }

                columns = GetMatrixDimension(MatrixDimensionEnum.Column);
                if (columns is null) continue;

                break;
            }

            if (rows is null) return;

            var matrixList = GetMatrixList(rows, columns);
            var matrix = CreateMatrix(matrixList);

            var isRecreateRequested = false;
            while (true)
            {
                var choose = ShowMainMenu(matrix);

                switch (choose)
                {
                    case 1:
                        ShowLargestOrSmallestNumbers(matrix, NumberTypeEnum.Positive);
                        break;
                    case 2:
                        ShowLargestOrSmallestNumbers(matrix, NumberTypeEnum.Negative);
                        break;
                    case 3:
                        SortRow(SortType.Ascending, matrix);
                        break;
                    case 4:
                        SortRow(SortType.Descending, matrix);
                        break;
                    case 5:
                        InvertRow(matrix);
                        break;
                    case 9:
                        isRecreateRequested = true;
                        break;
                    case 0:
                        Console.Clear();
                        Console.WriteLine("Программа завершена");
                        Environment.Exit(0);
                        break;
                    default:
                        continue;
                }

                if (isRecreateRequested) break;
            }
        }
    }

    private static int? GetMatrixDimension(MatrixDimensionEnum dimensionEnum)
    {
        var message = dimensionEnum switch
        {
            MatrixDimensionEnum.Row => "Укажите количество строк: ",
            MatrixDimensionEnum.Column => "Укажите количество столбцов: ",
            _ => ""
        };

        Console.Clear();
        Console.WriteLine("Выберите размер матрицы.\n");
        Console.Write(message);

        var input = Console.ReadLine();
        return int.TryParse(input, out var rows) ? rows : null;
    }

    private static List<List<double>> GetMatrixList(int? rows, int? columns)
    {
        var matrixList = new List<List<double>>();

        var count = 0;
        for (var i = 0; i < rows; i++)
        {
            var row = new List<double>();
            matrixList.Add(row);

            for (var j = 0; j < columns; j++)
            {
                var isSuccess = false;

                while (!isSuccess)
                {
                    var value = GetMatrixValue(matrixList, rows, columns, count);

                    if (value is null) continue;

                    matrixList[i].Add((double)value);

                    isSuccess = true;
                    count++;
                }
            }
        }

        return matrixList;
    }

    private static double? GetMatrixValue(List<List<double>> matrixList, int? rows, int? colums,
        int? current = null)
    {
        var builder = MatrixViewBuilder(matrixList, rows, colums, current);


        Console.Clear();
        Console.WriteLine(builder.ToString());
        Console.Write("Введите значение матрицы на месте 'x': ");

        var input = Console.ReadLine();
        return double.TryParse(input, out var value) ? value : null;
    }

    private static double[,] CreateMatrix(IReadOnlyList<List<double>> matrixList)
    {
        var array = new double[matrixList.Count, matrixList[0].Count];

        for (var i = 0; i < matrixList.Count; i++)
        for (var j = 0; j < matrixList[i].Count; j++)
            array[i, j] = matrixList[i][j];

        return array;
    }

    private static int ShowMainMenu(double[,] matrix)
    {
        while (true)
        {
            var builder = MatrixViewBuilder(matrix);

            builder.Append("""

                           1. Показать количество положительных чисел в матрице
                           2. Показать количество отрицательных чисел в матрице
                           3. Отсортировать числа в строке по возрастанию
                           4. Отсортировать числа в строке по убыванию
                           5. Инвертировать числа в строке

                           9. Пересоздать матрицу
                           0. Завершить программу

                           """);

            Console.Clear();
            Console.WriteLine(builder.ToString());
            Console.Write("Выберите необходимое действие: ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out var choose)) continue;

            return choose;
        }
    }

    private static StringBuilder MatrixViewBuilder(List<List<double>> matrixList, int? rows, int? colums, int? current)
    {
        var builder = new StringBuilder();

        builder.Append("Текущий вид матрицы:\n\n");

        var charFormat = matrixList?.Max(subList =>
            subList.DefaultIfEmpty().Max(number => number.ToString(CultureInfo.InvariantCulture).Length)) ?? 0;

        var count = 0;
        for (var i = 0; i < rows; i++)
        {
            builder.Append('|');

            for (var j = 0; j < colums; j++)
            {
                var charToAdd = matrixList is not null && matrixList.Count > i && matrixList[i].Count > j
                    ? matrixList![i][j].ToString(CultureInfo.InvariantCulture)
                    : current == count
                        ? "x"
                        : "*";

                builder.Append(string.Format($" {{0, {charFormat}}} ", charToAdd));

                count++;
            }

            builder.Append("|\n");
        }

        return builder;
    }

    private static StringBuilder MatrixViewBuilder(double[,] matrix)
    {
        var builder = new StringBuilder();

        builder.Append("Текущий вид матрицы:\n\n");

        var charFormat = matrix?.Cast<double>()
            .Max(number => number.ToString(CultureInfo.InvariantCulture).Length) ?? 0;

        for (var i = 0; i < matrix!.GetLength(0); i++)
        {
            builder.Append('|');

            for (var j = 0; j < matrix.GetLength(1); j++)
                builder.Append(string.Format($" {{0, {charFormat}}} ", matrix![i, j]));

            builder.Append("|\n");
        }

        return builder;
    }

    private static void ShowLargestOrSmallestNumbers(double[,] matrix, NumberTypeEnum numberType)
    {
        while (true)
        {
            var count = 0;
            var message = "";
            switch (numberType)
            {
                default:
                case NumberTypeEnum.Positive:
                    count = matrix.Cast<double>().Where(s => s >= 0).ToList().Count;
                    message = $"Количество положительных чисел: {count}";
                    break;
                case NumberTypeEnum.Negative:
                    count = matrix.Cast<double>().Where(s => s < 0).ToList().Count;
                    message = $"Количество отрицательных чисел: {count}";
                    break;
            }

            var builder = MatrixViewBuilder(matrix);

            builder.Append($"""

                            {message}

                            0. Вернуться в предыдущее меню
                            """);

            Console.Clear();
            Console.WriteLine(builder.ToString());
            Console.Write("Выберите необходимое действие: ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out var choose) || choose != 0) continue;

            break;
        }
    }

    private static void SortRow(SortType sortType, double[,] matrix)
    {
        var choosedRow = 0;

        while (true)
        {
            var builder = MatrixViewBuilder(matrix);

            Console.Clear();
            Console.WriteLine(builder.ToString());

            Console.Write($"Выберите строку для сортировки по {sortType switch
            {
                SortType.Ascending => "возрастанию",
                SortType.Descending => "убыванию",
                _ => ""
            }}: ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out choosedRow) || choosedRow > matrix.GetLength(0) || choosedRow <= 0) continue;
            choosedRow--;
            break;
        }

        var row = new double[matrix.GetLength(1)];

        for (var k = 0; k < matrix.GetLength(1); k++) row[k] = matrix[choosedRow, k];

        QuickSort(sortType, row, 0, row.Length - 1);

        for (var i = 0; i < matrix.GetLength(1); i++) matrix[choosedRow, i] = row[i];
    }

    private static void QuickSort(SortType sortType, double[] row, int left, int right)
    {
        int i = left, j = right;
        var pivot = row[(left + right) / 2];

        while (i <= j)
        {
            switch (sortType)
            {
                default:
                case SortType.Ascending:
                    while (row[i] < pivot)
                        i++;

                    while (row[j] > pivot)
                        j--;
                    break;
                case SortType.Descending:
                    while (row[i] > pivot)
                        i++;

                    while (row[j] < pivot)
                        j--;
                    break;
            }

            if (i > j) continue;

            (row[i], row[j]) = (row[j], row[i]);

            i++;
            j--;
        }

        if (left < j)
            QuickSort(sortType, row, left, j);

        if (i < right)
            QuickSort(sortType, row, i, right);
    }

    private static void InvertRow(double[,] matrix)
    {
        var choosedRow = 0;

        while (true)
        {
            var builder = MatrixViewBuilder(matrix);

            Console.Clear();
            Console.WriteLine(builder.ToString());

            Console.Write("Выберите строку для инвертирования: ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out choosedRow) || choosedRow > matrix.GetLength(0) || choosedRow <= 0) continue;
            choosedRow--;
            break;
        }

        var rowLength = matrix.GetLength(1);
        for (var j = 0; j < rowLength / 2; j++)
            (matrix[choosedRow, j], matrix[choosedRow, rowLength - j - 1]) =
                (matrix[choosedRow, rowLength - j - 1], matrix[choosedRow, j]);
    }
}