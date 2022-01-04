namespace CardPlay.Utilities
{
    public class IntToStringConverter : IConverter<int, string>
    {
        public object Convert(object input)
        {
            return input.ToString();
        }
    }
}
