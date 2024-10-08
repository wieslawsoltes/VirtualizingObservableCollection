﻿namespace ModelFlow.DataVirtualization.Interfaces
{
    using System.Threading.Tasks;

    internal interface IItemSourceProviderAsync<T> : IBaseSourceProvider
    {
        Task<int> Count { get; }
        Task<T> GetAt(int index, object voc);

        T GetPlaceHolder(int index);

        Task<int> IndexOf(T item);
    }
}