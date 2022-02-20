using System.Collections.Generic;
using System.Linq;

namespace Validations.Internal
{
    internal class StringReplacer
    {
        public StringReplacer(Dictionary<string, string> targets)
        {
            StartToken = "{";
            EndToken = "}";
            Targets = targets.Select(c => (c.Key, c.Value)).ToList();
        }

        public string StartToken { get; }
        public string EndToken { get; }
        public List<(string, string)> Targets { get; } = new();

        public void AddTarget(string target, string value)
        {
            Targets.Add(new (target, value)); 
        }

        public string Replace(string stringToReplace)
        {
            var copy = stringToReplace;
            foreach (var target in Targets)
            {
                copy = copy.Replace(target.Item1, target.Item2);
            }

            return copy;
        }
    }
}
