![ReadSharp](https://raw.github.com/ceee/ReadSharp/master/Assets/github-header.png)

ReadSharp was previously **PocketSharp.Reader** and is now hosted without the [PocketSharp](https://github.com/ceee/PocketSharp) dependency.

## Install ReadSharp using [NuGet](https://www.nuget.org/packages/ReadSharp/)

```
Install-Package ReadSharp
```


## What's it all about?

The library extracts the main content of a website and returns the article as HTML with it's associated title, description, favicon and all included images.

The content can be encapsulated in a `<body>`-Tag and displayed as a readable website with a custom CSS (it's up to you!).

ReadSharp is based on a custom PCL port of NReadability and SgmlReader, which are included in the solution.

#### Association with Pocket

This library is a replacement for the Article View API by Pocket which is  limited by usage and privacy.

With ReadSharp you won't hit any usage limits, as you are extracting the content directly. And it's open source.

---

## Example

```csharp
using ReadSharp;

Reader reader = new Reader();
Article article;

try
{
  article = await reader.Read(new Uri("http://frontendplay.com/story/4/http-caching-demystified-part-2-implementation"));
}
catch (ReadException exc)
{
  // handle exception
}
```

## Options

You can pass `HttpOptions` to the `Reader` constructor, which count for all requests:

- `HttpMessageHandler` **CustomHttpHandler**<br>Use your own HTTP handler
- `int?` **RequestTimeout**<br>Define a custom timeout _in seconds_, after which requests should cancel
- `string` **UserAgent**<br>Override the user agent, which is passed to the destination server
- `bool` **UseMobileUserAgent**<br>There are desktop and mobile default user agents. By enabling this property, the mobile user agent is used. _If you pass a custom user agent, this property is ignored!_

## Article Model

The `Article` contains following fields:

- `string` **Title** (the title of the page)
- `string` **Description** (description of the page, extracted from meta information)
- `string` **Content** (contains the article)
- `Uri` **FrontImage** (main page image extracted from meta tags like apple-touch-icon and others)
- `Uri` **Favicon** (the favicon of the page)
- `List<ArticleImage>` **Images** (contains all images found in the text)
- `string` **NextPage** (contains the next page URI, if available)

### Article Image

- `Uri` **Uri**
- `string` **Title** (extracted from the title attribute)
- `string` **AlternativeText** (extracted from the alt attribute)

## Supported platforms

ReadSharp is a **Portable Class Library**, therefore it's compatible with multiple platforms:

- **.NET** >= 4.0.3 (including WPF)
- **Silverlight** >= 4
- **Windows Phone** >= 7.5
- **Windows Store**

## Forked Dependencies

_forks are included in the primary source code_

- [NReadability](https://github.com/marek-stoj/NReadability)
- [SgmlReader](https://github.com/MindTouch/SGMLReader)

## Contributors
| [![ceee](http://gravatar.com/avatar/9c61b1f4307425f12f05d3adb930ba66?s=70)](https://github.com/ceee "Tobias Klika") |
|---|
| [ceee](https://github.com/ceee) |

## License

[MIT License](https://github.com/ceee/ReadSharp/blob/master/LICENSE-MIT)