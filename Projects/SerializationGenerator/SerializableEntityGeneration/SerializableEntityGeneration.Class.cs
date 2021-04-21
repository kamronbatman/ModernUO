/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2021 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: SerializableEntityGeneration.Class.cs                           *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SerializationGenerator
{
    public static partial class SerializableEntityGeneration
    {
        public static string GenerateSerializationPartialClass(
            INamedTypeSymbol classSymbol,
            List<IFieldSymbol> fields,
            GeneratorExecutionContext context
        )
        {
            var serializableEntityAttribute = context.Compilation.GetTypeByMetadataName("Server.SerializableAttribute");
            var serializableFieldAttribute = context.Compilation.GetTypeByMetadataName("Server.SerializableFieldAttribute");
            var serializableInterface = context.Compilation.GetTypeByMetadataName("Server.ISerializable");
            var genericWriterInterface = context.Compilation.GetTypeByMetadataName("Server.IGenericWriter");
            var genericReaderInterface = context.Compilation.GetTypeByMetadataName("Server.IGenericReader");

            // This is a class symbol if the containing symbol is the namespace
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null;
            }

            // If we have a parent that is or derives from ISerializable, then we are in override
            var isOverride = classSymbol.BaseType.ContainsInterface(serializableInterface);

            if (!isOverride && !classSymbol.ContainsInterface(serializableInterface))
            {
                return null;
            }

            var versionValue = classSymbol.GetAttributes()
                .FirstOrDefault(
                    attr => attr.AttributeClass?.Equals(serializableEntityAttribute, SymbolEqualityComparer.Default) ?? false
                )
                ?.ConstructorArguments.FirstOrDefault()
                .Value;

            if (versionValue == null)
            {
                return null; // We don't have the attribute
            }

            var version = int.Parse(versionValue.ToString());
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            HashSet<string> namespaceList = new();

            StringBuilder source = new StringBuilder();

            source.GenerateNamespaceStart(namespaceName);

            source.GenerateClassStart(
                className,
                isOverride ?
                    new ImmutableArray<ITypeSymbol>{ serializableInterface } :
                    ImmutableArray<ITypeSymbol>.Empty
            );

            foreach (IFieldSymbol fieldSymbol in fields)
            {
                var hasAttribute = fieldSymbol.GetAttributes()
                    .Any(
                        attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttribute)
                    );

                if (!hasAttribute)
                {
                    continue;
                }

                source.GenerateProperty(fieldSymbol, serializableFieldAttribute);
            }

            // Serial constructor
            source.GenerateSerialCtor(context, className);

            // Serialize Method
            source.GenerateMethodStart(
                "Serialize",
                AccessModifier.Public,
                isOverride,
                "void",
                new ImmutableArray<(ITypeSymbol, string)>{ (genericWriterInterface, "writer") }
            );
            source.GenerateMethodEnd();

            // Deserialize Method
            source.GenerateMethodStart(
                "Deserialize",
                AccessModifier.Public,
                isOverride,
                "void",
                new ImmutableArray<(ITypeSymbol, string)>{ (genericReaderInterface, "reader") }
            );
            source.GenerateMethodEnd();

            source.GenerateClassEnd();
            source.GenerateNamespaceEnd();

            source.GenerateClassEnd();

            return source.ToString();
        }
    }
}
