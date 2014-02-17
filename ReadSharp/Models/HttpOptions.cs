using System.Net.Http;

namespace ReadSharp
{
  public class HttpOptions
  {
    /// <summary>
    /// Gets or sets the custom HTTP handler.
    /// </summary>
    /// <value>
    /// The custom HTTP handler.
    /// </value>
    public HttpMessageHandler CustomHttpHandler { get; set; }

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    /// <value>
    /// The timeout after which the requests cancels.
    /// </value>
    public int? RequestTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [use mobile user agent].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [use mobile user agent]; otherwise, <c>false</c>.
    /// </value>
    public bool UseMobileUserAgent { get; set; }

    /// <summary>
    /// Used UserAgent for HTTP request.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Used mobile UserAgent for HTTP request.
    /// </summary>
    public string UserAgentMobile { get; set; }

    /// <summary>
    /// Gets or sets the download limit for articles with multiple pages (default: 10).
    /// </summary>
    /// <value>
    /// The multipage limit.
    /// </value>
    public int MultipageLimit { get; set; }

    /// <summary>
    /// Creates the default HTTP options.
    /// </summary>
    /// <returns></returns>
    public static HttpOptions CreateDefault()
    {
      return new HttpOptions()
      {
        UserAgentMobile = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; ARM; Mobile; Touch{0}) like Gecko",
        UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64{0}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1736.2 Safari/537.36 OPR/20.0.1380.1",
        UseMobileUserAgent = false,
        RequestTimeout = null,
        CustomHttpHandler = null,
        MultipageLimit = 10
      };
    }
  }
}
