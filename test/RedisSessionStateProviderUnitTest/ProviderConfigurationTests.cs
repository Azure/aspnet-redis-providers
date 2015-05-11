//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class ProviderConfigurationTests
    {
        private string loggingClassName = "loggingClassName";
        private string loggingMethodName = "loggingMethodName";

        [Fact]
        public void EnableLoggingIfParametersAvailable_WrongClassName()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, "DummyClass");
            config.Add(loggingMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<TypeLoadException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("Could not load type 'DummyClass'", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_ClassNameButNotAssemblyQualified()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, "Microsoft.Web.Redis.Tests.ProviderConfigurationTests");
            config.Add(loggingMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<TypeLoadException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("Could not load type 'Microsoft.Web.Redis.Tests.ProviderConfigurationTests'", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_WrongMethodName()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("DummyMethodName", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_InternalMethod()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriterInternal");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("GetTextWriterInternal", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_MethodWithParam()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriterWithParam");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("GetTextWriterWithParam", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_NonStaticMethod()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriterNonStatic");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("GetTextWriterNonStatic", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_MethodWithIntReturn()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriterWithIntReturn");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("GetTextWriterWithIntReturn", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_Valid()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriter");
            ProviderConfiguration.EnableLoggingIfParametersAvailable(config);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_ValidWithStreamWriterReturn()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, typeof(Logger).AssemblyQualifiedName);
            config.Add(loggingMethodName, "GetTextWriterWithStreamWriterReturn");
            ProviderConfiguration.EnableLoggingIfParametersAvailable(config);
        }
    }

    public class Logger
    {
        internal static TextWriter GetTextWriterInternal()
        {
            return new StreamWriter(@"GetTextWriterInternal.txt");
        }

        public static TextWriter GetTextWriterWithParam(int param)
        {
            return new StreamWriter(@"GetTextWriterWithParam.txt");
        }

        public TextWriter GetTextWriterNonStatic()
        {
            return new StreamWriter(@"GetTextWriterNonStatic.txt");
        }

        public static int GetTextWriterWithIntReturn()
        {
            return 0;
        }
        
        public static TextWriter GetTextWriter()
        {
            return new StreamWriter(@"GetTextWriter.txt");
        }

        public static StreamWriter GetTextWriterWithStreamWriterReturn()
        {
            return new StreamWriter(@"GetTextWriterWithStreamWriterReturn.txt");
        }
    }
}
