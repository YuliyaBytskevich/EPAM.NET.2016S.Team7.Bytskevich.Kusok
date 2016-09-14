using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Xml.Serialization;
using ToDoClient.Models;
using ToDoClient.Services;
using System.Threading;

namespace ToDoClient.Infrastructure
{
    public class OperationsCollection
    {
        private List<ToDoOperation> _items;
        private List<ToDoOperation> _actualCopy;
        private static OperationsCollection _instance;
        private readonly string _fileName;
        private readonly XmlSerializer _formatter = new XmlSerializer(typeof(List<ToDoOperation>));
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _refreshFileLock = new ReaderWriterLockSlim();

        public static OperationsCollection GetInstance()
        {
            return _instance ?? (_instance = new OperationsCollection());
        }

        private OperationsCollection()
        {
            _items = new List<ToDoOperation>();
            // ACHTUNG! HARDCODE'S HERE!
            _fileName = "E:/operations.xml";
        }

        /// <summary>
        /// Adds a new command to the collection
        /// </summary>
        /// <param name="item">The todo item.</param>
        /// <param name="operation">The operation.</param>
        public void Add(ToDoItemViewModel item, Operation operation)
        {
            this._readerWriterLock.EnterWriteLock();
            try
            {
                if (operation == Operation.Update)
                {
                    int numOfUnnessesaryCreates = _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Create);
                    _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Update);
                    _items.Add(numOfUnnessesaryCreates > 0
                        ? new ToDoOperation() {Item = item, Operation = Operation.Create}
                        : new ToDoOperation() {Item = item, Operation = Operation.Update});                   
                }
                else if (operation == Operation.Delete)
                {
                    int countRemovedItems = this._items.RemoveAll(x => x.Item.ToDoId == item.ToDoId); // x.Operation == Operation.Create || Operations.Update
                    if (countRemovedItems == 0)
                    {
                        this._items.Add(new ToDoOperation() { Item = item, Operation = Operation.Delete });
                    }
                }
                else
                {
                    this._items.Add(new ToDoOperation() { Item = item, Operation = operation });
                }
                RefreshLocalFile(_items);
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sinchronization with remote cloud.
        /// </summary>
        /// <param name="remote">Service that processes requests to a remote cloud.</param>
        public void Sync(ToDoService remote)
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                var temp = new ToDoOperation[_items.Count];
                _items.CopyTo(temp);
                _actualCopy = temp.ToList();
                foreach (var command in _items)
                {
                    switch (command.Operation)
                    {
                        case Operation.Create:
                            remote.CreateItem(command.Item);
                            break;
                        case Operation.Update:
                            remote.UpdateItem(command.Item);
                            break;
                        case Operation.Delete:
                            remote.DeleteItem(command.Item.ToDoId);
                            break;
                    }
                    _actualCopy = _actualCopy.Except(new List<ToDoOperation>() { command }).ToList();
                    RefreshLocalFile(_actualCopy);
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public bool IsNotEmpty()
        {
            return _items.Any();
        }

        /// <summary>
        /// Refreshes the local file according to actual operations collection state
        /// </summary>
        private void RefreshLocalFile(List<ToDoOperation> actialList)
        {
            using (FileStream s = File.Create(_fileName))
            {
                _formatter.Serialize(s, actialList);
            }
        }

        /// <summary>
        /// Loads from local file.
        /// </summary>
        public void LoadCommandsFromLocalFile()
        {
            this._readerWriterLock.EnterWriteLock();
            try
            {
                if (File.Exists(_fileName))
                {
                    using (Stream s = new FileStream(this._fileName, FileMode.Open))
                    {
                        this._items = (List<ToDoOperation>) this._formatter.Deserialize(s);
                    }
                }
            }
            finally
            {
                this._readerWriterLock.ExitWriteLock();
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