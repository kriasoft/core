//-----------------------------------------------------------------------
// <copyright file="XpoReaderTests.cs" company="KriaSoft LLC">
//     Copyright (c) KriaSoft LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace KriaSoft.IO.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class XpoReaderTests
    {
        [TestMethod]
        public void GetSymbols_Retuns_a_List_of_Symbols()
        {
            // Arrange
            using (var xpo = new XpoReader("test-data-020112.xpo"))
            {
                // Act
                var symbols = xpo.GetSymbols().ToArray();

                // Assert
                Assert.IsTrue(symbols.Length == 36);
            }
        }
    }
}
