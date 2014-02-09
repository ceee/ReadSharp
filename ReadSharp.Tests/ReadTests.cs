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
      Article result = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"), new ReadOptions()
      {
        HasNoHeadline = true,
        HasHeaderTags = true
      });

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
      await ThrowsAsync<ReadException>(async () =>
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
      Article result = await reader.Read(new Uri("https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering"), new ReadOptions()
      {
        UseDeepLinks = true
      });

      Assert.Contains("<a href=\"#Browser_compatibility\">", result.Content);

      result = await reader.Read(new Uri("https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering"), new ReadOptions()
      {
        UseDeepLinks = false
      });

      Assert.DoesNotContain("<a href=\"#Browser_compatibility\">", result.Content);
      Assert.Contains("<a href=\"https://developer.mozilla.org/en-US/docs/Web/CSS/image-rendering#Browser_compatibility\">", result.Content);
    }


    [Fact]
    public async Task ArePropertiesCorrectlyAssignedWithEmptyArticle()
    {
      Article result = await reader.Read(new Uri("https://docs.google.com/presentation/d/1n4NyG4uPRjAA8zn_pSQ_Ket0RhcWC6QlZ6LMjKeECo0/preview?sle=true#slide=id.g178014302_016"));
      Assert.False(result.ContentExtracted);
      Assert.True(result.WordCount == 7);
      Assert.Equal(result.Title, result.PlainContent);
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

      expectedTitle = "Zkázoděl | dialog.ihned.cz - Komentáře";
      result = await reader.Read(new Uri("http://dialog.ihned.cz/komentare/c1-61530110-zkazodel"));
      Assert.Equal(result.Title, expectedTitle);
    }


    [Fact]
    public async Task TestDifferentCharsets()
    {
      // chinese?
      string expectedTitle = "优艺客-专注互联网品牌建设-原韩雪冬网页设计工作室（公司站）";
      Article result = await reader.Read(new Uri("http://www.uelike.com"));
      Assert.Equal(result.Title, expectedTitle);

      // arabic
      result = await reader.Read(new Uri("http://www.it-scoop.com/2014/01/internet-of-things-google-nest/"));
      Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task TestCriticalURIs()
    {
      Article result = await reader.Read(new Uri("http://wpcentral.com.feedsportal.com/c/33999/f/616880/s/35a02b5e/sc/15/l/0L0Swpcentral0N0Cgameloft0Ediscusses0Etheir0Enew0Egame0Ebrothers0Earms0E30Esons0Ewar0Eceslive/story01.htm"));
      Assert.NotEmpty(result.Content);

      result = await reader.Read(new Uri("http://www.fastcoexist.com/3016005/futurist-forum/10-creative-ideas-for-thriving-cities-of-the-future"));
      Assert.Contains("1: 311", result.Content);

      result = await reader.Read(new Uri("http://msdn.microsoft.com/en-us/library/windows/apps/hh464925.aspx"));
      Assert.NotEmpty(result.Content);

      result = await reader.Read(new Uri("http://bit.ly/KAh7FJ"));
      Assert.NotEmpty(result.Content);

      result = await reader.Read(new Uri("http://www.nytimes.com/2014/01/31/world/europe/ukraine-unrest.html?hp&_r=0"));
      Assert.True(result.Images != null && result.Images.Count > 0);

      result = await reader.Read(new Uri("http://www.polygon.com/2013/2/25/4026668/tomb-raider-review"));
      Assert.True(result.Images != null && result.Images.Count > 3 && result.Content.Contains("For a reboot of a series that had lost its focus and purpose"));

      result = await reader.Read(new Uri("http://www.polygon.com/2014/1/31/5364728/super-bowl-xlviii-xbox-activities-new-york"));
      Assert.True(result.Content.Contains("week for Super Bowl XLVIII") && result.Content.Contains("two tickets to the Super Bowl."));
    }

    [Fact]
    public async Task TestHintUrlsReturnFullArticles()
    {
      Article result = await reader.Read(new Uri("http://www.theverge.com/2013/11/18/5116360/nokia-lumia-1520-review"));
      Assert.Contains("Three years ago, Nokia shipped over 110 million smartphones worldwide. ", result.Content);
      Assert.True(result.Content.Length > 6000);

      result = await reader.Read(new Uri("http://blog.bufferapp.com/connections-in-the-brain-understanding-creativity-and-intelligenceconnections"));
      Assert.Contains("The Tweet resulted in over 1,000 retweets", result.Content);
    }

    [Fact]
    public async Task AreMultipageArticlesWorking()
    {
      Article result = await reader.Read(new Uri("http://www.pcwelt.de/ratgeber/Acht_Tipps_fuer_die_sichere_Cloud-Google_Drive__Microsoft_Skydrive__Teamdrive-8205869.html?redirect=1"));
      Assert.NotNull(result.NextPage);

      result = await reader.Read(new Uri("http://arstechnica.com/apple/2014/01/two-steps-forward-a-review-of-the-2013-mac-pro/"));
      Assert.NotNull(result.NextPage);
    }
  }
}