﻿namespace VirtualizingCollection.Tests
{
    using System.Reactive.Linq;
    using DataGridAsyncDemoMVVM;
    using FluentAssertions;
    using ReactiveUI;
    using ModelFlow.DataVirtualization;

    public class VirtualizingObservableCollectionTest
    {
        public VirtualizingObservableCollectionTest()
        {
            VirtualizationManager.Instance.UiThreadExcecuteAction = UiThreadExcecuteAction;
        }

        private Task UiThreadExcecuteAction(Action arg)
        {
            arg();
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(24)]
        [InlineData(25)]
        [InlineData(26)]
        [InlineData(49)]
        [InlineData(50)]
        [InlineData(51)]
        [InlineData(76)]
        [InlineData(75)]
        [InlineData(74)]
        [InlineData(101)]
        [InlineData(100)]
        [InlineData(99)]
        public async Task? Deleting_Last_Item_Not_At_Page_Boundary_Removes_The_Correct_Item(int offsetFromEnd)
        {
            var dataSource = new RemoteOrDbDataSource();
            
            // Trigger datasource loading.
            Assert.Equal(0, dataSource.Collection.Count);

            await dataSource.Collection.WhenAnyValue(x => x.Count)
                .Skip(1)
                .FirstAsync();

            Assert.Equal(1025, dataSource.Collection.Count);

            var item = dataSource.Collection[^offsetFromEnd];

            Assert.True(item.IsLoading);

            await item.WhenAnyValue(x => x.IsLoading)
                .Skip(1).FirstAsync();
            
            Assert.False(item.IsLoading);

            Assert.Same(item, dataSource.Collection[^offsetFromEnd]);
            
            await dataSource.DeleteAsync(item);

            Assert.Equal(1024, dataSource.Collection.Count);

            Assert.NotSame(item, dataSource.Collection[^offsetFromEnd]);
        }

        [Fact]
        public async Task Item_Is_Set_When_DataItem_IsLoading_Changes()
        {
            var dataSource = new RemoteOrDbDataSource();
            
            // Trigger datasource loading.
            Assert.Equal(0, dataSource.Collection.Count);
            
            // Wait for the count to be updated.
            await dataSource.Collection.WhenAnyValue(x => x.Count)
                .Skip(1)
                .FirstAsync();

            // Get the last item.
            var item = dataSource.Collection[^1];
            Assert.True(item.IsLoading);

            using var monitoredItem = item.Monitor();
            
            // Check when IsLoading changes, the item is already wrapping the correct model.
            item.WhenAnyValue(x => x.IsLoading)
                .Skip(1)
                .Do(_ =>
                {
                    Assert.False(item.IsLoading);
                    Assert.Same(dataSource.Emulation.Items[^1], item.Item.Model);
                })
                .Subscribe();
            
            // Check when the item changes the IsLoading is correct and the models are set correctly.
            item.WhenAnyValue(x => x.Item)
                .Skip(1)
                .Do(_ =>
                {
                    Assert.False(item.IsLoading);
                    Assert.Same(dataSource.Emulation.Items[^1], item.Item.Model);
                })
                .Subscribe();

            await Task.Delay(500);

            monitoredItem.Should().RaisePropertyChangeFor(x => x.IsLoading);
            monitoredItem.Should().RaisePropertyChangeFor(x => x.Item);

            Assert.False(item.IsLoading);
            Assert.Same(dataSource.Emulation.Items[^1], item.Item.Model);
        }
        
    }
}