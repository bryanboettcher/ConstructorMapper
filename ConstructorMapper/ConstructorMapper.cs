using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pingring.Utilities.ConstructorMapper
{
    public static class ConstructorMapper
    {
        private static readonly IDictionary<TypePair, object> Mappings;

        static ConstructorMapper()
        {
            Mappings = new Dictionary<TypePair, object>();
        }

        public static void Clear()
        {
            Mappings.Clear();
        }

        public static void Configure<TIn, TOut>()
        {
            var key = new TypePair(typeof(TIn), typeof(TOut));

            var inputParameter = Expression.Parameter(typeof(TIn));
            var destinationConstructor = GetIdealConstructor<TOut>();

            var sourceProperties = typeof(TIn).GetProperties();

            var destinationConstructorArguments = (from destinationConstructorParameter in destinationConstructor.GetParameters()
                                                   let sourceProperty = sourceProperties.FirstOrDefault(pi => IsCorrectProperty(pi, destinationConstructorParameter))
                                                   select sourceProperty != null
                                                       ? (Expression)Expression.Property(inputParameter, sourceProperty.Name)
                                                       : (Expression)Expression.Default(destinationConstructorParameter.ParameterType));

            // "new TOutput(x, y, z)"
            var constructionExpression = Expression.New(destinationConstructor, destinationConstructorArguments);

            // old => new TOutput(old.x, old.y, old.z)
            var converterExpression = Expression.Lambda<Func<TIn, TOut>>(constructionExpression, inputParameter);

            var converterLambda = converterExpression.Compile();
            Mappings.Add(key, converterLambda);
        }

        private static bool IsCorrectProperty(PropertyInfo pi, ParameterInfo destinationConstructorParameter)
        {
            return pi.PropertyType.IsAssignableFrom(destinationConstructorParameter.ParameterType) &&
                   (NamesMatch(pi.Name, destinationConstructorParameter.Name));
        }

        private static bool NamesMatch(string a, string b)
        {
            Func<string, string> sanitize = s => s.ToLowerInvariant().Replace("_", "");

            return sanitize(a) == sanitize(b);
        }

        private static ConstructorInfo GetIdealConstructor<TOut>()
        {
            return typeof(TOut).GetConstructors().OrderByDescending(ci => ci.GetParameters().Count()).First();
        }

        public static TOut Map<TIn, TOut>(TIn input)
        {
            var key = new TypePair(typeof(TIn), typeof(TOut));

            if (!Mappings.ContainsKey(key))
                throw new InvalidOperationException("No mapping defined");

            var converter = (Func<TIn, TOut>)Mappings[key];
            return converter(input);
        }

        class TypePair : IEquatable<TypePair>
        {
            public bool Equals(TypePair other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return T1.Equals(other.T1) && T2.Equals(other.T2);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((TypePair)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (T1.GetHashCode() * 397) ^ T2.GetHashCode();
                }
            }

            public static bool operator ==(TypePair left, TypePair right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TypePair left, TypePair right)
            {
                return !Equals(left, right);
            }

            public TypePair(Type t1, Type t2)
            {
                T1 = t1;
                T2 = t2;
            }

            public readonly Type T1;
            public readonly Type T2;
        }
    }
}
