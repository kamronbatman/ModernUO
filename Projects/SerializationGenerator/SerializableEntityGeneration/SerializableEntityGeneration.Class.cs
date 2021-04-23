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

using System;
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
            var serializableFieldAttrAttribute =
                context.Compilation.GetTypeByMetadataName("Server.SerializableFieldAttrAttribute");
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
                    ImmutableArray<ITypeSymbol>.Empty :
                    ImmutableArray.Create<ITypeSymbol>(serializableInterface)
            );

            foreach (IFieldSymbol fieldSymbol in fields)
            {
                var allAttributes = fieldSymbol.GetAttributes();

                var hasAttribute = allAttributes
                    .Any(
                        attr =>
                            SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttribute)
                    );

                if (hasAttribute)
                {
                    foreach (var attr in allAttributes)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttrAttribute))
                        {
                            continue;
                        }

                        if (attr.AttributeClass == null)
                        {
                            continue;
                        }

                        var ctorArgs = attr.ConstructorArguments;
                        var attrType = ((Type)ctorArgs[0].Value).Name;
                        var args = ctorArgs.Skip(1).ToImmutableArray();

                        source.GenerateAttribute(attrType, args);
                    }

                    source.GenerateSerializableProperty(fieldSymbol);
                    source.AppendLine();
                }
            }

            // If we are not inheriting ISerializable, then we need to define some stuff
            if (!isOverride)
            {
                // long ISerializable.SavePosition { get; set; }
                source.GenerateAutoProperty(
                    AccessModifier.None,
                    "long",
                    "ISerializable.SavePosition",
                    AccessModifier.None,
                    AccessModifier.None
                );
                source.AppendLine();

                // BufferWriter ISerializable.SaveBuffer { get; set; }
                source.GenerateAutoProperty(
                    AccessModifier.None,
                    "BufferWriter",
                    "ISerializable.SaveBuffer",
                    AccessModifier.None,
                    AccessModifier.None
                );
                source.AppendLine();
            }

            // Serial constructor
            source.GenerateSerialCtor(context, className, isOverride);
            source.AppendLine();

            // Serialize Method
            source.GenerateMethodStart(
                "Serialize",
                AccessModifier.Public,
                isOverride,
                "void",
                ImmutableArray.Create<(ITypeSymbol, string)>((genericWriterInterface, "writer"))
            );
            // Generate serialize method stuff here
            source.GenerateMethodEnd();
            source.AppendLine();

            // Deserialize Method
            source.GenerateMethodStart(
                "Deserialize",
                AccessModifier.Public,
                isOverride,
                "void",
                ImmutableArray.Create<(ITypeSymbol, string)>((genericReaderInterface, "reader"))
            );
            // Generate deserialize method stuff here
            source.GenerateMethodEnd();

            source.GenerateClassEnd();
            source.GenerateNamespaceEnd();

            return source.ToString();
        }
    }
}
