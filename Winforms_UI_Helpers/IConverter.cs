namespace CardPlay.Utilities
{
    public interface IConverter
    {
        object Convert(object input);
    }

    public interface IConverter<TInputType, TOutputType> : IConverter
    {

    }
}
