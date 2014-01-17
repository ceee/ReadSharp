using System;

namespace ReadSharp
{
  /// <summary>
  /// Article image
  /// </summary>
  public class ArticleImage
  {
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public string ID { get; set; }

    /// <summary>
    /// Gets or sets the URI.
    /// </summary>
    /// <value>
    /// The URI.
    /// </value>
    public Uri Uri { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the alternative text.
    /// </summary>
    /// <value>
    /// The alternative text.
    /// </value>
    public string AlternativeText { get; set; }

    /// <summary>
    /// Gets a value indicating whether [is valid URI].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [is valid URI]; otherwise, <c>false</c>.
    /// </value>
    public bool IsValidUri
    {
      get { return Uri != null && Uri.IsWellFormedUriString(Uri.ToString(), UriKind.Absolute); }
    }
  }
}
