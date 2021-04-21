/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2021 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: SerializableEntityGeneration.SerialCtor.cs                      *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SerializationGenerator
{
    public static partial class SerializableEntityGeneration
    {
        private static readonly ImmutableArray<string> _baseParameters = new[] { "serial" }.ToImmutableArray();
        public static void GenerateSerialCtor(
            this StringBuilder source,
            GeneratorExecutionContext context,
            string className,
            bool isOverride
        )
        {
            var serialType = (ITypeSymbol)context.Compilation.GetTypeByMetadataName("Server.Serial");

            source.GenerateConstructorStart(
                className,
                AccessModifier.Public,
                new []{ (serialType, "serial") }.ToImmutableArray(),
                isOverride ? ImmutableArray<string>.Empty : _baseParameters
            );

            if (!isOverride)
            {
                source.Append(@$"            Serial = serial;
            SetTypeRef(typeof({className}));");
            }

            source.GenerateMethodEnd();
        }
    }
}
