using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class DynamicAnimationInterfaceGenerator
{
	public static void WriteGeneratedAccessors(IEnumerable<DynamicAnimationDefinition> definitions, string outputPath)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
		File.WriteAllText(outputPath, GenerateSource(definitions));
	}

	public static string GenerateSource(IEnumerable<DynamicAnimationDefinition> definitions)
	{
		StringBuilder source = new StringBuilder();
		source.AppendLine("using Godot;");
		source.AppendLine("using System;");
		source.AppendLine();
		source.AppendLine("public static class DynamicAnimationGeneratedAccessors");
		source.AppendLine("{");
		source.AppendLine("\tstatic bool registered;");
		source.AppendLine();
		source.AppendLine("\tpublic static void RegisterAll()");
		source.AppendLine("\t{");
		source.AppendLine("\t\tif (registered)");
		source.AppendLine("\t\t{");
		source.AppendLine("\t\t\treturn;");
		source.AppendLine("\t\t}");
		source.AppendLine();

		List<IGrouping<string, DynamicAnimationPropertyRequirement>> groups = definitions
			.SelectMany(definition => definition.PropertyRequirements)
			.GroupBy(requirement => requirement.InterfaceName)
			.ToList();

		foreach (IGrouping<string, DynamicAnimationPropertyRequirement> group in groups)
		{
			source.AppendLine("\t\tDynamicAnimationPropertyAccessorRegistry.Register(new " + CreateAccessorName(group.Key) + "());");
		}

		source.AppendLine("\t\tregistered = true;");
		source.AppendLine("\t}");
		source.AppendLine("}");

		foreach (IGrouping<string, DynamicAnimationPropertyRequirement> group in groups)
		{
			AppendInterface(source, group);
			AppendAccessor(source, group);
		}

		return source.ToString();
	}

	static void AppendInterface(StringBuilder source, IGrouping<string, DynamicAnimationPropertyRequirement> requirements)
	{
		string targetType = requirements.First().TargetTypeName;
		source.AppendLine();
		source.AppendLine("public interface " + requirements.Key);
		source.AppendLine("{");
		foreach (DynamicAnimationPropertyRequirement requirement in requirements)
		{
			source.AppendLine("\tvoid Set" + CreateMethodSuffix(requirement.PropertyName) + "(" + targetType + " target, float value);");
		}
		source.AppendLine("}");
	}

	static void AppendAccessor(StringBuilder source, IGrouping<string, DynamicAnimationPropertyRequirement> requirements)
	{
		DynamicAnimationPropertyRequirement first = requirements.First();
		string targetType = first.TargetTypeName;
		string accessorName = CreateAccessorName(first.InterfaceName);
		source.AppendLine();
		source.AppendLine("public class " + accessorName + " : IDynamicAnimationPropertyAccessor, " + first.InterfaceName);
		source.AppendLine("{");
		source.AppendLine("\tpublic string TargetName => \"" + first.TargetName + "\";");
		source.AppendLine("\tpublic Type TargetType => typeof(" + targetType + ");");
		source.AppendLine();
		source.AppendLine("\tpublic bool Supports(string targetName, string propertyName, Type targetType)");
		source.AppendLine("\t{");
		source.AppendLine("\t\treturn targetName == TargetName && TargetType.IsAssignableFrom(targetType) && (");
		source.AppendLine(string.Join("\n\t\t\t|| ", requirements.Select(requirement => "propertyName == \"" + requirement.PropertyName + "\"")) + ");");
		source.AppendLine("\t}");
		source.AppendLine();
		source.AppendLine("\tpublic void SetValue(object target, string propertyName, float value)");
		source.AppendLine("\t{");
		source.AppendLine("\t\t" + targetType + " typedTarget = target as " + targetType + ";");
		source.AppendLine("\t\tif (typedTarget == null)");
		source.AppendLine("\t\t{");
		source.AppendLine("\t\t\treturn;");
		source.AppendLine("\t\t}");
		source.AppendLine();
		source.AppendLine("\t\tswitch (propertyName)");
		source.AppendLine("\t\t{");
		foreach (DynamicAnimationPropertyRequirement requirement in requirements)
		{
			source.AppendLine("\t\t\tcase \"" + requirement.PropertyName + "\":");
			source.AppendLine("\t\t\t\tSet" + CreateMethodSuffix(requirement.PropertyName) + "(typedTarget, value);");
			source.AppendLine("\t\t\t\tbreak;");
		}
		source.AppendLine("\t\t}");
		source.AppendLine("\t}");
		foreach (DynamicAnimationPropertyRequirement requirement in requirements)
		{
			AppendSetterMethod(source, targetType, requirement);
		}
		source.AppendLine("}");
	}

	static void AppendSetterMethod(StringBuilder source, string targetType, DynamicAnimationPropertyRequirement requirement)
	{
		string[] path = requirement.PropertyName.Split('.');
		source.AppendLine();
		source.AppendLine("\tpublic void Set" + CreateMethodSuffix(requirement.PropertyName) + "(" + targetType + " target, float value)");
		source.AppendLine("\t{");
		if (path.Length == 1)
		{
			source.AppendLine("\t\ttarget." + path[0] + " = " + ConvertValueExpression("value", requirement.ValueTypeName) + ";");
		}
		else if (path.Length == 2)
		{
			source.AppendLine("\t\ttarget." + path[0] + " = target." + path[0] + " with { " + path[1] + " = " + ConvertValueExpression("value", requirement.ValueTypeName) + " };");
		}
		source.AppendLine("\t}");
	}

	static string ConvertValueExpression(string valueExpression, string valueTypeName)
	{
		return valueTypeName == "int" ? "(int)" + valueExpression : valueExpression;
	}

	static string CreateAccessorName(string interfaceName)
	{
		return interfaceName.TrimStart('I') + "Accessor";
	}

	static string CreateMethodSuffix(string propertyName)
	{
		return string.Concat(propertyName.Split('.').Select(part => part.Substring(0, 1).ToUpperInvariant() + part.Substring(1)));
	}
}
