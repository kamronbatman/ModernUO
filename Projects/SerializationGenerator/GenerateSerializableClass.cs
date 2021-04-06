/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2021 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: GenerateSerializableClass.cs                                    *
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
using System.Text;
using Microsoft.CodeAnalysis;

namespace SerializationGenerator
{
    public static class GenerateSerializableClass
    {
        public static void GenerateSerialCtor(this StringBuilder source, GeneratorExecutionContext context, string className, HashSet<ITypeSymbol> list)
        {
            var serialType = (ITypeSymbol)context.Compilation.GetTypeByMetadataName("Server.Serial");
            list.Add(serialType);

            var serialConstant = new TypedConstant();

            source.GenerateConstructorStart(
                className,
                AccessModifier.Public,
                new []{ (serialType, "serial") }.ToImmutableArray(),
                new []{ "serial" }.ToImmutableArray()
            );

            source.AppendLine($@"        public {className}(Serial serial)
        {{
            Serial = serial;
            SetTypeRef(GetType());
        }}");
        }

        public static void GenerateSerialCtorOverride(this StringBuilder source, string className)
        {
            source.AppendLine($@"        public {className}(Serial serial) : base(serial)
        {{
        }}");
        }
    }
}
