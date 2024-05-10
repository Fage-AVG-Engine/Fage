namespace Fage.Runtime;

/// <summary>
/// 表示配置文件中的存在的错误
/// </summary>
[Serializable]
public class InvalidConfigurationException : Exception
{
	public InvalidConfigurationException(string section) : base($"配置文件中，节{section}存在错误。") { }
	public InvalidConfigurationException(string section, Exception inner) : base($"配置文件中，节{section}存在错误。", inner) { }
	public InvalidConfigurationException(string section, string message) : base($"配置文件中，节{section}存在错误，{message}") { }
	public InvalidConfigurationException(string section, string message, Exception inner) : base($"配置文件中，节{section}存在错误，{message}", inner) { }
	
	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.",
		DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	protected InvalidConfigurationException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
