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
        // try get encoding from string
        try
        {
          correctEncoding = Encoding.GetEncoding(encoding);
        }
        catch (ArgumentException)
        {
          // encoding not found in environment
          // handled in finally block as it could also generate null without throwing exceptions
        }
        finally
        {
          // use a custom encoder
          if (correctEncoding == null)
          {
            try
            {
              KeyValuePair<string, string> customEncoder = customEncodings.FirstOrDefault(item => item.Key == encoding.ToLower());

              if (!String.IsNullOrEmpty(customEncoder.Value))
              {
                Type encoderType = Type.GetType("ReadSharp.Encodings." + customEncoder.Value);

                if (encoderType != null)
                {
                  correctEncoding = (Encoding)System.Activator.CreateInstance(Type.GetType("ReadSharp.Encodings." + customEncoder.Value));
                }
              }
            }
            catch (Exception)
            {
              // couldn't create instance for whatever reason.
              // nothing to do here, as the default encoder will be used in the program.
            }
          }
        }
      }

      return correctEncoding;
    }
  }
}
