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
      { "windows-1250", "Windows1250" },
      { "windows-1251", "Windows1251" },
      { "windows-1252", "Windows1252" },
      { "windows-1253", "Windows1253" },
      { "windows-1254", "Windows1254" },
      { "windows-1255", "Windows1255" },
      { "windows-1256", "Windows1256" },
      { "windows-1257", "Windows1257" },
      { "windows-1258", "Windows1258" },
      { "iso-8859-1", "Iso88591" },
      { "iso-8859-2", "Iso88592" },
      { "iso-8859-3", "Iso88593" },
      { "iso-8859-4", "Iso88594" },
      { "iso-8859-5", "Iso88595" },
      { "iso-8859-6", "Iso88596" },
      { "iso-8859-7", "Iso88597" },
      { "iso-8859-8", "Iso88598" },
      { "iso-8859-9", "Iso88599" },
      { "iso-8859-13", "Iso885913" },
      { "iso-8859-15", "Iso885915" }
    };


    /// <summary>
    /// try custom encoder
    /// </summary>
    private bool tryCustomEncoder;


    /// <summary>
    /// Initializes a new instance of the <see cref="Encoder"/> class.
    /// </summary>
    public Encoder(bool tryCustomEncoder = false)
    {
      this.tryCustomEncoder = tryCustomEncoder;
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
          if (tryCustomEncoder && correctEncoding == null)
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
