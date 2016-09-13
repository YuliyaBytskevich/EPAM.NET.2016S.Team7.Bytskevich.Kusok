using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    /// <summary>
    /// Works with list of operations from local collection
    /// </summary>
    public class OperationsCollection
    {
        private static OperationsCollection instance;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private readonly string fileName;
        private List<ToDoOperation> items;
        private XmlSerializer formatter = new XmlSerializer(typeof(List<ToDoOperation>));

        private OperationsCollection()
        {
            this.items = new List<ToDoOperation>();
            this.fileName = @"D:\operations.xml";
            // _fileName = ConfigurationManager.AppSettings["LocalFilePath"];
        }

        /// <summary>
        /// Gets the instance of OperationsCollection.
        /// </summary>
        /// <returns>The instance of OperationsCollection</returns>
        public static OperationsCollection GetInstance()
        {
            return instance ?? (instance = new OperationsCollection());
        }

        /// <summary>
        /// Adds a operation.
        /// </summary>
        /// <param name="item">The todo item.</param>
        /// <param name="operation">The operation.</param>
        public void Add(ToDoItemViewModel item, Operation operation)
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                if (operation == Operation.Update)
                {
                    int countRemovedItems =
                        this.items.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Create);
                    if (countRemovedItems > 0)
                    {
                        this.items.Add(new ToDoOperation() { Item = item, Operation = Operation.Create });
                    }
                    else
                    {
                        this.items.Add(new ToDoOperation() { Item = item, Operation = Operation.Update });
                    }
                }
                else if (operation == Operation.Delete)
                {
                    int countRemovedItems = this.items.RemoveAll(x => x.Item.ToDoId == item.ToDoId); // x.Operation == Operation.Create || Operations.Update
                    if (countRemovedItems == 0)
                    {
                        this.items.Add(new ToDoOperation() { Item = item, Operation = Operation.Delete });
                    }
                }
                else
                {
                    this.items.Add(new ToDoOperation() { Item = item, Operation = operation });
                }
                using (FileStream s = File.OpenWrite(this.fileName))
                {
                    this.formatter.Serialize(s, this.items);
                }
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The todo item to delete.</param>
        /// <param name="operation">The operation to delete.</param>
        public void Delete(ToDoItemViewModel item, Operation operation)
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                this.items.RemoveAll(x => x.Item == item && x.Operation == operation);
                using (FileStream s = File.OpenWrite(this.fileName))
                {
                    this.formatter.Serialize(s, this.items);
                }
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Refreshes the local file.
        /// </summary>
        public void RefreshLocalFile()
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                using (FileStream s = File.Create(this.fileName))
                {
                    this.formatter.Serialize(s, this.items);
                }
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Loads from local file.
        /// </summary>
        public void LoadFromLocalFile()
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                using (Stream s = new FileStream(this.fileName, FileMode.Open))
                {
                    this.items = (List<ToDoOperation>)this.formatter.Deserialize(s);
                }
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Represent todo item with operation. Uses in list.
        /// </summary>
        [Serializable]
        public struct ToDoOperation
        {
            public ToDoItemViewModel Item;
            public Operation Operation;
        }
    }
}