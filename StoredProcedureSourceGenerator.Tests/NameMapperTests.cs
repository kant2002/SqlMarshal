// -----------------------------------------------------------------------
// <copyright file="NameMapperTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace StoredProcedureSourceGenerator.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameMapperTests
    {
        [DataTestMethod]
        [DataRow("personId", "person_id")]
        public void MyTestMethod(string parameterName, string expectedStoredProcedureParameter)
        {
            var storedProcedureParameter = NameMapper.MapName(parameterName);

            Assert.AreEqual(expectedStoredProcedureParameter, storedProcedureParameter);
        }
    }
}
