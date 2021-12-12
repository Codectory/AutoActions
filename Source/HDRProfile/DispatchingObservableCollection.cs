using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutoHDR
{
    /// <summary>  
    /// This class is an observablecollection which invokes automatically.  
    /// This means that any change will be done in the right thread.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements</typeparam>  
    public class DispatchingObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>  
        /// The default constructor of the ObservableCollection  
        /// </summary>  
        public DispatchingObservableCollection()
        {
            //Assign the current Dispatcher (owner of the collection)  
            _currentDispatcher = Dispatcher.CurrentDispatcher;
        }

        private readonly Dispatcher _currentDispatcher;

        /// <summary>  
        /// Executes this action in the right thread  
        /// </summary>  
        ///<param name="action">The action which should be executed</param>  
        private void DoDispatchedAction(Action action)
        {
            if (_currentDispatcher.CheckAccess())
                action.Invoke();
            else
                _currentDispatcher.Invoke(DispatcherPriority.DataBind, action);
        }

        /// <summary>  
        /// Clears all items  
        /// </summary>  
        protected override void ClearItems()
        {
            DoDispatchedAction(BaseClearItems);
        }

        private void BaseClearItems()
        {
            base.ClearItems();
        }

        /// <summary>  
        /// Inserts a item at the specified index  
        /// </summary>  
        ///<param name="index">The index where the item should be inserted</param>  
        ///<param name="item">The item which should be inserted</param>  
        protected override void InsertItem(int index, T item)
        {
            DoDispatchedAction(() => BaseInsertItem(index, item));
        }

        private void BaseInsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }

        /// <summary>  
        /// Moves an item from one index to another  
        /// </summary>  
        ///<param name="oldIndex">The index of the item which should be moved</param>  
        ///<param name="newIndex">The index where the item should be moved</param>  
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            DoDispatchedAction(() => BaseMoveItem(oldIndex, newIndex));
        }

        private void BaseMoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        /// <summary>  
        /// Removes the item at the specified index  
        /// </summary>  
        ///<param name="index">The index of the item which should be removed</param>  
        protected override void RemoveItem(int index)
        {
            DoDispatchedAction(() => BaseRemoveItem(index));
        }

        private void BaseRemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        /// <summary>  
        /// Sets the item at the specified index  
        /// </summary>  
        ///<param name="index">The index which should be set</param>  
        ///<param name="item">The new item</param>  
        protected override void SetItem(int index, T item)
        {
            DoDispatchedAction(() => BaseSetItem(index, item));
        }

        private void BaseSetItem(int index, T item)
        {
            base.SetItem(index, item);
        }

        /// <summary>  
        /// Fires the CollectionChanged Event  
        /// </summary>  
        ///<param name="e">The additional arguments of the event</param>  
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DoDispatchedAction(() => BaseOnCollectionChanged(e));
        }

        private void BaseOnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }

        /// <summary>  
        /// Fires the PropertyChanged Event  
        /// </summary>  
        ///<param name="e">The additional arguments of the event</param>  
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            DoDispatchedAction(() => BaseOnPropertyChanged(e));
        }

        private void BaseOnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}
