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

namespace ToDoClient.Infrastructure
{
    public class OperationsCollection
    {
        private List<ToDoOperation> _items;
        private static OperationsCollection _instance;
        private readonly string _fileName;
        private readonly XmlSerializer _formatter = new XmlSerializer(typeof(List<ToDoOperation>));

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

        public void Add(ToDoItemViewModel item, Operation operation)
        {
            if (operation == Operation.Update)
            {
                int countRemovedItems =
                    _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Create);
                if (countRemovedItems > 0)
                {
                    _items.Add(new ToDoOperation() {Item = item, Operation = Operation.Create});
                }
                else
                {
                    _items.Add(new ToDoOperation() { Item = item, Operation = Operation.Update });
                }
            }
            else if (operation == Operation.Delete)
            {
                int countRemovedItems = _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId); // x.Operation == Operation.Create || Operations.Update
                if (countRemovedItems == 0)
                {
                    _items.Add(new ToDoOperation() { Item = item, Operation = Operation.Delete });
                }
            }
            else
            {
                _items.Add(new ToDoOperation() {Item = item, Operation = operation});
            }
            RefreshLocalFile(_items);
        }

        private void RemoveCommandFromFile(ToDoOperation command)
        {
            var actual = _items.Except(new List<ToDoOperation>() { command });
            RefreshLocalFile(actual.ToList());
        }

        public void Sync(ToDoService remote)
        {
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
                RemoveCommandFromFile(command);
            }
            var commandsInFile = File.ReadAllText(_fileName);
            if (!string.IsNullOrEmpty(commandsInFile))
            {
                LoadCommandsFromLocalFile();
            }
        }

        public bool IsNotEmpty()
        {
            return _items.Any();
        }

        private void RefreshLocalFile(List<ToDoOperation> actialList)
        {
            using (FileStream s = File.Create(_fileName))
            {
                _formatter.Serialize(s, actialList);
            }
        }

        public void LoadCommandsFromLocalFile()
        {
            using (Stream s = new FileStream(_fileName, FileMode.Open))
            {
                _items = (List<ToDoOperation>)_formatter.Deserialize(s);
            }
        }
        
        [Serializable]
        public struct ToDoOperation
        {
            public ToDoItemViewModel Item;
            public Operation Operation;
        }
    }

    public enum Operation
    {
        GetAll,
        Create,
        Update,
        Delete
    }
}