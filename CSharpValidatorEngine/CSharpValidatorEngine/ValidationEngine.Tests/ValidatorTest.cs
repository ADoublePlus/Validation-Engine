using System.Collections.Generic;

using ValidationEngine.DataStructures;
using ValidationEngine.Engine;
using ValidationEngine.Tests.Helpers;

using NUnit.Framework;

namespace ValidationEngine.Tests
{
    [TestFixture]
    public class ValidatorTest
    {
        private Validator GetValidator()
        {
            return new Validator()
            {
                Name = "test validation",
                RuleSets = new List<RuleSet>()
                {
                    new RuleSet()
                    {
                        OnInvalid = (prop) => {}, // Do nothing
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
                    }
                }
            };
        }

        [Test]
        public void Test_validate_succeed()
        {
            Validator v = GetValidator();

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

            ValidationResult validationResult = v.Execute(t);

            Assert.IsTrue(validationResult.SucceededRules.Count == 1);
        }

        [Test]
        public void Test_validate_fail()
        {
            Validator v = GetValidator();

            TestEntity t = new TestEntity()
            {
                SubClassArray = new SubEntity[]
                {
                    new SubEntity()
                    {
                        TestProp = "this is not...."
                    }
                }
            };

            ValidationResult validationResult = v.Execute(t);

            Assert.IsTrue(validationResult.FailedRules.Count == 1);
        }
    }
}