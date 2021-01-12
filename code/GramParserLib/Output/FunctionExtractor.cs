using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GramParserLib.Output
{
    internal class FunctionOutput : IRuleOutput
    {
        #region Inner Types
        private interface IFunctionProxy
        {
            string FunctionName { get; }

            int? ParameterCount { get; }

            object? Invoke(IImmutableList<object?> parameters);
        }

        private class ScalableFunctionProxyBase<T, R> : IFunctionProxy
        {
            private readonly Func<IEnumerable<T>, R> _function;

            public ScalableFunctionProxyBase(Func<IEnumerable<T>, R> function)
            {
                _function = function;
            }

            string IFunctionProxy.FunctionName => FormatFunctionName(_function.Method.Name);

            int? IFunctionProxy.ParameterCount => null;

            object? IFunctionProxy.Invoke(IImmutableList<object?> parameters)
            {
                var standardParameters = StandardizeParameters(parameters);

                ValidateParameters(standardParameters);

                return _function(standardParameters.Cast<T>());
            }

            private void ValidateParameters(IImmutableList<object?> parameters)
            {
                var isTypes = from p in parameters.Zip(
                    Enumerable.Range(1, parameters.Count),
                    (p, i) => new { Parameter = p, Index = i })
                              where p.Parameter != null
                              let isType = p.Parameter.GetType() == typeof(T)
                              || p.Parameter.GetType().IsInstanceOfType(typeof(T))
                              where !isType
                              select p;
                var firstTypeViolation = isTypes.FirstOrDefault();

                if (firstTypeViolation != null)
                {
                    throw new ParsingException(
                        $"Function '{((IFunctionProxy)this).FunctionName}' has wrong argument "
                        + $"at parameter {firstTypeViolation.Index}");
                }

                if (typeof(T).IsValueType)
                {
                    var isValueNull = from p in parameters.Zip(
                        Enumerable.Range(1, parameters.Count),
                        (p, i) => new { Parameter = p, Index = i })
                                      where p.Parameter == null
                                      select p;
                    var firstNullViolation = isValueNull.FirstOrDefault();

                    if (firstNullViolation != null)
                    {
                        throw new ParsingException(
                            $"Function '{((IFunctionProxy)this).FunctionName}' "
                            + $" parameter {firstNullViolation.Index}"
                            + " can't be null");
                    }
                }
            }
        }

        private abstract class FixedFunctionProxyBase : IFunctionProxy
        {
            string IFunctionProxy.FunctionName => FormatFunctionName(FunctionName());

            int? IFunctionProxy.ParameterCount => GetParameterTypes().Length;

            object? IFunctionProxy.Invoke(IImmutableList<object?> parameters)
            {
                var standardParameters = StandardizeParameters(parameters);

                ValidateParameters(standardParameters);

                return Invoke(standardParameters);
            }

            protected abstract string FunctionName();

            protected abstract Type[] GetParameterTypes();

            protected abstract object? Invoke(IImmutableList<object?> parameters);

            private void ValidateParameters(IImmutableList<object?> parameters)
            {
                var types = GetParameterTypes();

                if (types.Length != parameters.Count)
                {
                    throw new ParsingException(
                        $"Function '{((IFunctionProxy)this).FunctionName}' should have "
                        + $"{types.Length} parameters, not {parameters.Count}");
                }

                var isTypes = from p in parameters.Zip(
                    types.Zip(
                        Enumerable.Range(1, parameters.Count),
                        (t, i) => (t, i)),
                        (param, pair) => new
                        {
                            Parameter = param,
                            Type = pair.t,
                            Index = pair.i
                        })
                              where p.Parameter != null
                              let isType = p.Parameter.GetType() == p.Type
                              || p.Type.IsAssignableFrom(p.Parameter.GetType())
                              where !isType
                              select p;
                var firstTypeViolation = isTypes.FirstOrDefault();

                if (firstTypeViolation != null)
                {
                    throw new ParsingException(
                        $"Function '{((IFunctionProxy)this).FunctionName}' has wrong argument "
                        + $"at parameter {firstTypeViolation.Index}");
                }
            }
        }

        private class FixedFunctionProxyNoParam<R> : FixedFunctionProxyBase
        {
            private static readonly Type[] _emptyParams = new Type[0];
            private readonly Func<R> _function;

            public FixedFunctionProxyNoParam(Func<R> function)
            {
                _function = function;
            }

            protected override string FunctionName()
            {
                return _function.Method.Name;
            }

            protected override Type[] GetParameterTypes()
            {
                return _emptyParams;
            }

            protected override object? Invoke(IImmutableList<object?> parameters)
            {
                return _function();
            }
        }

        private class FixedFunctionProxyOneParam<T, R> : FixedFunctionProxyBase
            where T : class?
        {
            private static readonly Type[] _params = new[] { typeof(T) };
            private readonly Func<T, R> _function;

            public FixedFunctionProxyOneParam(Func<T, R> function)
            {
                _function = function;
            }

            protected override string FunctionName()
            {
                return _function.Method.Name;
            }

            protected override Type[] GetParameterTypes()
            {
                return _params;
            }

            protected override object? Invoke(IImmutableList<object?> parameters)
            {
                var firstParam = parameters.First();

                if (firstParam == null)
                {
                    throw new NotSupportedException("Do not support null parameter");
                }
                else
                {
                    return _function((T)firstParam);
                }
            }
        }

        private class FixedFunctionProxyTwoParams<T1, T2, R> : FixedFunctionProxyBase
        {
            private static readonly Type[] _params = new[] { typeof(T1), typeof(T2) };
            private readonly Func<T1, T2, R> _function;

            public FixedFunctionProxyTwoParams(Func<T1, T2, R> function)
            {
                _function = function;
            }

            protected override string FunctionName()
            {
                return _function.Method.Name;
            }

            protected override Type[] GetParameterTypes()
            {
                return _params;
            }

            protected override object? Invoke(IImmutableList<object?> parameters)
            {
                var p1 = (T1)parameters[0] ?? throw new NotSupportedException("Do not support null parameter");
                var p2 = (T2)parameters[1] ?? throw new NotSupportedException("Do not support null parameter");

                return _function(p1, p2);
            }
        }

        private class PrependFunctionProxy : IFunctionProxy
        {
            #region Inner Types
            private class PrependList : IEnumerable<object?>
            {
                private readonly object? _head;
                private readonly IEnumerable _tail;

                public PrependList(object? head, IEnumerable<object?> tail)
                {
                    if (head == null)
                    {
                        throw new ArgumentNullException(nameof(head));
                    }
                    if (tail == null)
                    {
                        throw new ArgumentNullException(nameof(tail));
                    }
                    _head = head;
                    _tail = tail;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable<object?>)this).GetEnumerator();
                }

                IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
                {
                    yield return _head;
                    foreach (var element in _tail)
                    {
                        yield return element;
                    }
                }
            }
            #endregion

            string IFunctionProxy.FunctionName => "prepend";

            int? IFunctionProxy.ParameterCount => 2;

            object? IFunctionProxy.Invoke(IImmutableList<object?> parameters)
            {
                var standardParameters = StandardizeParameters(parameters);
                var head = standardParameters[0];
                var list = standardParameters[1] as IEnumerable<object?>;

                if (head == null)
                {
                    throw new ParsingException(
                        $"Function '{((IFunctionProxy)this).FunctionName}' has wrong argument "
                        + "at parameter 0:  isn't a head");
                }
                if (list == null)
                {
                    throw new ParsingException(
                        $"Function '{((IFunctionProxy)this).FunctionName}' has wrong argument "
                        + "at parameter 1:  isn't a list");
                }

                return new PrependList(head, list);
            }
        }
        #endregion

        private static readonly IImmutableDictionary<string, IFunctionProxy>
            _functionMap = InitializeFunctions();
        private readonly IFunctionProxy _function;
        private readonly IImmutableList<IRuleOutput> _parameters;

        public FunctionOutput(
            string functionName,
            IEnumerable<IRuleOutput> parameters)
        {
            if (!_functionMap.TryGetValue(functionName, out _function))
            {
                throw new ParsingException($"Function '{functionName}' doesn't exist");
            }
            _parameters = ImmutableArray<IRuleOutput>
                .Empty
                .AddRange(parameters);
            if (_function.ParameterCount != null && _function.ParameterCount != _parameters.Count)
            {
                throw new ParsingException(
                    $"Function '{functionName}' should have {_function.ParameterCount} parameters, "
                    + $"not {_parameters.Count}");
            }
        }

        object? IRuleOutput.ComputeOutput(SubString text, Lazy<object?> lazyDefaultOutput)
        {
            var parameterOutputs = from p in _parameters
                                   select p.ComputeOutput(text, lazyDefaultOutput);

            return _function.Invoke(ImmutableList<object?>.Empty.AddRange(parameterOutputs));
        }

        private static IImmutableList<object?> StandardizeParameters(IImmutableList<object?> parameters)
        {
            var standards = parameters
                .Select(p => (p != null && p.GetType() == typeof(SubString))
                ? p.ToString()
                : p)
                .ToImmutableList();

            return standards;
        }

        private static IImmutableDictionary<string, IFunctionProxy> InitializeFunctions()
        {
            var proxies = new IFunctionProxy[]
            {
                new FixedFunctionProxyOneParam<string, bool>(Boolean),
                new FixedFunctionProxyOneParam<string, int>(Integer),
                new FixedFunctionProxyOneParam<
                    IImmutableList<object?>,
                    IEnumerable<object?>>(Flatten),
                new FixedFunctionProxyOneParam<
                    IEnumerable<object?>,
                    object?>(FirstOrNull),
                new ScalableFunctionProxyBase<string, string>(Concat),
                new PrependFunctionProxy()
            };
            var pairs = from p in proxies
                        select KeyValuePair.Create(p.FunctionName, p);

            return ImmutableDictionary<string, IFunctionProxy>
                .Empty
                .AddRange(pairs);
        }

        private static string FormatFunctionName(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1, name.Length - 1);
        }

        #region Functions
        private static int Integer(string text)
        {
            int integer;

            if (!int.TryParse(text, out integer))
            {
                throw new ParsingException($"Can't parse '{text}' to an integer");
            }

            return integer;
        }

        private static bool Boolean(string text)
        {
            bool boolean;

            if (!bool.TryParse(text, out boolean))
            {
                throw new ParsingException($"Can't parse '{text}' to a boolean");
            }

            return boolean;
        }

        private static string Concat(IEnumerable<string> texts)
        {
            return string.Concat(texts);
        }

        private static IImmutableList<object?> Flatten(IEnumerable<object?> arrays)
        {
            var flatten = arrays
                .Cast<IEnumerable<object?>>()
                .SelectMany(i => i)
                .ToImmutableArray<object?>();

            return flatten;
        }

        private static object? FirstOrNull(IEnumerable<object?> arrays)
        {
            var firstOrNull = arrays.FirstOrDefault();

            return firstOrNull;
        }
        #endregion
    }
}