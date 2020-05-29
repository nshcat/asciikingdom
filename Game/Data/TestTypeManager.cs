namespace Game.Data
{
    public class TestTypeManager : TypeClassLoader<TestType>
    {
        public TestTypeManager() : base("test.json")
        {
        }
    }
}