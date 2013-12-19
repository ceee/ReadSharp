using System;
using System.Collections.Generic;

namespace ReadSharp
{
  /// <summary>
  /// Readable article
  /// </summary>
  public class Article
  {
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>
    /// The content.
    /// </value>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the front image.
    /// </summary>
    /// <value>
    /// The front image.
    /// </value>
    public Uri FrontImage { get; set; }

    /// <summary>
    /// Gets or sets the favicon.
    /// </summary>
    /// <value>
    /// The favicon.
    /// </value>
    public Uri Favicon { get; set; }

    /// <summary>
    /// Gets or sets the images.
    /// </summary>
    /// <value>
    /// The images.
    /// </value>
    public List<ArticleImage> Images { get; set; }

    /// <summary>
    /// Gets or sets the next page URL.
    /// </summary>
    /// <value>
    /// The next page URL.
    /// </value>
    public Uri NextPage { get; set; }
  }
}
