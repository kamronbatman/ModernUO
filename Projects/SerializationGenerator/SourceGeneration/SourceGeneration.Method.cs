/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2021 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: SourceGeneration.Method.cs                                      *
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
    public static partial class SourceGeneration
    {
        public static void GenerateMethodStart(this StringBuilder source, string methodName, AccessModifier accessors, bool isOverride, string returnType, ImmutableArray<(ITypeSymbol, string)> parameters)
        {
            source.Append($@"        {accessors.ToFriendlyString()}{(isOverride ? " override" : "")} {returnType} {methodName}(");
            source.GenerateSignatureArguments(parameters);
            source.AppendLine(@")
        {");
        }

        public static void GenerateMethodEnd(this StringBuilder source) => source.AppendLine(@"        }");

        public static void GenerateConstructorStart(
            this StringBuilder source, string className, AccessModifier accessors, ImmutableArray<(ITypeSymbol, string)> parameters,
            ImmutableArray<string> baseParameters, bool isOverload = false
        )
        {
            source.Append($@"        {accessors.ToFriendlyString()} {className}(");
            source.GenerateSignatureArguments(parameters);
            source.Append(')');
            bool hasBaseParams = baseParameters.Length > 0;
            if (hasBaseParams)
            {
                source.AppendFormat(" : {0}(", isOverload ? "this" : "base");
                for (int i = 0; i < baseParameters.Length; i++)
                {
                    source.Append(baseParameters[i]);
                    if (i < baseParameters.Length - 1)
                    {
                        source.Append(',');
                    }
                }
                source.Append(')');
            }

            source.AppendLine("        {");
        }
    }
}
