using System.IO;
using System.Text;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private StreamReader _localStream;
        private string lsCurrentString;
        private int currentCharIndex;

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            IsEof = true;
            IsEoStr = true;

            // TODO : Заменить на создание реального стрима для чтения файла!
            _localStream = new StreamReader(fileFullPath);
        }

        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof
        {
            get; // TODO : Заполнять данный флаг при достижении конца файла/стрима при чтении
            private set;
        }

        /// <summary>
        /// Флаг окончания строки.
        /// </summary>
        public bool IsEoStr
        {
            get;
            private set;
        }

        public void DisposeStream()
        {
            _localStream.Dispose();
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// </summary>
        /// <returns>Считанный символ или, при достижении конца файла,- нулевой указатель (\0).</returns>
        public char ReadNextChar()
        {
            // TODO : Необходимо считать очередной символ из _localStream
            if (IsEoStr)
            {
                IsEof = false;
                IsEoStr = false;
                currentCharIndex = 0;

                lsCurrentString = _localStream.ReadLine();

                if (lsCurrentString == null)
                {
                    IsEof = true;
                    IsEoStr = true;
                    return '\0';
                }
            }

            if (lsCurrentString.Length == currentCharIndex)
            {
                IsEoStr = true;
                return '\0';
            }

            char c = lsCurrentString[currentCharIndex];
            currentCharIndex++;

            return c;
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream == null)
            {
                IsEof = true;
                return;
            }

            _localStream.BaseStream.Position = 0;
            IsEof = false;
        }
    }
}
