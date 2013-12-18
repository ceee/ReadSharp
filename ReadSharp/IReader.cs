using System;
using System.Threading.Tasks;

namespace ReadSharp
{
  public interface IReader
  {
    /// <summary>
    /// Reads article content from the given URI.
    /// </summary>
    /// <param name="uri">An URI to extract the content from.</param>
    /// <param name="bodyOnly">if set to <c>true</c> [only body is returned].</param>
    /// <param name="noHeadline">if set to <c>true</c> [no headline (h1) is included].</param>
    /// <returns>
    /// An article with extracted content and meta information.
    /// </returns>
    /// <exception cref="Exception"></exception>
    Task<Article> Read(Uri uri, bool bodyOnly = true, bool noHeadline = false);
  }
}