namespace KriaSoft.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class XpoReader : IDisposable
    {
        private Stream stream;
        private bool leaveOpen;

        public XpoReader(Stream stream, bool leaveOpen = false)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;
        }

        public XpoReader(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.SequentialScan))
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
