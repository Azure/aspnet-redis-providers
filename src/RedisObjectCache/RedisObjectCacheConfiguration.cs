using System.Configuration;
using System.Linq;
using System.Xml;

namespace Microsoft.Web.Redis
{
	// Use the following web.config file.
	//<?xml version="1.0" encoding="utf-8" ?>
	//<configuration>
	//  <configSections>
	//    <section name="redisObjectCache" type="Microsoft.Web.Redis.RedisObjectCacheConfiguration, Microsoft.Caching.RedisObjectCache" />
	//  </configSections>
	//  <redisObjectCache>
	//		<caches>
	//			<add name = "Default" [String]
	//				  host = "127.0.0.1" [String]
	//			      port = "" [number]
	//			      accessKey = "" [String]
	//			      ssl = "false" [true|false]
	//				  throwOnError = "true" [true|false]
	//			      databaseId = "0" [number]
	//				  applicationName = "" [String]
	//			      connectionTimeoutInMilliseconds = "5000" [number]
	//			      operationTimeoutInMilliseconds = "5000" [number] />
	//		</caches>
	//  </redisObjectCache>
	//</configuration>
	public sealed class RedisObjectCacheConfiguration : ConfigurationSection
	{
		// Properties
		public static RedisObjectCacheConfiguration Instance
		{
			get { return (RedisObjectCacheConfiguration)ConfigurationManager.GetSection("redisObjectCache"); }
		}

		protected override void DeserializeSection(XmlReader reader)
		{
			// ProviderSettingsCollection and ProviderSettings needs to have a type-attribute on the add-element
			// for the cache configuration type is not needed. To prevent the user from having to add an empty
			// type-attribute, this piece of code just adds an empty attribute.
			reader.Read();
			string xml = reader.ReadOuterXml();
			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			foreach (XmlElement childNode in document.DocumentElement.SelectNodes("//add[not(@type)]").OfType<XmlElement>())
				childNode.SetAttribute("type", string.Empty);

			using (XmlReader innerReader = new XmlNodeReader(document))
				base.DeserializeSection(innerReader);
		}

		[ConfigurationProperty("caches", IsRequired = true)]
		public ProviderSettingsCollection Caches
		{
			get { return (ProviderSettingsCollection)this["caches"]; }
		}
	}
}