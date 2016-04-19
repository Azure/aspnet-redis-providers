//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
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
            Assert.Contains("The specified class 'DummyClass' could not be loaded", ex.Message);
        }

        [Fact]
        public void EnableLoggingIfParametersAvailable_ClassNameButNotAssemblyQualified()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(loggingClassName, "Microsoft.Web.Redis.Tests.ProviderConfigurationTests");
            config.Add(loggingMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.EnableLoggingIfParametersAvailable(config));
            Assert.Contains("DummyMethodName", ex.Message);
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

        [Fact]
        public void UseConnectionStringByName()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionString", "RedisSession");
            Assert.Equal(ConfigurationManager.ConnectionStrings["RedisSession"].ConnectionString,
                ProviderConfiguration.ProviderConfigurationForSessionState(config).ConnectionString);
        }

        private string settingsClassName = "settingsClassName";
        private string settingsMethodName = "settingsMethodName";

        [Fact]
        public void GetConnectionString_EitherOfTwoSettings()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionString", "DummyString");
            config.Add(settingsClassName, "DummyClass");
            config.Add(settingsMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<ConfigurationErrorsException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("Either use the combination of parameters \"settingsClassName\" and \"settingsMethodName\" to provide the value of connection string or use the parameter \"connectionString\" but not both.", ex.Message);

            NameValueCollection config2 = new NameValueCollection();
            config2.Add("connectionString", "DummyString");
            config2.Add(settingsClassName, "DummyClass");

            Exception ex2 = Assert.Throws<ConfigurationErrorsException>(() => ProviderConfiguration.GetConnectionString(config2));
            Assert.Contains("Either use the combination of parameters \"settingsClassName\" and \"settingsMethodName\" to provide the value of connection string or use the parameter \"connectionString\" but not both.", ex2.Message);
            
            NameValueCollection config3 = new NameValueCollection();
            config3.Add("connectionString", "DummyString");
            config3.Add(settingsMethodName, "DummyMethodName");

            Exception ex3 = Assert.Throws<ConfigurationErrorsException>(() => ProviderConfiguration.GetConnectionString(config3));
            Assert.Contains("Either use the combination of parameters \"settingsClassName\" and \"settingsMethodName\" to provide the value of connection string or use the parameter \"connectionString\" but not both.", ex3.Message);
        }

        [Fact]
        public void GetConnectionString_WrongClassName()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, "DummyClass");
            config.Add(settingsMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<TypeLoadException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("The specified class 'DummyClass' could not be loaded", ex.Message);
        }

        [Fact]
        public void GetConnectionString_ClassNameButNotAssemblyQualified()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, "Microsoft.Web.Redis.Tests.ProviderConfigurationTests");
            config.Add(settingsMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("DummyMethodName", ex.Message);
        }

        [Fact]
        public void GetConnectionString_WrongMethodName()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "DummyMethodName");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("DummyMethodName", ex.Message);
        }

        [Fact]
        public void GetConnectionString_InternalMethod()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "GetSettingsInternal");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("GetSettingsInternal", ex.Message);
        }

        [Fact]
        public void GetConnectionString_MethodWithParam()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "GetSettingsWithParam");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("GetSettingsWithParam", ex.Message);
        }

        [Fact]
        public void GetConnectionString_NonStaticMethod()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "GetSettingsNonStatic");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("GetSettingsNonStatic", ex.Message);
        }

        [Fact]
        public void GetConnectionString_MethodWithIntReturn()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "GetSettingsWithIntReturn");

            Exception ex = Assert.Throws<MissingMethodException>(() => ProviderConfiguration.GetConnectionString(config));
            Assert.Contains("GetSettingsWithIntReturn", ex.Message);
        }

        [Fact]
        public void GetConnectionString_Valid()
        {
            NameValueCollection config = new NameValueCollection();
            config.Add(settingsClassName, typeof(SettingsProvider).AssemblyQualifiedName);
            config.Add(settingsMethodName, "GetSettings");
            Assert.Equal("localhost:6380", ProviderConfiguration.GetConnectionString(config));
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

    public class SettingsProvider
    {
        internal static string GetSettingsInternal()
        {
            return "localhost:6380";
        }

        public static string GetSettingsWithParam(int param)
        {
            return "localhost:6380";
        }

        public string GetSettingsNonStatic()
        {
            return "localhost:6380";
        }

        public static int GetSettingsWithIntReturn()
        {
            return 0;
        }

        public static string GetSettings()
        {
            return "localhost:6380";
        }
    }
}
