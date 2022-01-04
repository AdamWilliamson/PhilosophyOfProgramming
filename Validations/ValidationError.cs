namespace Validations
{
    //=====================================
    // Validation Results
    public class ValidationError
    {
        public string Error { get; }
        public bool IsFatal { get; } = false;

        public ValidationError(string error) : this(error, false) {}

        public ValidationError(string error, bool fatal) 
        { 
            Error = error;
            IsFatal = fatal;
        }
    }
}