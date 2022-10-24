using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Wrench.CodeGen
{
	public static class DebugPrinter
	{
		public static string Encoded(Action<StringBuilder> action)
		{
			StringBuilder sb = new StringBuilder();
			action?.Invoke(sb);
			return sb.ToString().Replace(Environment.NewLine,"%0A").Replace("\t","%09");
		}

		private static void Tabbed(this StringBuilder sb, int depth, string msg)
		{
			sb.Tabs(depth);
			sb.AppendLine(msg);
		}
		
		private static void Tabs(this StringBuilder sb, int depth)
		{
			for (int i = 0; i < depth; i++) sb.Append("\t");
		}
		
		private static void Open(this StringBuilder sb, string name, ref int depth, int? count = null)
		{
			sb.Tabs(depth);
			sb.Append(name);
			if (count.HasValue) sb.Append($"({count})");
			sb.AppendLine("{");
			depth++;
		}

		
		private static void Close(this StringBuilder sb, ref int depth)
		{
			depth--;
			sb.Tabbed(depth, "}");
		}
		
		public static void Print(StringBuilder sb, int d, MethodReference method)
		{
			switch (method)
			{
				default:
					var metaType = method.GetType();
					if (metaType.IsSubclassOf(typeof(TypeReference)))
					{
						sb.Tabbed(d, $"~{metaType}~");
					}

					sb.Open(method.FullName, ref d);

					var defMethod = method.Resolve();
					Print(sb, d, defMethod.CustomAttributes);
					
					Print(sb, d, method.Parameters);
					Print(sb, d, method.GenericParameters);
					
					sb.Close(ref d);
					break;
			}
		}

		public static void Print(StringBuilder sb, int d, TypeReference type)
		{
			switch (type)
			{
				case ByReferenceType byReferenceType:
					Print(sb, d, byReferenceType);
					break;

				case ArrayType arrayType:
					Print(sb, d, arrayType);
					break;

				case GenericInstanceType genericInstanceType:
					Print(sb, d, genericInstanceType);
					break;

				case GenericParameter genericParameter:
					Print(sb, d, genericParameter);
					break;

				default:
					var metaType = type.GetType();
					if (metaType != typeof(TypeDefinition) && metaType.IsSubclassOf(typeof(TypeReference)))
					{
						sb.Tabbed(d, $"~{metaType}~");
					}

					sb.Open(type.FullName, ref d);
					
					Print(sb, d, type.GenericParameters);
					
					sb.Close(ref d);
					break;
			}
		}

		public static void Print(StringBuilder sb, int d, IList<ParameterDefinition> types)
		{
			sb.Open("ParameterReferences", ref d, types.Count);

			for (int i = 0; i < types.Count; i++)
			{
				Print(sb, d, types[i].ParameterType);
			}

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, ByReferenceType type)
		{
			sb.Open("ByReferenceType", ref d);

			Print(sb, d, type.ElementType.Resolve());

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, GenericInstanceType type)
		{
			sb.Open("GenericInstanceType", ref d);
			sb.Open(type.FullName, ref d);

			Print(sb, d, type.GenericArguments, "GenericArguments");

			sb.Close(ref d);
			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, IList<GenericParameter> types)
		{
			sb.Open("GenericParameters", ref d, types.Count);

			for (int i = 0; i < types.Count; i++)
			{
				Print(sb, d, (TypeReference) types[i]);
			}

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, IList<CustomAttribute> types)
		{
			sb.Open("CustomAttribute", ref d, types.Count);

			for (int i = 0; i < types.Count; i++)
			{
				Print(sb, d, types[i].AttributeType);
				Print(sb, d, types[i].ConstructorArguments);
			}

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, IList<CustomAttributeArgument> types)
		{
			sb.Open("CustomAttributeArgument", ref d, types.Count);

			for (int i = 0; i < types.Count; i++)
			{
				if (types[i].Value is TypeReference reference) Print(sb, d, reference);
				else Print(sb, d, types[i].Type);
			}

			sb.Close(ref d);
		}

		
		public static void Print(StringBuilder sb, int d, IList<TypeReference> types, string name = "TypeReferences")
		{
			sb.Open(name, ref d, types.Count);

			for (int i = 0; i < types.Count; i++)
			{
				Print(sb, d, types[i]);
			}

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, GenericParameter type)
		{
			sb.Open(type.FullName, ref d);

			Print(sb, d, type.Constraints, "Constraints");

			sb.Close(ref d);
		}

		public static void Print(StringBuilder sb, int d, ArrayType type)
		{
			sb.Open("ArrayType", ref d);

			Print(sb, d, type.ElementType);

			sb.Close(ref d);
		}
	}
}
