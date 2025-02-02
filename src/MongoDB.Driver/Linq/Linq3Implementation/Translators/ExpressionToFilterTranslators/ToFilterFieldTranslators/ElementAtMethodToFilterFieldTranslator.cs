﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.ExtensionMethods;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators
{
    internal static class ElementAtMethodToFilterFieldTranslator
    {
        public static AstFilterField Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.Is(EnumerableMethod.ElementAt))
            {
                var sourceExpression = arguments[0];
                var field = ExpressionToFilterFieldTranslator.Translate(context, sourceExpression);

                var indexExpression = arguments[1];
                var index = indexExpression.GetConstantValue<int>(containingExpression: expression);

                if (index < 0)
                {
                    var reason = "negative indexes are not valid";
                    if (index == -1)
                    {
                        reason += ". To use the positional operator $ use FirstMatchingElement instead of an index value of -1"; // closing period is added by exception
                    }
                    throw new ExpressionNotSupportedException(expression, because: reason);
                }

                if (field.Serializer is IBsonArraySerializer arraySerializer &&
                    arraySerializer.TryGetItemSerializationInfo(out var itemSerializationInfo))
                {
                    var itemSerializer = itemSerializationInfo.Serializer;
                    return field.SubField(index.ToString(), itemSerializer);
                }
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
