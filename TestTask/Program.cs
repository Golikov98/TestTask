using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestTask
{
    public class Program
    {

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
            //Раскомментировать для ручного тестирования
            //var argsList = new List<string>();
            //Console.WriteLine("Введите путь для первого файла:");
            //var firstPath = Console.ReadLine();
            //argsList.Add(WriteErrorMessage(firstPath));
            //Console.WriteLine("Введите путь для второго файла:");
            //var secondPath = Console.ReadLine();
            //argsList.Add(WriteErrorMessage(secondPath));
            //args = argsList.ToArray();

            if (args.Length < 2) 
            {
                Console.WriteLine("Произошла ошибка!" + '\n' + "Необходимо указать два пути к файлам");
                return;
            }

            using (var inputStream1 = GetInputStream(args[0]))
            using (var inputStream2 = GetInputStream(args[1]))
            {
                IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                PrintStatistic(singleLetterStats);
                PrintStatistic(doubleLetterStats);
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }


        /// <summary>
        /// Ф-ция проверяет указанный путь к файлу на пустоту, а файл существование
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string WriteErrorMessage(string filePath)
        {
            filePath = filePath.Replace("\"", "");

            while (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Значение не должно быть пустым, введите действительный путь к файлу");
                filePath = Console.ReadLine();
            }

            while (!File.Exists(filePath))
            {
                Console.WriteLine("Файл по указанному пути не найден \n" + "Введите действительный путь к файлу:");
                filePath = Console.ReadLine();
            }

            return filePath;
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
            var stats = new Dictionary<char, LetterStats>();

            stream.ResetPositionToStart();

            //Раскомментировать для ручного тестирования
            //try
            //{
                while (!stream.IsEof)
                {
                    char c = stream.ReadNextChar();

                    if (!char.IsLetter(c))
                    {
                        continue;
                    }

                    if (!stats.ContainsKey(c))
                    {
                        stats[c] = new LetterStats { Letter = c.ToString(), Count = 0 };
                    }

                    IncStatistic(stats[c]);
                }
            //}
            //catch (EndOfStreamException) 
            //{
            //    return new List<LetterStats>(stats.Values);
            //}

            return new List<LetterStats>(stats.Values);
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
            var stats = new Dictionary<string, LetterStats>(StringComparer.OrdinalIgnoreCase);
            char? p = null;

            stream.ResetPositionToStart();

            //Раскомментировать для ручного тестирования
            //try
            //{
                while (!stream.IsEof)
                {
                    char c = stream.ReadNextChar();

                    if (!char.IsLetter(c))
                    {
                        p = null;
                        continue;
                    }

                    char lowerCurrent = char.ToLowerInvariant(c);

                    if (p.HasValue && char.ToLowerInvariant(p.Value) == lowerCurrent)
                    {
                        string pair = $"{p}{c}".ToLowerInvariant();

                        if (!stats.ContainsKey(pair))
                        {
                            stats[pair] = new LetterStats { Letter = pair, Count = 0 };
                        }

                        IncStatistic(stats[pair]);
                        p = null;
                    }
                    else
                    {
                        p = c;
                    }
                }
            //}
            //catch (EndOfStreamException)
            //{
            //    return new List<LetterStats>(stats.Values);
            //}

            return new List<LetterStats>(stats.Values);
        }

        /// <summary>
        /// Массив HashSet гласных букв (RU, EN)
        /// </summary>
        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'А','Е','Ё','И','О','У','Ы','Э','Ю','Я',
            'а','е','ё','и','о','у','ы','э','ю','я',
            'A','E','I','O','U','Y',
            'a','e','i','o','u','y'
        };

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.
            for (int i = letters.Count - 1; i >= 0; i--)
            {
                string s = letters[i].Letter;

                switch (charType)
                {
                    case CharType.Consonants:
                        bool allConsonants = true;

                        foreach (char c in s)
                        {
                            if (!char.IsLetter(c) || Vowels.Contains(c))
                            {
                                allConsonants = false; 
                                break;
                            }
                        }

                        if (allConsonants)
                        {
                            letters.RemoveAt(i);
                        }

                        break;
                    case CharType.Vowel:
                        bool allVowels = true;

                        foreach (char c in s)
                        {
                            if (!Vowels.Contains(c))
                            {
                                allVowels = false;
                                break;
                            }
                        }

                        if (!allVowels)
                        {
                            letters.RemoveAt(i);
                        }
                        break;
                }
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
            var sorted = letters.OrderBy(l  => l.Letter, StringComparer.OrdinalIgnoreCase).ToList();
            int total = 0;

            foreach (var stat in sorted) 
            { 
                Console.WriteLine($"{stat.Letter} : {stat.Count}");
                total += stat.Count;
            }

            Console.WriteLine($"ИТОГО: {total}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
