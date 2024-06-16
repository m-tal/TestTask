namespace TestTask
{
    /// <summary>
    /// Интерфейс для работы с файлом.
    /// </summary>
    internal interface IReadOnlyStream
    {
        // TODO : Необходимо доработать данный интерфейс для обеспечения гарантированного закрытия файла, по окончанию работы с таковым!
        char ReadNextChar();

        void ResetPositionToStart();

        void DisposeStream();

        bool IsEof { get; }

        bool IsEoStr { get; }
    }
}
