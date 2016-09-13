using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class ToDosCollection
    {
        private List<ToDoItemViewModel> _items;
        private static ToDosCollection _instance;
        private int _lastId;
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public static ToDosCollection GetInstance()
        {
            return _instance ?? (_instance = new ToDosCollection());
        }

        /// <summary>
        /// Gets all todos.
        /// </summary>
        /// <returns>The list of todos.</returns>
        public IList<ToDoItemViewModel> GetAll()
        {
            this._readerWriterLock.EnterReadLock();
            try
            {
                return this._items;
            }
            finally
            {
                this._readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Loads the specified todo items in local todos' collection.
        /// </summary>
        /// <param name="loadItems">The list of todo items.</param>
        public void Load(IList<ToDoItemViewModel> loadItems)
        {
            List<ToDoItemViewModel> existItems;

            this._readerWriterLock.EnterWriteLock();
            try
            {
                existItems = this._items;
                this._items = loadItems.ToList();
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
            }

            this._readerWriterLock.EnterReadLock();
            try
            {
                if (this._items.Count > 0)
                {
                    this._lastId = this._items.Max(x => x.ToDoId);
                }
            }
            finally
            {
                this._readerWriterLock.ExitReadLock();
            }

            foreach (var existItem in existItems)
            {
                this.Add(existItem);
            }
        }

        /// <summary>
        /// Adds a todo.
        /// </summary>
        /// <param name="item">The todo to add.</param>
        public void Add(ToDoItemViewModel item)
        {
            this._readerWriterLock.EnterWriteLock();
            try
            {
                this._lastId++;
                item.ToDoId = this._lastId;
                this._items.Add(item);
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Deletes a todo.
        /// </summary>
        /// <param name="id">The todo ID to delete.</param>
        public void Delete(int id)
        {
            this._readerWriterLock.EnterWriteLock();
            try
            {
                this._items.RemoveAll(x => x.ToDoId == id);
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates a todo.
        /// </summary>
        /// <param name="item">The todo to update.</param>
        public void Update(ToDoItemViewModel item)
        {
            this._readerWriterLock.EnterWriteLock();
            try
            {
                var toBeEdited = this._items.Find(x => x.ToDoId == item.ToDoId);
                toBeEdited.IsCompleted = item.IsCompleted;
                toBeEdited.Name = item.Name;
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
            }
        }
    }
}