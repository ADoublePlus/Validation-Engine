using System.Collections.Generic;

using ValidationEngine.Engine;

namespace ValidationEngine.DataStructures
{
    public class ValidationResult
    {
        public bool Succeeded { get; set; }
        public List<Rule> SucceededRules { get; set; }
        public List<Rule> FailedRules { get; set; }
        public List<Rule> NotRanRules { get; set; }
    }
}