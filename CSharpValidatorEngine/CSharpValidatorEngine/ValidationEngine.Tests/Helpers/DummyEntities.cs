namespace ValidationEngine.Tests.Helpers
{
    internal class TestEntity
    {
        public SubEntity[] SubClassArray { get; set; }
        public string[] StringArray { get; set; }
        public SubEntity SubClass { get; set; }
    }

    internal class SubEntity
    {
        public string TestProp { get; set; }
        public string[] SecondarySub { get; set; }
    }
}