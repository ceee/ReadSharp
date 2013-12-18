using System;
using System.Threading.Tasks;
using Xunit;

namespace ReadSharp.Tests
{
  public class ReadTests : TestsBase
  {
    private Reader reader;


    public ReadTests()
      : base()
    {
      reader = new Reader();
    }


    [Fact]
    public async Task ReadArticleTest()
    {
      Article result = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"));

      Assert.DoesNotContain("<!DOCTYPE html>", result.Content);
      Assert.Contains("<h1>", result.Content);
      Assert.True(result.Content.Length > 15000);
    }


    [Fact]
    public async Task ReadArticleWithContainerNoHeadlineTest()
    {
      Article result = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"), false, true);

      Assert.Contains("<!DOCTYPE html>", result.Content);
      Assert.DoesNotContain("<h1>", result.Content);
      Assert.True(result.Content.Length > 15000);
    }


    [Fact]
    public async Task ReadArticleWithImagesTest()
    {
      Article result = await reader.Read(new Uri("https://hacks.mozilla.org/2013/12/application-layout-with-css3-flexible-box-module/"));
      Assert.True(result.Images.Count >= 3);
      Assert.True(result.Images[0].Uri.ToString().StartsWith("https://hacks.mozilla.org"));
      Assert.True(result.Images[1].Uri.ToString().EndsWith(".gif"));
    }


    [Fact]
    public async Task ReadArticleWithNoImagesTest()
    {
      Article result = await reader.Read(new Uri("http://getpocket.com/hits/awards/2013/"));
      Assert.True(result.Images == null || result.Images.Count < 1);
    }


    [Fact]
    public async Task ReadArticleWithInvalidUriTest()
    {
      await ThrowsAsync<Exception>(async () =>
      {
        await reader.Read(new Uri("http://frontendplayyyyy.com"));
      });
    }


    [Fact]
    public async Task IsBodyOnlyProperlyResolved()
    {
      Article result = await reader.Read(new Uri("http://calebjacob.com/tooltipster/"));

      Assert.True(result.Content.Substring(0, 4) == "<div");
    }
  }
}