using ReadSharp.Ports.NReadability;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
    /// Used UserAgent for HTTP request
    /// </summary>
    protected string _userAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; ARM; Mobile; Touch{0}) like Gecko";

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
    /// Initializes a new instance of the <see cref="Reader" /> class.
    /// </summary>
    /// <param name="userAgent">Custom UserAgent string.</param>
    /// <param name="handler">The HttpMessage handler.</param>
    /// <param name="timeout">Request timeout (in seconds).</param>
    public Reader(string userAgent = null, HttpMessageHandler handler = null, int? timeout = null)
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

      // initialize custom encoder
      _encoder = new Encodings.Encoder(true);

      // override user agent
      if (!string.IsNullOrEmpty(userAgent))
      {
        _userAgent = userAgent;
      }

      // initialize HTTP client
      _httpClient = new HttpClient(handler ?? new HttpClientHandler()
      {
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
        AllowAutoRedirect = true
      });

      if (timeout.HasValue)
      {
        _httpClient.Timeout = TimeSpan.FromSeconds(timeout.Value);
      }

      // add accept types
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

      // add accepted encodings
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip,deflate");

      //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "UTF-8");

      // add user agent
      string version = Assembly.GetExecutingAssembly().FullName.Split(',')[1].Split('=')[1];
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", String.Format(_userAgent, "; ReadSharp/" + version));
    }



    /// <summary>
    /// Reads article content from the given URI.
    /// </summary>
    /// <param name="uri">An URI to extract the content from.</param>
    /// <param name="bodyOnly">if set to <c>true</c> [only body is returned].</param>
    /// <param name="noHeadline">if set to <c>true</c> [no headline (h1) is included in generated HTML].</param>
    /// <param name="useDeepLinks">if set to <c>true</c> [deep links with hashes are not transformed to absolute URIs].</param>
    /// <returns>
    /// An article with extracted content and meta information.
    /// </returns>
    /// <exception cref="Exception"></exception>
    public async Task<Article> Read(
      Uri uri,
      bool bodyOnly = true,
      bool noHeadline = false,
      bool useDeepLinks = false)
    {
      Response response;
      TranscodingResult transcodingResult;
      Encoding encoding;

      // make async request
      try
      {
        // get HTML string from URI
        response = await Request(uri);
      }
      catch (HttpRequestException exc)
      {
        throw new ReadException(exc.Message);
      }

      // handle deep links
      if (useDeepLinks)
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
        transcodingResult = ExtractReadableInformation(uri, response.Stream, bodyOnly, noHeadline);

        encoding = _encoder.GetEncodingFromString(transcodingResult.Charset);

        // extract again if encoding didn't match or failed to retrieve
        if (encoding != null && (
          String.IsNullOrEmpty(response.Charset)
          ||
          !String.Equals(response.Charset, transcodingResult.Charset, StringComparison.OrdinalIgnoreCase)))
        {
          transcodingResult = ExtractReadableInformation(uri, response.Stream, bodyOnly, noHeadline, encoding);
        }
      }
      catch (Exception exc)
      {
        throw new ReadException(exc.Message);
      }

      // get images from article
      int id = 1;

      List<ArticleImage> images = transcodingResult.Images.Select<XElement, ArticleImage>(image =>
      {
        Uri imageUri;
        Uri.TryCreate(image.GetAttributeValue("src", null), UriKind.Absolute, out imageUri);

        return new ArticleImage()
        {
          ID = (id++).ToString(),
          Uri = imageUri,
          Title = image.GetAttributeValue("title", null),
          AlternativeText = image.GetAttributeValue("alt", null)
        };
      }).ToList();

      // create article
      return new Article()
      {
        Title = transcodingResult.ExtractedTitle,
        Description = transcodingResult.ExtractedDescription,
        Content = transcodingResult.ExtractedContent,
        FrontImage = transcodingResult.ExtractedImage,
        Images = images,
        Favicon = transcodingResult.ExtractedFavicon,
        NextPage = transcodingResult.NextPageUrl != null ? new Uri(transcodingResult.NextPageUrl, UriKind.Absolute) : null,
        Encoding = _encoder.GetEncodingFromString(response.Charset) ?? encoding ?? null
      };
    }



    /// <summary>
    /// Extracts the readable information.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="textStream">The text stream.</param>
    /// <param name="bodyOnly">if set to <c>true</c> [body only].</param>
    /// <param name="noHeadline">if set to <c>true</c> [no headline].</param>
    /// <returns></returns>
    protected TranscodingResult ExtractReadableInformation(
      Uri uri,
      Stream textStream,
      bool bodyOnly = true,
      bool noHeadline = false,
      Encoding encoding = null)
    {
      // response stream to text
      textStream.Position = 0;
      StreamReader streamReader = new StreamReader(textStream, encoding ?? Encoding.UTF8);
      string text = streamReader.ReadToEnd();

      // set properties for processing
      TranscodingInput transcodingInput = new TranscodingInput(text)
      {
        Url = uri.ToString(),
        DomSerializationParams = new DomSerializationParams()
        {
          BodyOnly = bodyOnly,
          NoHeadline = noHeadline,
          PrettyPrint = true,
          DontIncludeContentTypeMetaElement = true,
          DontIncludeMobileSpecificMetaElements = true,
          DontIncludeDocTypeMetaElement = false,
          DontIncludeGeneratorMetaElement = true
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
    /// <returns></returns>
    private async Task<Response> Request(Uri uri)
    {
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
      HttpResponseMessage response = null;

      // make async request
      try
      {
        response = await _httpClient.SendAsync(request);
      }
      catch (HttpRequestException exc)
      {
        throw new ReadException(exc.Message, exc);
      }

      // validate HTTP response
      if (response.StatusCode != HttpStatusCode.OK)
      {
        string exceptionString = String.Format("Request error: {0} ({1})", response.ReasonPhrase, (int)response.StatusCode);

        throw new ReadException(exceptionString);
      }

      // read response
      Stream responseStream = await response.Content.ReadAsStreamAsync();

      return new Response()
      {
        Stream = responseStream,
        Charset = response.Content.Headers.ContentType.CharSet
      };
    }
  }
}