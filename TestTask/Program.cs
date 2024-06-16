using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestTask
{
    public class Program
    {
        readonly private static List<char> otherChars = new List<char> { 'ь', 'ъ', 'Ь', 'Ъ' };
        readonly private static List<char> vowels = new List<char> { 'а', 'о', 'и', 'е', 'ё', 'э', 'ы', 'у', 'ю', 'я', 'А', 'О', 'И', 'Е', 'Ё', 'Э', 'Ы', 'У', 'Ю', 'Я' };
        readonly private static List<char> consonants = new List<char> { 'б', 'в', 'г', 'д', 'ж', 'з', 'й', 'к', 'л', 'м', 'н', 'п', 'р', 'с', 'т', 'ф', 'х', 'ц', 'ч', 'ш', 'щ',
                                                                 'Б', 'В', 'Г', 'Д', 'Ж', 'З', 'Й', 'К', 'Л', 'М', 'Н', 'П', 'Р', 'С', 'Т', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ' };

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Отсутствуют пути к файлам для чтения!");
            }
            else
            {
                Console.WriteLine("Первый файл:");

                if (args[0] is null)
                {
                    Console.WriteLine("Отсутствует путь к первому файлу!");
                }
                else if (!File.Exists(args[0]))
                {
                    PrintMissingFileInfo(args[0]);
                }
                else if (Path.GetExtension(args[0]) != ".txt")
                {
                    PrintInvalidFileExtensionInfo();
                }
                else
                {
                    IReadOnlyStream inputStream1 = GetInputStream(args[0]);
                    IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                    inputStream1.DisposeStream();

                    RemoveCharStatsByType(ref singleLetterStats, CharType.Vowels);

                    Console.WriteLine("Количество одиночных регистрозависимых букв:");
                    PrintStatistic(singleLetterStats);
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Второй файл:");

                if (args.Length < 2 || args[1] is null)
                {
                    Console.WriteLine("Отсутствует путь ко второму файлу!");
                }
                else if (!File.Exists(args[1]))
                {
                    PrintMissingFileInfo(args[1]);
                }
                else if (Path.GetExtension(args[1]) != ".txt")
                {
                    PrintInvalidFileExtensionInfo();
                }
                else
                {
                    IReadOnlyStream inputStream2 = GetInputStream(args[1]);
                    IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);
                    inputStream2.DisposeStream();

                    RemoveCharStatsByType(ref doubleLetterStats, CharType.Others);

                    Console.WriteLine("Количество парных не регистрозависимых букв:");
                    PrintStatistic(doubleLetterStats);
                }
            }

            // TODO : Необходимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
            Console.ReadKey();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();

            IList<LetterStats> letterStats = new List<LetterStats>();
            LetterStats ls = new LetterStats();

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
                if (char.IsLetter(c) && IsCyrillic(c))
                {
                    if (letterStats.Any(item => item.Letter == c.ToString()))
                    {
                        IncStatistic(letterStats, c.ToString());
                    }
                    else
                    {
                        ls.Letter = c.ToString();
                        ls.Count = 1;
                        letterStats.Add(ls);
                    }
                }
            }

            return letterStats;
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();

            IList<LetterStats> letterStats = new List<LetterStats>();
            LetterStats ls = new LetterStats();

            string prevString = null;

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                string curString = c.ToString();
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - НЕ регистрозависимый.
                if (char.IsLetter(c) && IsCyrillic(c))
                {
                    curString = curString.ToLower();
                    curString += curString;

                    if (curString == prevString)
                    {
                        if (letterStats.Any(item => item.Letter == curString))
                        {
                            IncStatistic(letterStats, curString);
                        }
                        else
                        {
                            ls.Letter = curString;
                            ls.Count = 1;
                            letterStats.Add(ls);
                        }
                    }
                }

                prevString = curString;
            }

            return letterStats;
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные, согласные или прочие(Ь, Ъ) буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(ref IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.
            switch (charType)
            {
                case CharType.Others:
                    letters = letters.Where(item => !otherChars.Contains(item.Letter[0])).ToList();
                    break;
                case CharType.Vowels:
                    letters = letters.Where(item => !vowels.Contains(item.Letter[0])).ToList();
                    break;
                default:
                    letters = letters.Where(item => !consonants.Contains(item.Letter[0])).ToList();
                    break;
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            letters = letters.OrderBy(item => item.Letter).ToList();

            int allCountedLetters = 0;

            foreach (var item in letters)
            {
                Console.WriteLine($"{item.Letter} : {item.Count}");
                allCountedLetters += item.Count;
            }

            Console.WriteLine(allCountedLetters == 0 ? $"Буквы отсутствуют!" : $"ИТОГО: {allCountedLetters}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats">Передаваемая коллекция</param>
        /// <param name="str">Строка для поиска элемента и увеличения счётчика его вхождения</param>
        private static void IncStatistic(IList<LetterStats> letterStats, string str)
        {
            LetterStats ls = letterStats.Single(item => item.Letter == str);

            int index = letterStats.IndexOf(ls);

            ls.Count++;

            letterStats[index] = ls;
        }

        /// <summary>
        /// Метод выводит в консоль информацию об отсутствии файла
        /// </summary>
        /// <param name="path"></param>
        private static void PrintMissingFileInfo(string path)
        {
            Console.WriteLine($"Файл '{Path.GetFileName(path)}' отсутствует по заданному пути!");
        }

        /// <summary>
        /// Метод выводит в консоль информацию о файле с несоответствующим расширением
        /// </summary>
        private static void PrintInvalidFileExtensionInfo()
        {
            Console.WriteLine("Программа работает только с текстовыми файлами, имеющими расширением \".txt\"!");
            Console.WriteLine("Был передан путь к файлу с другим расширением!");
        }

        /// <summary>
        /// Метод, проверяющий символ на принадлежность кириллице
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCyrillic(char c)
        {
            return Regex.IsMatch(c.ToString(), @"\p{IsCyrillic}");
        }
    }
}