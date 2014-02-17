using ReadSharp.Ports.NReadability;
using System.Text;

namespace ReadSharp
{
  internal class Response
  {
    public TranscodingResult TranscodingResult { get; set; }

    public Encoding Encoding { get; set; }

    public int PageCount { get; set; }
  }
}
