﻿using System;
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
        private List<ToDoItemViewModel> items;
        private static ToDosCollection instance;
        private int tempId;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        public static ToDosCollection GetInstance()
        {
            return instance ?? (instance = new ToDosCollection());
        }

        private ToDosCollection()
        {
            items = new List<ToDoItemViewModel>();
            tempId = 0;
        }

        /// <summary>
        /// Gets all todos.
        /// </summary>
        /// <returns>The list of todos.</returns>
        public IList<ToDoItemViewModel> GetAll()
        {
            this.readerWriterLock.EnterReadLock();
            try
            {
                return this.items;
            }
            finally
            {
                this.readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Loads the specified todo items in local todos' collection.
        /// </summary>
        /// <param name="loadItems">The list of todo items.</param>
        public void Load(IList<ToDoItemViewModel> loadItems)
        {
            List<ToDoItemViewModel> existItems;

            this.readerWriterLock.EnterWriteLock();
            try
            {
                existItems = this.items;
                this.items = loadItems.ToList();
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }

            if (existItems == null) return;
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
            this.readerWriterLock.EnterWriteLock();
            try
            {
                item.ToDoId = this.tempId--;
                this.items.Add(item);
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Deletes a todo.
        /// </summary>
        /// <param name="id">The todo ID to delete.</param>
        public void Delete(int id)
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                this.items.RemoveAll(x => x.ToDoId == id);
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates a todo.
        /// </summary>
        /// <param name="item">The todo to update.</param>
        public void Update(ToDoItemViewModel item)
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                var toBeEdited = this.items.Find(x => x.ToDoId == item.ToDoId);
                toBeEdited.IsCompleted = item.IsCompleted;
                toBeEdited.Name = item.Name;
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }
    }
}