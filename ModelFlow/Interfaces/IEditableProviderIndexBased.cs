﻿namespace ModelFlow.DataVirtualization.Interfaces
{
    internal interface IEditableProviderIndexBased<T> : IEditableProvider<T>
    {
        T OnRemove(int index, object timestamp);
        T OnReplace(int index, T newItem, object timestamp);
    }
}