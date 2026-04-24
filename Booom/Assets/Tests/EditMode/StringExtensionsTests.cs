using NUnit.Framework;

public class StringExtensionsTests
{
    [Test]
    public void AddSpacesBeforeCaps_AddsSpacesBetweenWords()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("HelloWorld");
        Assert.AreEqual("Hello World", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_PreservesExistingSpaces()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("Hello World");
        Assert.AreEqual("Hello World", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesEmptyString()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("");
        Assert.AreEqual("", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesNull()
    {
        string result = StringExtensions.AddSpacesBeforeCaps(null);
        Assert.AreEqual("", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesSingleCharacter()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("A");
        Assert.AreEqual("A", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesMultipleCaps()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("ThisIsATest");
        Assert.AreEqual("This Is ATest", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesLowerCaseOnly()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("hello");
        Assert.AreEqual("hello", result);
    }

    [Test]
    public void AddSpacesBeforeCaps_HandlesConsecutiveCaps()
    {
        string result = StringExtensions.AddSpacesBeforeCaps("XMLParser");
        Assert.AreEqual("XMLParser", result);
    }
}