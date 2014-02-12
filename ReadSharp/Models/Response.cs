using System.IO;
using System.Net.Http;

namespace ReadSharp
{
  internal class Response
  {
    public HttpResponseMessage RawResponse { get; set; }

    public Stream Stream { get; set; }

    public string Charset { get; set; }
  }
}
