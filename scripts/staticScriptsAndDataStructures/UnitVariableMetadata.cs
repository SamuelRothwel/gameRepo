using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class UnitVariableTypeDefinition
{
	public Type Type { get; set; }
	public string TypeName { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public string AssemblyName { get; set; } = "";
	public string Kind { get; set; } = "";
	public string DirectBaseTypeName { get; set; } = "";
	public string SourceVersion { get; set; } = "";
	public List<UnitVariableDefinition> Variables { get; set; } = new();
	public List<UnitVariableObjectMemberDefinition> ObjectMembers { get; set; } = new();
}

public class UnitVariableDefinition
{
	public string Name { get; set; } = "";
	public string ValueTypeName { get; set; } = "";
	public string VariableKind { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool CanRead { get; set; }
	public bool CanWrite { get; set; }
	public bool IsObjectReference { get; set; }
}

public class UnitVariableObjectMemberDefinition
{
	public string Name { get; set; } = "";
	public string MemberTypeName { get; set; } = "";
	public string VariableKind { get; set; } = "";
	public bool IsCollection { get; set; }
	public string ElementTypeName { get; set; } = "";
}

public static class UnitVariableMetadataScanner
{
	static readonly HashSet<Type> valueObjectTypes = new()
	{
		typeof(string),
		typeof(Vector2),
		typeof(Vector2I),
		typeof(Vector3),
		typeof(Vector3I),
		typeof(Vector4),
		typeof(Vector4I),
		typeof(Color),
		typeof(Rect2),
		typeof(Rect2I),
		typeof(Transform2D),
		typeof(Transform3D),
		typeof(Quaternion),
		typeof(Basis)
	};

	public static List<Type> GetUnitAndComponentTypes(Assembly assembly)
	{
		return assembly.GetTypes()
			.Where(type => type.IsClass && !type.IsAbstract)
			.Where(type =>
				typeof(unitControler).IsAssignableFrom(type) ||
				typeof(componentController).IsAssignableFrom(type) ||
				typeof(subComponent).IsAssignableFrom(type))
			.OrderBy(type => GetTypeName(type))
			.ToList();
	}

	public static List<UnitVariableTypeDefinition> Scan(IEnumerable<Type> rootTypes)
	{
		Dictionary<string, UnitVariableTypeDefinition> definitions = new();
		foreach (Type type in rootTypes)
		{
			ScanType(type, definitions, true, true);
		}

		return definitions.Values
			.OrderBy(definition => definition.TypeName)
			.ToList();
	}

	static void ScanType(Type type, Dictionary<string, UnitVariableTypeDefinition> definitions, bool includeImmediateBase, bool followObjectMembers)
	{
		type = NormalizeType(type);
		if (type == null || type == typeof(object))
		{
			return;
		}

		string typeName = GetTypeName(type);
		if (definitions.ContainsKey(typeName))
		{
			return;
		}

		UnitVariableTypeDefinition definition = new UnitVariableTypeDefinition
		{
			Type = type,
			TypeName = typeName,
			DisplayName = type.Name,
			AssemblyName = type.Assembly.GetName().Name ?? "",
			Kind = GetKind(type),
			DirectBaseTypeName = GetDirectBaseTypeName(type, includeImmediateBase),
			SourceVersion = GetSourceVersion(type)
		};
		definitions[typeName] = definition;

		foreach (MemberInfo member in GetVariableMembers(type))
		{
			Type memberType = GetMemberType(member);
			Type normalizedMemberType = NormalizeType(memberType);
			bool isObjectReference = IsObjectReference(memberType);

			definition.Variables.Add(new UnitVariableDefinition
			{
				Name = member.Name,
				ValueTypeName = GetTypeName(memberType),
				VariableKind = member.MemberType == MemberTypes.Field ? "field" : "property",
				IsPublic = IsPublic(member),
				CanRead = CanRead(member),
				CanWrite = CanWrite(member),
				IsObjectReference = isObjectReference
			});

			if (!isObjectReference)
			{
				continue;
			}

			Type elementType = GetCollectionElementType(memberType);
			Type storedObjectType = NormalizeType(elementType ?? memberType);
			if (storedObjectType == null)
			{
				continue;
			}

			definition.ObjectMembers.Add(new UnitVariableObjectMemberDefinition
			{
				Name = member.Name,
				MemberTypeName = GetTypeName(memberType),
				VariableKind = member.MemberType == MemberTypes.Field ? "field" : "property",
				IsCollection = elementType != null,
				ElementTypeName = elementType == null ? "" : GetTypeName(elementType)
			});

			if (followObjectMembers)
			{
				ScanType(storedObjectType, definitions, IsProjectType(storedObjectType), ShouldFollowObjectMembers(storedObjectType));
			}
		}

		if (!string.IsNullOrEmpty(definition.DirectBaseTypeName))
		{
			ScanType(type.BaseType, definitions, false, false);
		}
	}

	static IEnumerable<MemberInfo> GetVariableMembers(Type type)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
		foreach (FieldInfo field in type.GetFields(flags))
		{
			if (!field.IsSpecialName && !field.Name.Contains('<'))
			{
				yield return field;
			}
		}
		foreach (PropertyInfo property in type.GetProperties(flags))
		{
			if (!property.IsSpecialName && property.GetIndexParameters().Length == 0)
			{
				yield return property;
			}
		}
	}

	static string GetDirectBaseTypeName(Type type, bool includeImmediateBase)
	{
		if (!includeImmediateBase || type.BaseType == null || type.BaseType == typeof(object))
		{
			return "";
		}

		return GetTypeName(type.BaseType);
	}

	static Type GetMemberType(MemberInfo member)
	{
		if (member is FieldInfo field)
		{
			return field.FieldType;
		}
		return ((PropertyInfo)member).PropertyType;
	}

	static bool IsPublic(MemberInfo member)
	{
		if (member is FieldInfo field)
		{
			return field.IsPublic;
		}

		PropertyInfo property = (PropertyInfo)member;
		return (property.GetMethod?.IsPublic ?? false) || (property.SetMethod?.IsPublic ?? false);
	}

	static bool CanRead(MemberInfo member)
	{
		return member is FieldInfo || ((PropertyInfo)member).CanRead;
	}

	static bool CanWrite(MemberInfo member)
	{
		return member is FieldInfo field
			? !field.IsInitOnly
			: ((PropertyInfo)member).CanWrite;
	}

	static bool IsObjectReference(Type type)
	{
		Type normalizedType = NormalizeType(type);
		return normalizedType != null
			&& !normalizedType.IsPrimitive
			&& !normalizedType.IsEnum
			&& !normalizedType.IsValueType
			&& !valueObjectTypes.Contains(normalizedType);
	}

	static Type NormalizeType(Type type)
	{
		if (type == null)
		{
			return null;
		}
		if (type.IsByRef || type.IsPointer)
		{
			return NormalizeType(type.GetElementType());
		}
		return Nullable.GetUnderlyingType(type) ?? type;
	}

	static Type GetCollectionElementType(Type type)
	{
		if (type == typeof(string))
		{
			return null;
		}
		if (type.IsArray)
		{
			return type.GetElementType();
		}
		if (type.IsGenericType)
		{
			Type genericDefinition = type.GetGenericTypeDefinition();
			if (genericDefinition == typeof(List<>) ||
				genericDefinition == typeof(IReadOnlyList<>) ||
				genericDefinition == typeof(IEnumerable<>) ||
				genericDefinition == typeof(Dictionary<,>) ||
				type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
			{
				return type.GetGenericArguments().LastOrDefault();
			}
		}
		return null;
	}

	static bool ShouldFollowObjectMembers(Type type)
	{
		return IsProjectType(type) || type.Namespace == "coolbeats.scripts.staticScriptsAndDataStructures";
	}

	static bool IsProjectType(Type type)
	{
		return type.Assembly == typeof(unitControler).Assembly;
	}

	static string GetKind(Type type)
	{
		if (typeof(unitControler).IsAssignableFrom(type))
		{
			return "unit";
		}
		if (typeof(componentController).IsAssignableFrom(type))
		{
			return "component";
		}
		if (typeof(subComponent).IsAssignableFrom(type))
		{
			return "subComponent";
		}
		return "object";
	}

	static string GetSourceVersion(Type type)
	{
		try
		{
			return type.Module.ModuleVersionId + ":" + type.MetadataToken;
		}
		catch
		{
			return "";
		}
	}

	static string GetTypeName(Type type)
	{
		if (type == null)
		{
			return "";
		}
		if (!type.IsGenericType)
		{
			return type.FullName ?? type.Name;
		}

		string name = type.GetGenericTypeDefinition().FullName ?? type.Name;
		int tickIndex = name.IndexOf('`');
		if (tickIndex >= 0)
		{
			name = name[..tickIndex];
		}
		return name + "<" + string.Join(",", type.GetGenericArguments().Select(GetTypeName)) + ">";
	}
}
