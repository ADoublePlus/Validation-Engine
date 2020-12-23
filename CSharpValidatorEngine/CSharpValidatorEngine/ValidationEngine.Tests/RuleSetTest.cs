using System.Collections.Generic;

using ValidationEngine.DataStructures;
using ValidationEngine.Engine;
using ValidationEngine.Tests.Helpers;

using NUnit.Framework;

namespace ValidationEngine.Tests
{
    [TestFixture]
    public class RuleSetTest
    {
        [Test]
        public void Validate_a_ruleset_with_valid_data_and_oninvalid_delegate()
        {
            RuleSet rs = new RuleSet()
            {
                OnInvalid = (prop) => { }, // Do nothing
                Rules = new List<Rule>()
                {
                    new Rule()
                    {
                        Name = "Test rule",
                        ValidationExpression = "^.*Test.*$",
                        FieldName = "TestProp",
                        FieldPath = "TestEntity.SubClassArray",
                        ArrayIndex = 0
                    }
                }
            };

            TestEntity t = new TestEntity()
            {
                SubClassArray = new SubEntity[]
                {
                    new SubEntity()
                    {
                        TestProp = "this is a TEST"
                    }
                }
            };

            bool validationResult = rs.Run(t);

            Assert.IsTrue(validationResult);
        }

        [Test]
        public void Validate_a_ruleset_with_invalid_data_and_oninvalid_and_fieldcompare()
        {
            RuleSet rs = new RuleSet()
            {
                OnInvalid = (prop) => { }, // Do nothing
                Rules = new List<Rule>()
                {
                    new Rule()
                    {
                        Name = "Test rule",
                        FieldName = "StringArray",
                        FieldPath = "TestEntity",
                        FieldComparisonList = new List<FieldComparison>()
                        {
                            new FieldComparison()
                            {
                                FieldName = "SecondarySub",
                                FieldPath = "SubEntity"
                            }
                        }
                    }
                }
            };

            TestEntity t = new TestEntity()
            {
                StringArray = new string[] { "valor1", "valor1" }
            };

            SubEntity t1 = new SubEntity()
            {
                SecondarySub = new string[] { "valor1", "valor2" }
            };

            bool validationResult = rs.Run(t, t1);

            Assert.IsFalse(validationResult);
        }

        [Test]
        public void Validate_a_ruleset_with_mismatched_array_size_and_oninvalid_and_filecompare()
        {
            RuleSet rs = new RuleSet()
            {
                OnInvalid = (prop) => { }, // Do nothing
                Rules = new List<Rule>()
                {
                    new Rule()
                    {
                        Name = "Test rule",
                        FieldName = "StringArray",
                        FieldPath = "TestEntity",
                        FieldComparisonList = new List<FieldComparison>()
                        {
                            new FieldComparison()
                            {
                                FieldName = "SecondarySub",
                                FieldPath = "SubEntity"
                            }
                        }
                    }
                }
            };

            TestEntity t = new TestEntity()
            {
                StringArray = new string[] { "valor1" }
            };

            SubEntity t1 = new SubEntity()
            {
                SecondarySub = new string[] { "valor1", "valor2" }
            };

            bool validationResult = rs.Run(t, t1);

            Assert.IsFalse(validationResult);
        }

        [Test]
        public void Validate_a_ruleset_with_invalid_data_and_oninvalid_delegate_and_regex()
        {
            RuleSet rs = new RuleSet()
            {
                OnInvalid = (prop) => { }, // Do nothing
                Rules = new List<Rule>()
                {
                    new Rule()
                    {
                        Name = "Test rule",
                        ValidationExpression = "^.*TEST.*$",
                        FieldName = "TestProp",
                        FieldPath = "TestEntity.SubClassArray",
                        ArrayIndex = 0
                    }
                }
            };

            TestEntity t = new TestEntity()
            {
                SubClassArray = new SubEntity[]
                {
                    new SubEntity()
                    {
                        TestProp = "this is not a...."
                    }
                }
            };

            bool validationResult = rs.Run(t);

            Assert.IsFalse(validationResult);
        }
    }
}