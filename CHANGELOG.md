### 6.2.2

- Option (`PreferHTMLEncoding`) to either prefer HTML or HTTP encoding for generating content

### 6.2.0

- Option to replace images with placeholders

### 6.1.0

- Add RAW HTML content to Article

### 6.0.0

- Support for Universal apps (dropped SL and WP7 support)

### 5.0.0

- HttpOptions for better control over the request
- More reliable scraping of images
- Remove unnecessary attributes from tags
- Allow parsing of multi-page articles

### 4.2.3

- add PrettyPrint option

### 4.2.2

- use encoding found in HTTP headers in first iteration (fixes [issue #6](https://github.com/ceee/ReadSharp/issues/6))

### 4.2.1

- fixes [issue #3](https://github.com/ceee/ReadSharp/issues/3)

### 4.2.0

- use custom encoders if not supported on platform (implemented for ISO-8859 and Windows range).

### 4.1.0

- extract description, favicon and front image from meta tags
- correct encoding - retry reading stream with charset from HTML headers, if not available in HTTP headers or not matching (fixes #1)

### 4.0.0 

- migrate PocketSharp.Reader to ReadSharp
