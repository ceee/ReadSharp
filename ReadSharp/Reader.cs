using ReadSharp.Ports.NReadability;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace ReadSharp
{
  /// <summary>
  /// PocketReader
  /// </summary>
  public class Reader : IReader
  {
    /// <summary>
    /// REST client used for HTML retrieval
    /// </summary>
    protected readonly HttpClient _httpClient;

    /// <summary>
    /// The encoder
    /// </summary>
    protected readonly Encodings.Encoder _encoder;

    /// <summary>
    /// The NReadability transcoder
    /// </summary>
    protected NReadabilityTranscoder _transcoder;

    /// <summary>
    /// The HTTP options
    /// </summary>
    protected HttpOptions _options;

    /// <summary>
    /// The current pages
    /// </summary>
    protected List<string> _currentPages = new List<string>();

    /// <summary>
    /// The raw HTML from the last request
    /// </summary>
    private string _rawHTML = String.Empty;

    /// <summary>
    /// Redirect faulty mobile URIs to desktop equivalents
    /// </summary>
    private static readonly Dictionary<string, string> _redirectFaultyMobileURIs = new Dictionary<string, string>
    {
      { "//m.spiegel.de", "//www.spiegel.de" }
    };



    /// <summary>
    /// Initializes a new instance of the <see cref="Reader" /> class.
    /// </summary>
    /// <param name="options">The HTTP options.</param>
    public Reader(HttpOptions options = null)
    {
      // initialize transcoder
      _transcoder = new NReadabilityTranscoder(
        dontStripUnlikelys: false,
        dontNormalizeSpacesInTextContent: true,
        dontWeightClasses: false,
        readingStyle: ReadingStyle.Ebook,
        readingMargin: ReadingMargin.Narrow,
        readingSize: ReadingSize.Medium
      );

      // get default HTTP options if none available
      if (options == null)
      {
        options = HttpOptions.CreateDefault();
      }

      _options = options;

      // initialize custom encoder
      _encoder = new Encodings.Encoder(true);

      // initialize HTTP client
      _httpClient = new HttpClient(options.CustomHttpHandler ?? new HttpClientHandler()
      {
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
        AllowAutoRedirect = true
      });

      if (options.RequestTimeout.HasValue)
      {
        _httpClient.Timeout = TimeSpan.FromSeconds(options.RequestTimeout.Value);
      }

      // add accept types
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

      // add accepted encodings
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip,deflate");

      // add user agent
      string userAgent = options.UseMobileUserAgent ? options.UserAgentMobile : options.UserAgent;

      string version = typeof(Reader).GetTypeInfo().Assembly.FullName.Split(',')[1].Split('=')[1];

      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", String.Format(userAgent, "; ReadSharp/" + version));
    }



    /// <summary>
    /// Reads article content from the given URI.
    /// </summary>
    /// <param name="uri">An URI to extract the content from.</param>
    /// <param name="options">The transform options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An article with extracted content and meta information.
    /// </returns>
    /// <exception cref="ReadException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public async Task<Article> Read(Uri uri, ReadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
      _currentPages = new List<string>();

      Response response;
      string uriString = uri.OriginalString;

      if (options == null)
      {
        options = ReadOptions.CreateDefault();
      }

      // replace domain when URI is marked as faulty
      foreach (string faultyUri in _redirectFaultyMobileURIs.Keys)
      {
        if (uriString.Contains(faultyUri))
        {
          uri = new Uri(uriString.Replace(faultyUri, _redirectFaultyMobileURIs[faultyUri]));
        }
      }

      // make async request
      response = await Request(uri, options, null, cancellationToken);

      // get images from article
      int id = 1;
      IEnumerable<ArticleImage> images = response.TranscodingResult.Images
        .Select(image =>
        {
          Uri imageUri = null;
          Uri.TryCreate(image.GetAttributeValue("src", null), UriKind.Absolute, out imageUri);

          return new ArticleImage()
          {
            ID = (id++).ToString(),
            Uri = imageUri,
            Title = image.GetAttributeValue("title", null),
            AlternativeText = image.GetAttributeValue("alt", null)
          };
        });
        //.GroupBy(image => image.Uri)
        //.Select(g => g.First())
        //.Where(image => image.Uri != null);

      // get word count and plain text
      string plainContent;
      int wordCount = 0;

      try
      {
        plainContent = HtmlUtilities.ConvertToPlainText(response.TranscodingResult.ExtractedContent);
        wordCount = HtmlUtilities.CountWords(plainContent);
      }
      catch
      {
        plainContent = null;
      }

      // create article
      return new Article()
      {
        Title = response.TranscodingResult.ExtractedTitle,
        Description = response.TranscodingResult.ExtractedDescription,
        Content = response.TranscodingResult.ExtractedContent,
        ContentExtracted = response.TranscodingResult.ContentExtracted ? wordCount > 0 : false,
        Raw = _rawHTML,
        PlainContent = plainContent,
        WordCount = wordCount,
        PageCount = response.PageCount,
        FrontImage = response.TranscodingResult.ExtractedImage,
        Images = images,
        Favicon = response.TranscodingResult.ExtractedFavicon,
        NextPage = response.TranscodingResult.NextPageUrl != null ? new Uri(response.TranscodingResult.NextPageUrl, UriKind.Absolute) : null,
        Encoding = response.Encoding
      };
    }



    /// <summary>
    /// Extracts the readable information.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="textStream">The text stream.</param>
    /// <param name="options">The options.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns></returns>
    protected TranscodingResult ExtractReadableInformation(
      Uri uri,
      Stream textStream,
      ReadOptions options,
      Encoding encoding = null)
    {
      // response stream to text
      textStream.Position = 0;
      StreamReader streamReader = new StreamReader(textStream, encoding ?? Encoding.UTF8);
      _rawHTML = streamReader.ReadToEnd();

      // set properties for processing
      TranscodingInput transcodingInput = new TranscodingInput(_rawHTML)
      {
        Url = uri.ToString(),
        DomSerializationParams = new DomSerializationParams()
        {
          BodyOnly = !options.HasHeaderTags,
          NoHeadline = !options.HasHeadline,
          PrettyPrint = options.PrettyPrint,
          DontIncludeContentTypeMetaElement = true,
          DontIncludeMobileSpecificMetaElements = true,
          DontIncludeDocTypeMetaElement = false,
          DontIncludeGeneratorMetaElement = true,
          ReplaceImagesWithPlaceholders = options.ReplaceImagesWithPlaceholders
        }
      };

      // process/transcode HTML
      return _transcoder.Transcode(transcodingInput);
    }



    /// <summary>
    /// Reverses the deep links.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    private AttributeTransformationResult ReverseDeepLinks(AttributeTransformationInput input)
    {
      string articleUrl = input.ArticleUrl;
      string link = input.AttributeValue;

      // remove deep-link if in article URI
      if (articleUrl.Contains("#"))
      {
        articleUrl = articleUrl.Split('#')[0];
      }

      // anchor is a deep-link
      if (
        input.AttributeValue.Contains(articleUrl) &&
        input.AttributeValue.Contains("#") &&
        input.AttributeValue.Split('#')[1].Length > 0)
      {
        link = "#" + input.AttributeValue.Split('#')[1];
      }

      return new AttributeTransformationResult()
      {
        TransformedValue = link
      };
    }



    /// <summary>
    /// Fetches a resource
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="options">The options.</param>
    /// <param name="isContinuedPage">if set to <c>true</c> [is continued page].</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ReadException">
    /// </exception>
    private async Task<Response> Request(Uri uri, ReadOptions options, Response previousResponse, CancellationToken cancellationToken)
    {
      // URI already fetched
      if (previousResponse != null && _currentPages.Contains(uri.OriginalString))
      {
        return previousResponse;
      }
      _currentPages.Add(uri.OriginalString);

      HttpResponseMessage response = null;
      TranscodingResult transcodingResult;
      Encoding encoding;

      using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
      {
        // make async request
        try
        {
          response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exc)
        {
          throw new ReadException(exc.Message);
        }

        // validate HTTP response
        if (response.StatusCode != HttpStatusCode.OK)
        {
          string exceptionString = String.Format("Request error: {0} ({1})", response.ReasonPhrase, (int)response.StatusCode);

          throw new ReadException(exceptionString);
        }
      }

      // read response
      Stream responseStream = await response.Content.ReadAsStreamAsync();

      string charset = response.Content.Headers.ContentType.CharSet;

      // handle deep links
      if (options.UseDeepLinks)
      {
        _transcoder.AnchorHrefTranformer = ReverseDeepLinks;
      }
      else
      {
        _transcoder.AnchorHrefTranformer = null;
      }

      // readability
      try
      {
        // charset found in HTTP headers
        encoding = _encoder.GetEncodingFromString(charset);

        // transcode content
        transcodingResult = ExtractReadableInformation(uri, responseStream, options, encoding);

        // get encoding found in HTML
        Encoding encodingFromHTML = _encoder.GetEncodingFromString(transcodingResult.Charset);

        // extract again if encoding didn't match or failed to retrieve
        if ((encoding != null && String.IsNullOrEmpty(charset))
          ||
          (options.PreferHTMLEncoding && !String.Equals(charset, transcodingResult.Charset, StringComparison.OrdinalIgnoreCase)))
        {
          transcodingResult = ExtractReadableInformation(uri, responseStream, options, encodingFromHTML);
          encoding = encodingFromHTML;
        }
      }
      catch (Exception exc)
      {
        throw new ReadException(exc.Message);
      }
      finally
      {
        response.Dispose();
        responseStream.Dispose();
      }

      Response newResponse = new Response()
      {
        TranscodingResult = transcodingResult,
        PageCount = 1,
        Encoding = encoding
      };

      // in same special cases their are multiple pages, which are only comments or do not contain new content.
      // if this is the case we will break here and return the first page only.
      if (previousResponse != null && previousResponse.TranscodingResult.ExtractedContent.Contains(transcodingResult.ExtractedContent))
      {
        previousResponse.TranscodingResult.NextPageUrl = null;
        return previousResponse;
      }

      // multiple pages are available
      try
      {
        if (options.MultipageDownload && transcodingResult.NextPageUrl != null && (previousResponse == null || (previousResponse != null && previousResponse.PageCount < _options.MultipageLimit)))
        {
          return await Request(new Uri(transcodingResult.NextPageUrl), new ReadOptions()
          {
            PrettyPrint = options.PrettyPrint,
            UseDeepLinks = options.UseDeepLinks,
            MultipageDownload = true
          }, previousResponse != null ? MergeResponses(previousResponse, newResponse) : newResponse, cancellationToken);
        }

        // this is not the first page
        if (previousResponse != null)
        {
          return MergeResponses(previousResponse, newResponse);
        }
      }
      // silently fail when next pages fail to download
      catch { }

      return newResponse;
    }


    private Response MergeResponses(Response original, Response append)
    {
      if (original == null)
      {
        return append;
      }
      if (append == null)
      {
        return original;
      }

      TranscodingResult mergedResult = original.TranscodingResult;

      mergedResult.ExtractedContent += String.Format("<div class=\"readability-nextpage\" data-page=\"{0}\"></div>{1}", (original.PageCount + 1).ToString(), append.TranscodingResult.ExtractedContent);

      if (mergedResult.Images == null || mergedResult.Images.Count() == 0)
      {
        mergedResult.Images = append.TranscodingResult.Images;
      }
      else if (append.TranscodingResult.Images != null && append.TranscodingResult.Images.Count() > 0)
      {
        List<XElement> images = mergedResult.Images.ToList();
        images.AddRange(append.TranscodingResult.Images);
        mergedResult.Images = images;
      }

      mergedResult.NextPageUrl = append.TranscodingResult.NextPageUrl;

      return new Response()
      {
        PageCount = original.PageCount + 1,
        Encoding = original.Encoding,
        TranscodingResult = mergedResult
      };
    }
  }
}