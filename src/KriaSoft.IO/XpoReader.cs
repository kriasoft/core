//-----------------------------------------------------------------------
// <copyright file="XpoReader.cs" company="KriaSoft LLC">
//     Copyright (c) KriaSoft LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace KriaSoft.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Omega Research Export Format (.xpo) file reader.
    /// </summary>
    public class XpoReader : IDisposable
    {
        // TODO: Compete this list
        public enum Exchange : byte
        {
            AMEX    = 0x00,
            BOSTON  = 0x01,
            CBOE    = 0x02,
            CBOT    = 0x03,
            CEC     = 0x04,
            CME     = 0x05,
            KCBOT   = 0x06,
            COMEX   = 0x07,
            CSC     = 0x08,
            NYCE    = 0x09,
            NYMEX   = 0x0A,
            MIDWES  = 0x0B,
            ME      = 0x0C,
            MGE     = 0x0D,
            MIDAM   = 0x0E,
            NASDAQ  = 0x0F,
            PACIFI  = 0x10,
            NYFE    = 0x11,
            NYSE    = 0x12,
            OPRA    = 0x13,
            OTCBB   = 0x14,
            PBT     = 0x15,
            PSE     = 0x16,
            LINC    = 0x17,
            SPECI   = 0x18,
            WCE     = 0x19,
            UNDEF   = 0x1A,
            SICOVA  = 0x1B,
            MONEP   = 0x1C,
            BFE     = 0x1D,
            // ...
            FOREX   = 0x6D
        }

        // TODO: Complete this list
        public enum Interval : byte
        {
            Daily        = 0x12,
            Intraday1Min = 0x14
        }

        public struct Symbol
        {
            public string Ticker;

            public XpoReader.Exchange Exchange;

            public XpoReader.Interval Interval;

            public int Num1;

            public int Num2;

            public Symbol(string ticker, XpoReader.Exchange exchange, XpoReader.Interval interval, int num1, int num2)
            {
                this.Ticker = ticker;
                this.Exchange = exchange;
                this.Interval = interval;
                this.Num1 = num1;
                this.Num2 = num2;
            }
        }

        private Stream stream;

        private bool leaveOpen;

        public XpoReader(Stream stream, bool leaveOpen = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
            this.leaveOpen = leaveOpen;
        }

        public XpoReader(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.SequentialScan))
        {
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            // Read 10-bytes gzip header
            var buffer = new byte[10];
            
            if (this.stream.Read(buffer, 0, buffer.Length) != buffer.Length || !buffer.SequenceEqual(new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B }))
            {
                throw new InvalidDataException("Gzip header is missing.");
            }

            // TODO: Find the meaning of the 11-14 bytes (ex.: AA 04 00 00). Skipping them for now.
            this.stream.Position += 4;

            buffer = new byte[29];

            if (this.stream.Read(buffer, 0, buffer.Length) != buffer.Length || Encoding.ASCII.GetString(buffer) != "OmEgArEaSeArChExPoRtFoRmAt5.1")
            {
                throw new InvalidDataException("Omega Research file format header is missing.");
            }

            var symbolsCount = Convert.ToInt16(this.stream.ReadByte());

            // TODO: Find the meaning of the next 4 bytes (ex.: 00 00 00 32, 00 00 00 22). Skipping them for now.
            this.stream.Position += 4;

            // Iterate over the list of symbols
            for (var i = 0; i < symbolsCount; i++)
            {
                // TODO: Find the meaning of the next 7 bytes (ex.: 00 00 00 0F 00 00 0C, 00 00 12 00 00 0C). Skipping them for now.
                this.stream.Position += 7;

                var tickerLength = Convert.ToInt16(this.stream.ReadByte());

                buffer = new byte[tickerLength - 1];

                this.stream.Position += 1; // It's 0x00
                this.stream.Read(buffer, 0, buffer.Length);
                this.stream.Position += 1; // It's 0.00
                var ticker = Encoding.ASCII.GetString(buffer);
                var exchangeCode = (byte)this.stream.ReadByte();
                
                if (!Enum.IsDefined(typeof(XpoReader.Exchange), exchangeCode))
                {
                    throw new InvalidOperationException(String.Format("Unknown exchange code '{0}' for the symbol '{1}'.", exchangeCode, ticker));
                }

                var exchange = (XpoReader.Exchange)exchangeCode;

                // Iterate over the list of periods
                while (true)
                {
                    // TODO: Find meanings of the following bytes
                    var b0 = (byte)this.stream.ReadByte(); // 0x00 or 0x01.
                    var b1 = (byte)this.stream.ReadByte(); // 0x00
                    var b2 = (byte)this.stream.ReadByte(); // 0xC8
                    var b3 = (byte)this.stream.ReadByte();
                    var b4 = (byte)this.stream.ReadByte(); // Interval code
                    var b5 = (byte)this.stream.ReadByte();

                    if ((b3 == 0 || b3 == 2) && b4 == 0 && b5 == 0)
                    {
                        this.stream.Position -= 3;
                        break;
                    }
                    
                    if (!Enum.IsDefined(typeof(XpoReader.Interval), b4))
                    {
                        throw new InvalidOperationException(String.Format("Unknown interval code '{0}' for the symbol '{1}'.", b4, ticker));
                    }

                    var interval = (XpoReader.Interval)b4;

                    // TODO: Find the meaning of the following 5 bytes. Ex.: 00 50 C3 00 00
                    this.stream.Position += 4;

                    buffer = new byte[4];

                    this.stream.Read(buffer, 0, buffer.Length);
                    var num1 = BitConverter.ToInt32(buffer, 0); // TODO: Find the meaning
                    this.stream.Position += 2;
                    this.stream.Read(buffer, 0, buffer.Length); // TODO: Find the meaning
                    var num2 = BitConverter.ToInt32(buffer, 0);

                    yield return new Symbol(ticker, exchange, interval, num1, num2);
    
                }
            }
        }        

        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.stream.Dispose();
            }
        }
    }
}
