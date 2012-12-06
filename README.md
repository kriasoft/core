## KriaSoft Core Library

This is a collection of helper classes and utilities released under Apache License 2.0.

### Usage samples

Reading .xpo files (Omega Research Export File Type)

```csharp
using KriaSoft.IO;

using (var xpo = new XpoReader("quotes.xpo"))
{
    var symbols = xpo.GetSymbols();
}
```