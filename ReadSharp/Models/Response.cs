using System.IO;

namespace ReadSharp
{
  internal class Response
  {
    public Stream Stream { get; set; }

    public string Charset { get; set; }
  }
}
