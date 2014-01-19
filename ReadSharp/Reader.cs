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

      string version = Assembly.GetExecutingAssembly().FullName.Split(',')[1].Split('=')[1];

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
      Response response;
      TranscodingResult transcodingResult;
      Encoding encoding;

      if (options == null)
      {
        options = ReadOptions.CreateDefault();
      }

      // make async request
      try
      {
        // get HTML string from URI
        response = await Request(uri, cancellationToken);
      }
      catch (HttpRequestException exc)
      {
        throw new ReadException(exc.Message);
      }

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
        encoding = _encoder.GetEncodingFromString(response.Charset);

        // transcode content
        transcodingResult = ExtractReadableInformation(uri, response.Stream, options, encoding);

        // get encoding found in HTML
        encoding = _encoder.GetEncodingFromString(transcodingResult.Charset);

        // extract again if encoding didn't match or failed to retrieve
        if (encoding != null && (
          String.IsNullOrEmpty(response.Charset)
          ||
          !String.Equals(response.Charset, transcodingResult.Charset, StringComparison.OrdinalIgnoreCase)))
        {
          transcodingResult = ExtractReadableInformation(uri, response.Stream, options, encoding);
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
      string text = streamReader.ReadToEnd();

      // set properties for processing
      TranscodingInput transcodingInput = new TranscodingInput(text)
      {
        Url = uri.ToString(),
        DomSerializationParams = new DomSerializationParams()
        {
          BodyOnly = !options.HasHeaderTags,
          NoHeadline = options.HasNoHeadline,
          PrettyPrint = options.PrettyPrint,
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ReadException">
    /// </exception>
    private async Task<Response> Request(Uri uri, CancellationToken cancellationToken)
    {
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
      HttpResponseMessage response = null;

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