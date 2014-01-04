using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ReadSharp.Ports.NReadability
{
  public class MetaExtractor
  {
    /// <summary>
    /// Gets or sets a value indicating whether [has value].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [has value]; otherwise, <c>false</c>.
    /// </value>
    public bool HasValue { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    /// <value>
    /// The tags.
    /// </value>
    public IEnumerable<XElement> Tags { get; private set; }



    /// <summary>
    /// Initializes a new instance of the <see cref="MetaExtractor"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public MetaExtractor(XDocument document)
    {
      var documentRoot = document.Root;

      if (documentRoot == null || documentRoot.Name == null || !"html".Equals(documentRoot.Name.LocalName, StringComparison.OrdinalIgnoreCase))
      {
        HasValue = false;
        return;
      }

      var headElement = documentRoot.GetChildrenByTagName("head").FirstOrDefault();

      if (headElement == null)
      {
        HasValue = false;
        return;
      }

      IEnumerable<XElement> meta = headElement.GetChildrenByTagName("meta");
      IEnumerable<XElement> link = headElement.GetChildrenByTagName("link");

      Tags = meta != null ? meta.Concat(link) : link;
      HasValue = Tags != null && Tags.Count() > 0;
    }


    /// <summary>
    /// Gets the meta description.
    /// </summary>
    /// <returns></returns>
    public string GetMetaDescription()
    {
      return SearchCandidates(new Dictionary<string, string>()
      {
        { "property|og:description", "content" },
        { "name|description", "content" }
      });
    }


    /// <summary>
    /// Gets the meta image.
    /// </summary>
    /// <returns></returns>
    public string GetMetaImage()
    {
      return SearchCandidates(new Dictionary<string, string>()
      {
        { "property|og:image", "content" },
        { "rel|apple-touch-icon", "href" },
        { "rel|apple-touch-icon-precomposed", "href"},
        { "name|msapplication-square310x310logo", "content" },
        { "name|msapplication-square150x150logo", "content" },
        { "name|msapplication-square70x70logo", "content" },
        { "name|msapplication-TileImage", "content" },
        { "rel|image_src", "href" }
      });
    }


    /// <summary>
    /// Gets the meta favicon.
    /// </summary>
    /// <returns></returns>
    public string GetMetaFavicon()
    {
      return SearchCandidates(new Dictionary<string, string>()
      {
        { "rel|icon", "href" },
        { "rel|shortcut icon", "href" }
      });
    }


    /// <summary>
    /// Gets the charset.
    /// </summary>
    /// <returns></returns>
    public string GetCharset()
    {
      // find: <meta charset="utf-8">
      string result = SearchCandidates(new Dictionary<string, string>()
      {
        { "charset", "charset" }
      }, true);

      if (String.IsNullOrEmpty(result))
      {
        // find: <meta http-equiv="Content-Type" content="text/html; charset=windows-1250">
        result = SearchCandidates(new Dictionary<string, string>()
        {
          { "http-equiv|Content-Type", "content" }
        });

        if (!String.IsNullOrEmpty(result))
        {
          int charsetStart = result.IndexOf("charset=");
          if (charsetStart > 0)
          {
            charsetStart += 8;
            result = result.Substring(charsetStart, result.Length - charsetStart);
          }
        }
      }

      return String.IsNullOrEmpty(result) ? null : result.ToUpper();
    }


    /// <summary>
    /// Searches the candidates.
    /// </summary>
    /// <param name="candidates">The candidates.</param>
    /// <returns></returns>
    private string SearchCandidates(Dictionary<string, string> candidates, bool simple = false)
    {
      string result = null;

      foreach (var candidate in candidates)
      {
        XElement element;
        string[] type = candidate.Key.Split('|');

        if (simple)
        {
          element = Tags
            .Where(item => item.GetAttributeValue(type[0], null) != null)
            .FirstOrDefault();
        }
        else
        {
          element = Tags
            .Where(item => String.Equals(item.GetAttributeValue(type[0], null), type[1], StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();
        }

        if (element != null)
        {
          result = element.GetAttributeValue(candidate.Value, "");
        }

        if (result != null && result.Length > 1)
        {
          break;
        }
      }

      return result;
    }
  }
}
