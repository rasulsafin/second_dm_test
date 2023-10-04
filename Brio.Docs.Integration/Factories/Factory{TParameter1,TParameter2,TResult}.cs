using System;

namespace Brio.Docs.Integration.Factories
{
    public class Factory<TParameter1, TParameter2, TResult> : IFactory<TParameter1, TParameter2, TResult>
    {
        private readonly Func<TParameter1, TParameter2, TResult> func;

        public Factory(Func<TParameter1, TParameter2, TResult> func)
            => this.func = func;

        public TResult Create(TParameter1 parameter1, TParameter2 parameter2)
            => func(parameter1, parameter2);
    }
}
