
namespace ReadSharp
{
  public class ReadOptions
  {
    /// <summary>
    /// Are header tags and Doctype returned (default: false).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [has only body]; otherwise, <c>false</c>.
    /// </value>
    public bool HasHeaderTags { get; set; }

    /// <summary>
    /// Is no headline (h1) is included in generated HTML (default: false).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [has no headline]; otherwise, <c>false</c>.
    /// </value>
    public bool HasHeadline { get; set; }

    /// <summary>
    /// Are deep links with hashes not transformed to absolute URIs (default: false).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [use deep links]; otherwise, <c>false</c>.
    /// </value>
    public bool UseDeepLinks { get; set; }

    /// <summary>
    /// Determines whether the output will be formatted (default: false).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [pretty print]; otherwise, <c>false</c>.
    /// </value>
    public bool PrettyPrint { get; set; }

    /// <summary>
    /// Determines whether to prefer the encoding found in the HTML or the one found in the HTTP Header (default: true).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [prefer HTML encoding]; otherwise, <c>false</c>.
    /// </value>
    public bool PreferHTMLEncoding { get; set; }

    /// <summary>
    /// Download all pages for articles with multiple pages (default: false).
    /// </summary>
    /// <value>
    ///   <c>true</c> if [multipage download]; otherwise, <c>false</c>.
    /// </value>
    public bool MultipageDownload { get; set; }

    /// <summary>
    /// If true, replace all img-tags with placeholders.
    /// </summary>
    public bool ReplaceImagesWithPlaceholders { get; set; }

    /// <summary>
    /// Creates the default options.
    /// </summary>
    /// <returns></returns>
    public static ReadOptions CreateDefault()
    {
      return new ReadOptions()
      {
        HasHeaderTags = false,
        HasHeadline = false,
        UseDeepLinks = false,
        PrettyPrint = false,
        PreferHTMLEncoding = true,
        MultipageDownload = false,
        ReplaceImagesWithPlaceholders = false
      };
    }
  }
}
