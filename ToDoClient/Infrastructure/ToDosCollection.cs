using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class ToDosCollection
    {
        private List<ToDoItemViewModel> _items;
        private static ToDosCollection _instance;
        private int _lastId;

        public static ToDosCollection GetInstance()
        {
            return _instance ?? (_instance = new ToDosCollection());
        }

        private ToDosCollection()
        {
            _items = new List<ToDoItemViewModel>();
            _lastId = 0;
        }

        public IList<ToDoItemViewModel> GetAll()
        {
            return _items;
        }

        public void Load(IList<ToDoItemViewModel> items)
        {
            var existItems = _items;
            _items = items.ToList();
            if (_items.Count > 0)
            {
                _lastId = _items.Max(x => x.ToDoId);
            }
            foreach (var existItem in existItems)
            {
                this.Add(existItem);
            }
        }

        public void Add(ToDoItemViewModel item)
        {
            _lastId++;
            item.ToDoId = _lastId;
            _items.Add(item);
        }

        public void Delete(int id)
        {
            _items.RemoveAll(x => x.ToDoId == id);
        }

        public void Update(ToDoItemViewModel item)
        {
            var toBeEdited = _items.Find(x => x.ToDoId == item.ToDoId);
            toBeEdited.IsCompleted = item.IsCompleted;
            toBeEdited.Name = item.Name;
        }
    }
}