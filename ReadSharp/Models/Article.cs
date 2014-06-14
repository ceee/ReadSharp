using System;
using System.Collections.Generic;
using System.Text;

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
    /// Could the parser extract any contents?
    /// </summary>
    /// <value>
    ///   <c>true</c> if [content extracted]; otherwise, <c>false</c>.
    /// </value>
    public bool ContentExtracted { get; set; }

    /// <summary>
    /// Gets or sets the raw HTML.
    /// </summary>
    /// <value>
    /// The raw HTML.
    /// </value>
    public string Raw { get; set; }

    /// <summary>
    /// Plain content without HTML tags.
    /// </summary>
    /// <value>
    ///   The plain content.
    /// </value>
    public string PlainContent { get; set; }

    /// <summary>
    /// Gets or sets the word count (based on the PlainContent).
    /// </summary>
    /// <value>
    /// The word count.
    /// </value>
    public int WordCount { get; set; }

    /// <summary>
    /// Gets or sets the page count.
    /// </summary>
    /// <value>
    /// The page count.
    /// </value>
    public int PageCount { get; set; }

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
    public IEnumerable<ArticleImage> Images { get; set; }

    /// <summary>
    /// Gets or sets the next page URL.
    /// </summary>
    /// <value>
    /// The next page URL.
    /// </value>
    public Uri NextPage { get; set; }

    /// <summary>
    /// Gets or sets the encoding of the article.
    /// </summary>
    /// <value>
    /// The encoding.
    /// </value>
    public Encoding Encoding { get; set; }
  }
}
