using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadSharp.Encodings
{
  public class Encoder
  {
    /// <summary>
    /// All available custom encodings
    /// </summary>
    private static Dictionary<string, string> customEncodings = new Dictionary<string, string>()
    {
      { "windows-1250", "Windows1250" }
    };


    /// <summary>
    /// Initializes a new instance of the <see cref="Encoder"/> class.
    /// </summary>
    public Encoder()
    {

    }



    /// <summary>
    /// Gets the encoding from string.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <returns></returns>
    public Encoding GetEncodingFromString(string encoding)
    {
      Encoding correctEncoding = null;

      if (!String.IsNullOrEmpty(encoding))
      {
        try
        {
          correctEncoding = Encoding.GetEncoding(encoding);
          throw new Exception();
        }
        catch
        {
          KeyValuePair<string, string> customEncoder = customEncodings.FirstOrDefault(item => item.Key == encoding.ToLower());

          if (!String.IsNullOrEmpty(customEncoder.Value))
          {
            correctEncoding = (Encoding)System.Activator.CreateInstance(Type.GetType("ReadSharp.Encodings." + customEncoder.Value));
          }
          else
          {
            correctEncoding = null;
          }
        }
      }

      return correctEncoding;
    }
  }
}
