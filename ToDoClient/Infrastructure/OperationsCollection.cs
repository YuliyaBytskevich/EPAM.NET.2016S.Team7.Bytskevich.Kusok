using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Xml.Serialization;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class OperationsCollection
    {
        private List<ToDoOperation> _items;
        private static OperationsCollection _instance;
        private readonly string _fileName;
        private XmlSerializer _formatter = new XmlSerializer(typeof(List<ToDoOperation>));

        public static OperationsCollection GetInstance()
        {
            return _instance ?? (_instance = new OperationsCollection());
        }

        private OperationsCollection()
        {
            _items = new List<ToDoOperation>();
            _fileName = "operations.xml";
            // _fileName = ConfigurationManager.AppSettings["LocalFilePath"];
        }

        public void Add(ToDoItemViewModel item, Operation operation)
        {
            if (operation == Operation.Update)
            {
                int countRemovedItems =
                    _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Create);
                if (countRemovedItems > 0)
                {
                    operation = Operation.Create;
                }
            }
            if (operation == Operation.Delete)
            {
                _items.RemoveAll(x => x.Item.ToDoId == item.ToDoId); // x.Operation == Operation.Create || Operations.Update
            }
            _items.Add(new ToDoOperation() {Item = item, Operation = operation});


            using (FileStream s = File.OpenWrite(_fileName))
            {
                _formatter.Serialize(s, _items);
            }
        }

        public void Delete(ToDoItemViewModel item, Operation operation)
        {
            _items.RemoveAll(x => x.Item == item && x.Operation == operation);
            using (FileStream s = File.OpenWrite(_fileName))
            {
                _formatter.Serialize(s, _items);
            }
        }

        public void RefreshLocalFile()
        {
            using (FileStream s = File.Create(_fileName))
            {
                _formatter.Serialize(s, _items);
            }
        }

        public void LoadFromLocalFile()
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