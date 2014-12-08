using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace ReadSharp.Tests
{
  public class ReadTests : TestsBase
  {
    private Reader reader;


    public ReadTests() : base()
    {
      reader = new Reader();
    }


    [Fact]
    public async Task ReadArticleTest()
    {
      Article result = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"));

      Assert.DoesNotContain("<!DOCTYPE html>", result.Content);
      Assert.True(result.Content.Length > 15000);
    }


    [Fact]
    public async Task ReadArticleWithContainerNoHeadlineTest()
    {
      Article result = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"), new ReadOptions()
      {
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
      List<ArticleImage> images = result.Images.ToList();
      Assert.True(images.Count >= 3);
      Assert.True(images[0].Uri.ToString().StartsWith("https://hacks.mozilla.org"));
      Assert.True(images[1].Uri.ToString().EndsWith(".gif"));
    }


    [Fact]
    public async Task ReadArticleWithImagePlaceholdersTest()
    {
      ReadOptions options = ReadOptions.CreateDefault();
      options.ReplaceImagesWithPlaceholders = true;
      Article result = await reader.Read(new Uri("https://hacks.mozilla.org/2013/12/application-layout-with-css3-flexible-box-module/"), options);
      List<ArticleImage> images = result.Images.ToList();

      Assert.True(images.Count >= 3);
      Assert.True(images[0].Uri.ToString().StartsWith("https://hacks.mozilla.org"));
      Assert.True(images[1].Uri.ToString().EndsWith(".gif"));

      Assert.Contains("<!--IMG_1-->", result.Content);
      Assert.DoesNotContain("<img ", result.Content);
    }


    [Fact]
    public async Task ReadArticleWithNoImagesTest()
    {
      Article result = await reader.Read(new Uri("http://getpocket.com/hits/awards/2013/"));
      Assert.True(result.Images == null || result.Images.Count() < 1);
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
      Article result = await reader.Read(new Uri("http://www.anandtech.com/show/7594/samsung-ssd-840-evo-msata-120gb-250gb-500gb-1tb-review"));
      Assert.Equal(result.NextPage.ToString(), "http://www.anandtech.com/show/7594/samsung-ssd-840-evo-msata-120gb-250gb-500gb-1tb-review/2");

      result = await reader.Read(new Uri("http://www.zeit.de/gesellschaft/2014-02/alice-schwarzer-steuerhinterziehung-doppelmoral"));
      Assert.Equal(result.NextPage.ToString(), "http://www.zeit.de/gesellschaft/2014-02/alice-schwarzer-steuerhinterziehung-doppelmoral/seite-2");

      result = await reader.Read(new Uri("http://www.sueddeutsche.de/wirtschaft/netzbetreiber-und-die-energiewende-im-kampf-gegen-blackouts-und-buergerproteste-1.1880754"));
      Assert.Equal(result.NextPage.ToString(), "http://www.sueddeutsche.de/wirtschaft/netzbetreiber-und-die-energiewende-im-kampf-gegen-blackouts-und-buergerproteste-1.1880754-2");

      result = await reader.Read(new Uri("http://arstechnica.com/apple/2014/01/two-steps-forward-a-review-of-the-2013-mac-pro/"));
      Assert.Equal(result.NextPage.ToString(), "http://arstechnica.com/apple/2014/01/two-steps-forward-a-review-of-the-2013-mac-pro/2");
    }

    [Fact]
    public async Task AreSinglepageArticlesNotPopulatingNextPage()
    {
      Article result = await reader.Read(new Uri("http://www.wpcentral.com/developers-leak-new-features-windows-phone-81-sdk"), new ReadOptions() { MultipageDownload = true });
      Assert.Null(result.NextPage);
      Assert.Equal(result.PageCount, 1);

      result = await reader.Read(new Uri("http://arstechnica.com/apple/2014/01/two-steps-forward-a-review-of-the-2013-mac-pro/7/"));
      Assert.Null(result.NextPage);

      result = await reader.Read(new Uri("http://www.buzzfeed.com/mattlynley/the-16-most-interesting-things-to-come-out-of-bill-gates-qa"));
      Assert.Null(result.NextPage);

      result = await reader.Read(new Uri("http://www.sueddeutsche.de/wirtschaft/netzbetreiber-und-die-energiewende-im-kampf-gegen-blackouts-und-buergerproteste-1.1880754-2"));
      Assert.Null(result.NextPage);

      result = await reader.Read(new Uri("http://www.zeit.de/gesellschaft/2014-02/alice-schwarzer-steuerhinterziehung-doppelmoral/seite-2"));
      Assert.Null(result.NextPage);

    }

    [Fact]
    public async Task AreMultiPagesDownloadedAndMergedCorrectly()
    {
      ReadOptions options = new ReadOptions() { MultipageDownload = true };

      Article result = await reader.Read(new Uri("http://www.maximumpc.com/article/features/modders_toolkit_everything_you_need_make_kick-ass_custom_case_mods"), options);
      Assert.Equal(result.PageCount, 4);

      result = await reader.Read(new Uri("http://www.anandtech.com/show/7594/samsung-ssd-840-evo-msata-120gb-250gb-500gb-1tb-review"), options);
      Assert.Equal(result.PageCount, 9);

      result = await reader.Read(new Uri("http://www.zeit.de/gesellschaft/2014-02/alice-schwarzer-steuerhinterziehung-doppelmoral"), options);
      Assert.Equal(result.PageCount, 2);
      Assert.True(result.WordCount > 800);

      result = await reader.Read(new Uri("http://www.zeit.de/gesellschaft/2014-02/alice-schwarzer-steuerhinterziehung-doppelmoral"));
      Assert.True(result.PageCount == 1 && result.WordCount < 500);

      result = await reader.Read(new Uri("http://arstechnica.com/apple/2014/01/two-steps-forward-a-review-of-the-2013-mac-pro/"), options);
      Assert.Equal(result.PageCount, 7);
      Assert.True(result.WordCount > 13000 && result.Images.Count() > 10);

      result = await reader.Read(new Uri("http://www.sueddeutsche.de/wirtschaft/netzbetreiber-und-die-energiewende-im-kampf-gegen-blackouts-und-buergerproteste-1.1880754"), options);
      Assert.Equal(result.PageCount, 2);
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
      Assert.True(result.Images != null && result.Images.Count() > 0);

      result = await reader.Read(new Uri("http://www.polygon.com/2013/2/25/4026668/tomb-raider-review"));
      Assert.True(result.Images != null && result.Images.Count() > 3 && result.Content.Contains("For a reboot of a series that had lost its focus and purpose"));

      result = await reader.Read(new Uri("http://www.polygon.com/2014/1/31/5364728/super-bowl-xlviii-xbox-activities-new-york"));
      Assert.True(result.Content.Contains("week for Super Bowl XLVIII") && result.Content.Contains("two tickets to the Super Bowl."));

      result = await reader.Read(new Uri("http://habrahabr.ru/post/211905/"));
      Assert.NotEmpty(result.Content);

      result = await reader.Read(new Uri("http://www.dgtle.com/article-5682-1.html"));
      Assert.Contains("http://img.dgtle.com/forum/201402/13/162237x8oumb8i0i0y0087.jpeg!680px", result.Content);
    }

    [Fact]
    public async Task TestCriticalURIs2()
    {
      ReadOptions options;

      Article result = await reader.Read(new Uri("https://medium.com/best-thing-i-found-online-today/9e7455ca375b"));
      Assert.Contains("16. Be confident in how you ask", result.Content);

      result = await reader.Read(new Uri("http://www.dgtle.com/article-5682-1.html"));
      Assert.Contains("http://img.dgtle.com/forum/201402/13/162237x8oumb8i0i0y0087.jpeg!680px", result.Content);

      result = await reader.Read(new Uri("http://m.spiegel.de/spiegelgeschichte/a-946060.html"));
      Assert.DoesNotContain("Detecting browser settings", result.Content);

      result = await reader.Read(new Uri("https://vimeo.com/84391640"));
      Assert.Contains("twitter.com/pokiapp", result.Content);

      result = await reader.Read(new Uri("http://www.youtube.com/watch?v=GI2lHSPkW1c"));
      Assert.Contains("IT PAST MIDNIGHT A COUPLE HOURS AGO, IT'S FEELS COLDER", result.Content);

      result = await reader.Read(new Uri("http://www.jn.pt/PaginaInicial/Politica/Interior.aspx?content_id=3996648&utm_source=feedburner&utm_medium=feed&utm_campaign=Feed%3A+JN-ULTIMAS+%28JN+-+Ultimas%29"));
      Assert.DoesNotContain("Alberto João Jardim", result.Content);

      options = ReadOptions.CreateDefault();
      options.PreferHTMLEncoding = false;
      result = await reader.Read(new Uri("http://www.jn.pt/PaginaInicial/Politica/Interior.aspx?content_id=3996648&utm_source=feedburner&utm_medium=feed&utm_campaign=Feed%3A+JN-ULTIMAS+%28JN+-+Ultimas%29"), options);
      Assert.Contains("Alberto João Jardim", result.Content);
    }


    [Fact]
    public async Task DebugArticle()
    {
      string uri = "http://nstarikov.ru/blog/45260";

      Article result = await reader.Read(new Uri(uri), new ReadOptions()
      {
        MultipageDownload = true
      });
      Assert.Equal(true, result.ContentExtracted);
    }
  }
}