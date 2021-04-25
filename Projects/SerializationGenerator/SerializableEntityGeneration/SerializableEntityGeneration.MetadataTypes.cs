/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2021 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: SerializableEntityGeneration.MetadataTypes.cs                   *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System.Linq;
using Microsoft.CodeAnalysis;

namespace SerializationGenerator
{
    public static partial class SerializableEntityGeneration
    {
        public const string SERIALIZABLE_ATTRIBUTE = "Server.SerializableAttribute";
        public const string SERIALIZABLE_FIELD_ATTRIBUTE = "Server.SerializableFieldAttribute";
        public const string SERIALIZABLE_FIELD_ATTR_ATTRIBUTE = "Server.SerializableFieldAttrAttribute";
        public const string SERIALIZABLE_INTERFACE = "Server.ISerializable";
        public const string GENERIC_WRITER_INTERFACE = "Server.IGenericWriter";
        public const string GENERIC_READER_INTERFACE = "Server.IGenericReader";
        public const string DELTA_DATE_TIME_ATTRIBUTE = "Server.DeltaDateTimeAttribute";

        public static bool IsDeltaDateTime(this ISymbol symbol, Compilation compilation) =>
            symbol.Equals(compilation.GetTypeByMetadataName(DELTA_DATE_TIME_ATTRIBUTE), SymbolEqualityComparer.Default);

        public static bool IsEnum(this ITypeSymbol symbol) =>
            symbol.SpecialType == SpecialType.System_Enum || symbol.TypeKind == TypeKind.Enum;

        public static bool HasSerializableInterface(this ITypeSymbol symbol, Compilation compilation) =>
            symbol.BaseType.ContainsInterface(compilation.GetTypeByMetadataName(SERIALIZABLE_INTERFACE));

        public static bool HasGenericReaderCtor(this INamedTypeSymbol symbol, Compilation compilation)
        {
            var genericReaderInterface = compilation.GetTypeByMetadataName(GENERIC_READER_INTERFACE);

            return symbol.Constructors.Any(
                m => !m.IsStatic &&
                     m.MethodKind == MethodKind.Constructor &&
                     m.Parameters.Length == 1 &&
                     m.Parameters[0].Equals(genericReaderInterface, SymbolEqualityComparer.Default)
            );
        }

        public static bool HasPublicSerializeMethod(this INamedTypeSymbol symbol, Compilation compilation)
        {
            if (symbol.HasSerializableInterface(compilation))
            {
                return true;
            }

            var genericWriterInterface = compilation.GetTypeByMetadataName(GENERIC_WRITER_INTERFACE);

            return symbol.GetAllMethods("Serialize")
                .Any(
                    m => !m.IsStatic &&
                         m.ReturnsVoid &&
                         m.Parameters.Length == 1 &&
                         m.Parameters[0].Equals(genericWriterInterface, SymbolEqualityComparer.Default) &&
                         m.DeclaredAccessibility == Accessibility.Public
                );
        }

        public static bool IsListOfSerializable(this ITypeSymbol symbol, Compilation compilation) =>
            symbol.AllInterfaces.FirstOrDefault(
                i => i.ContainsInterface(compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1"))
            )?.TypeParameters[0].HasSerializableInterface(compilation) == true;

        public static bool IsHashSetOfSerializable(this ITypeSymbol symbol, Compilation compilation) =>
            symbol.AllInterfaces.FirstOrDefault(
                i => i.ContainsInterface(compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1"))
            )?.TypeParameters[0].HasSerializableInterface(compilation) == true;
    }
}
