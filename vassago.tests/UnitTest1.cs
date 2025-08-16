namespace vassago.tests;
using vassago.Models;

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
        var u = new User();
        Assert.Pass();
    }
    [Test]
    public void Test3()
    {
	    Assert.Fail();
    }
}
