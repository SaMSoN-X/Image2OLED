using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageToOLED
{
    class Program
    {
        /// <summary>
        /// Возвращает массив строк, представляющий собой текстовое представление 
        /// (по сути ASCI-арт) изображения bitmap.
        /// </summary>
        /// <param name="bitmap">Изображение размером 64x32 (TODO) и глубиной цвета 1 бит (монохромное, чёрно-белое).</param>
        /// <returns></returns>
        static List<string> GetTextViewFromImage(Bitmap bitmap)
        {
            Color pixelColor;

            // Сюда будет формироваться текстовое представление картинки.
            List<string> stringList = new List<string>();
            string currentString;

            /* Обходим картинку, опрашиваем пиксели 
            и формируем её текстовое представление. */
            for (int y = 0; y < bitmap.Height; y++)
            {
                /* 4 пробела для удобства - это отступ в 
                коде библиотеки дисплея моём случае. */
                currentString = "    ";

                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Получаем значение пикселя.
                    pixelColor = bitmap.GetPixel(x, y);

                    /* Картинки должны быть подготовлены с глубиной 
                    цвета в 1 бит (т.е. или белый, или чёрный).
                    (Возможность сохранения картинок с такими настройками  
                    доступны, к примеру, в программе Paint .NET.) */

                    /* Формируем строку в формате, в котором мы подставим её 
                    в нашу библиотеку для взаимодействия с дисплеем. */
                    currentString += (pixelColor.ToArgb() == Color.White.ToArgb()) ? "_" : "X";

                    /* Каждый байт записывается отдельно. 
                    После каждого байта добавляем запятую и пробел. */
                    if ((x + 1) % 8 == 0)
                        currentString += ", ";
                }

                // Добавляем сформированную строку в результирующий список.
                stringList.Add(currentString);
            }
            return stringList;
        }

        /// <summary>
        /// Записывает список строк listString в текстовый файл textFilePath.
        /// </summary>
        /// <param name="listString">Список строк.</param>
        /// <param name="textFilePath">Текстовый файл.</param>
        static void WriteToFile(List<string> listString, string textFilePath)
        {
            string dir = Path.GetDirectoryName(textFilePath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (StreamWriter textFile = new StreamWriter(textFilePath))
                foreach (string str in listString)
                    textFile.WriteLine(str);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("            Инструмент конвертирует картинку в код для библиотеки");
            Console.WriteLine("                     OLED-дисплея QG-5664GSWHG14.");
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("Для добавления символа в библиотеку шрифта 64x32 необходимо:");
            Console.WriteLine("    1. отрисовать на полотне размером 64x32");
            Console.WriteLine("       картинку с требуемым изображением (например, букву или цифру);");
            Console.WriteLine("    2. сохранить изображение в одном из следующих");
            Console.WriteLine("       форматов: png, jpg или bmp.");
            Console.WriteLine("    3. при сохранении указать следующие настройки:");
            Console.WriteLine("       глубина цвета - 1 бит (т.е в изображении");
            Console.WriteLine("       должны быть использованы лишь 2 цвета -");
            Console.WriteLine("       чёрный или белый).");
            Console.WriteLine("Все перечисленные операции крайне просто сделать в программе Paint .NET.");
            Console.WriteLine("При желании можно использовать любой другой графический редактор.");
            Console.WriteLine("    4. запустить эту программу, передать ей путь к");
            Console.WriteLine("       картинке (или к папке с такими картинками -");
            Console.WriteLine("       в этом случае будут опознаны и преобразованы все");
            Console.WriteLine("       картинки, находящиеся в этой папке).");
            Console.WriteLine("    5. Вуаля! Программа создала поддиректорию code,");
            Console.WriteLine("       в которой лежат текстовые файлы с кодами, полученными");
            Console.WriteLine("       в результате преобразования переданной программе");
            Console.WriteLine("       картинки (или картинок - в случае с папкой).");
            Console.WriteLine("    6. Добавить код символа в текст программы -");
            Console.WriteLine("       а конкретно в файл реализации шрифта 64x32.");
            Console.WriteLine("(При необходимости код программы неприлично легко преобразовать под");
            Console.WriteLine("шрифт любого размера - а не конкретно под 64x32.)");
            Console.WriteLine("-------------------------------------------------------------------------");

            string targetPath = string.Empty;
            Console.Write("Путь к файлу или папке: ");
            targetPath = Console.ReadLine();
            Console.WriteLine();

            // Сюда будут записываться адреса к файлам.
            List<string> fileEntries = new List<string>();

            // Проверяем, папка это или файл.
            bool isDir = File.GetAttributes(@targetPath).HasFlag(FileAttributes.Directory);

            // Передана директория.
            if (isDir)
            {
                // Получаем все файлы в текущей директории.
                fileEntries = Directory.GetFiles(targetPath).ToList();
            }
            else // Передан файл.
            {
                fileEntries.Add(targetPath);
            }

            // Обходим все файлы.
            int count = 0;
            foreach (var file in fileEntries)
            {
                string ext = Path.GetExtension(file);

                if ((ext == ".png") || (ext == ".jpg") || (ext == ".bmp"))
                {
                    Console.Write("Файл: " + file);

                    // Сюда помещается картинка из указанного пути.
                    var bitmap = new Bitmap(file);

                    // Получаем текстовое представление картинки.
                    List<string> imageTextView = GetTextViewFromImage(bitmap);

                    // В целевой директории (где мы осуществляем работу) создадим папку code и положим туда коды, полученные из картинок.
                    string outputPath = (isDir ? targetPath : Path.GetDirectoryName(targetPath)) + @"\code\" + Path.GetFileNameWithoutExtension(file) + ".txt";

                    WriteToFile(imageTextView, outputPath);

                    count++;
                    Console.WriteLine(" - Успех!");
                }
            }
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("Всего файлов обработано: {0}", count);

            Console.ReadKey();
        }
    }
}
