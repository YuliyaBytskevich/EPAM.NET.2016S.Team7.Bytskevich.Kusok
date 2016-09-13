using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class OperationsCollection
    {
        private List<ToDoOperation> _operations;
        private static OperationsCollection _instance;
        private readonly string _fileName;
        private XmlSerializer _formatter = new XmlSerializer(typeof(List<ToDoOperation>));

        public static OperationsCollection GetInstance()
        {
            return _instance ?? (_instance = new OperationsCollection());
        }

        private OperationsCollection()
        {
            _operations = new List<ToDoOperation>();
            _fileName = "";
            // _fileName = ConfigurationManager.AppSettings["LocalFilePath"];
        }

        public void Add(ToDoItemViewModel item, Operation operation)
        {
            _operations.Add(new ToDoOperation() { Item = item, Operation = operation });
            using (FileStream s = File.OpenWrite(_fileName))
            {
                _formatter.Serialize(s, _operations);
            }
        }

        public void Delete(ToDoItemViewModel item, Operation operation)
        {
            _operations.RemoveAll(x => x.Item == item && x.Operation == operation);
            using (FileStream s = File.OpenWrite(_fileName))
            {
                _formatter.Serialize(s, _operations);
            }
        }

        public void RefreshLocalFile()
        {
            using (FileStream s = File.Create(_fileName))
            {
                _formatter.Serialize(s, _operations);
            }
        }

        public void LoadFromLocalFile()
        {
            using (Stream s = new FileStream(_fileName, FileMode.Open))
            {
                _operations = (List<ToDoOperation>)_formatter.Deserialize(s);
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