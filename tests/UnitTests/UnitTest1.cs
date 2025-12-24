namespace UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [Test]
    public void Test2()
    {
        int result = 1 + 2;
        
        Assert.That(result, Is.EqualTo(3));
    }
}
