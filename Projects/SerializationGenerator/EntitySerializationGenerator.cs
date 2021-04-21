/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2021 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: EntityJsonGenerator.cs                                          *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SerializationGenerator
{
    [Generator]
    public class EntitySerializationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            context.RegisterForPostInitialization(i =>
            {
                SerializerSyntaxReceiver.AttributeTypes.Add("Server.SerializableAttribute");
                SerializerSyntaxReceiver.AttributeTypes.Add("Server.SerializableFieldAttribute");
            });

            context.RegisterForSyntaxNotifications(() => new SerializerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SerializerSyntaxReceiver receiver)
            {
                return;
            }

            foreach (IGrouping<ISymbol, IFieldSymbol> group in receiver.Fields.GroupBy(f => f.ContainingType, SymbolEqualityComparer.Default))
            {
                string classSource = SerializableEntityGeneration.GenerateSerializationPartialClass(
                    group.Key as INamedTypeSymbol,
                    group.ToList(),
                    context
                );

                if (classSource != null)
                {
                    context.AddSource($"{group.Key.Name}.Serialization.cs", SourceText.From(classSource, Encoding.UTF8));
                }
            }
        }
    }
}
