using System;
using System.Collections.Generic;
using System.Linq;

using ValidationEngine.DataStructures;

namespace ValidationEngine.Engine
{
    public class RuleSet
    {
        internal List<Rule> SucceededRules { get; set; }
        internal List<Rule> FailedRules { get; set; }
        internal List<Rule> NotRanRules { get; set; }

        public bool SkipOnError { get; set; }
        public List<Rule> Rules { get; set; }
        public Action<ComparableProperty> OnInvalid { get; set; }

        internal bool Run(params object[] entities)
        {
            this.SucceededRules = new List<Rule>();
            this.FailedRules = new List<Rule>();
            this.NotRanRules = new List<Rule>();

            bool succeed = false;

            foreach (var rule in Rules)
            {
                // Assign the OnInvalid delegate that was defined on this RuleSet
                rule.OnInvalid = OnInvalid;

                succeed = rule.Validate(entities);

                if (!succeed)
                {
                    // If SkipOnError is active, we assume everything is ok if the first test fails
                    if (this.SkipOnError && this.SucceededRules.Count == 0 && this.FailedRules.Count == 0)
                    {
                        succeed = true;
                        break;
                    }

                    this.FailedRules.Add(rule);
                    this.NotRanRules.AddRange(Rules.Where(r => !this.FailedRules.Contains(r) && !this.SucceededRules.Contains(r)));

                    break;
                }
                else
                {
                    this.SucceededRules.Add(rule);
                }
            }

            return succeed;
        }
    }
}