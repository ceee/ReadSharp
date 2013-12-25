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
    /// <param name="options">The transform options.</param>
    /// <returns>
    /// An article with extracted content and meta information.
    /// </returns>
    /// <exception cref="ReadException"></exception>
    Task<Article> Read(Uri uri, ReadOptions options = null);
  }
}