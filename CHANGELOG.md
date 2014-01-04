### 4.2.1

- fixes [issue #3](https://github.com/ceee/ReadSharp/issues/3)

### 4.2.0

- use custom encoders if not supported on platform (implemented for ISO-8859 and Windows range).

### 4.1.0

- extract description, favicon and front image from meta tags
- correct encoding - retry reading stream with charset from HTML headers, if not available in HTTP headers or not matching (fixes #1)

### 4.0.0 

- migrate PocketSharp.Reader to ReadSharp
