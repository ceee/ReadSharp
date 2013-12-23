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


    [Fact]
    public async Task DoesUseDeepLinksWork()
    {
      Article result = await reader.Read(new Uri("https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering"), true, false, true);

      Assert.Contains("<a href=\"#Browser_compatibility\">", result.Content);

      result = await reader.Read(new Uri("https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering"), true, false, false);

      Assert.DoesNotContain("<a href=\"#Browser_compatibility\">", result.Content);
      Assert.Contains("<a href=\"https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering#Browser_compatibility\">", result.Content);
    }


    [Fact]
    public async Task TestCzechCharsets()
    {
      string expectedTitle = "Kouzelné české Vánoce";
      Article result = await reader.Read(new Uri("http://www.czech.cz/cz/Zivot-a-prace/Jak-se-zije-v-CR/Zvyky-a-tradice/Kouzelne-ceske-Vanoce"));
      Assert.Equal(result.Title, expectedTitle);

      expectedTitle = "Kolik se dá vydělat na volné noze?";
      result = await reader.Read(new Uri("http://navolnenoze.cz/blog/vydelky/"));
      Assert.Equal(result.Title, expectedTitle);
    }


    [Fact]
    public async Task TestDifferentCharsets()
    {
      string expectedTitle = "优艺客-专注互联网品牌建设-原韩雪冬网页设计工作室（公司站）";
      Article result = await reader.Read(new Uri("http://www.uelike.com"));
      Assert.Equal(result.Title, expectedTitle);
    }
  }
}