
namespace ReadSharp
{
  public class ReadOptions
  {
    /// <summary>
    /// Are header tags and Doctype returned
    /// </summary>
    /// <value>
    ///   <c>true</c> if [has only body]; otherwise, <c>false</c>.
    /// </value>
    public bool HasHeaderTags { get; set; }

    /// <summary>
    /// Is no headline (h1) is included in generated HTML
    /// </summary>
    /// <value>
    ///   <c>true</c> if [has no headline]; otherwise, <c>false</c>.
    /// </value>
    public bool HasNoHeadline { get; set; }

    /// <summary>
    /// Are deep links with hashes not transformed to absolute URIs
    /// </summary>
    /// <value>
    ///   <c>true</c> if [use deep links]; otherwise, <c>false</c>.
    /// </value>
    public bool UseDeepLinks { get; set; }

    /// <summary>
    /// Creates the default options.
    /// </summary>
    /// <returns></returns>
    public static ReadOptions CreateDefault()
    {
      return new ReadOptions()
      {
        HasHeaderTags = false,
        HasNoHeadline = false,
        UseDeepLinks = false
      };
    }
  }
}
