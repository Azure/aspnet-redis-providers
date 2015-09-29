//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SessionStateProvider")]
[assembly: AssemblyCopyright("Copyright ©  2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyMetadata("Serviceable", "True")]

#if !CODESIGNING
[assembly: InternalsVisibleTo("Microsoft.Web.RedisSessionStateProvider.Unit.Tests")]
[assembly: InternalsVisibleTo("Microsoft.Web.RedisSessionStateProvider.Functional.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
#if !NOCOMMONASSEMBLYVERSION
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
#endif
[assembly: AssemblyTitle("Cache Providers")]

namespace System.Reflection
{
    /// <summary>
    /// Provided as a down-level stub for the 4.5 AssemblyMetaDataAttribute class.
    /// All released assemblies should define [AssemblyMetadata("Serviceable", "True")].
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
