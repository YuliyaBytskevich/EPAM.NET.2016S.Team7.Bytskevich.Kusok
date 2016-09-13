using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class ToDosCollection
    {  
        public List<ToDoItemViewModel> Items { get; set; } 
        private static ToDosCollection instance;

        public static ToDosCollection GetInstance()
        {
            return instance ?? (instance = new ToDosCollection());
        }

        private ToDosCollection()
        {
            Items = new List<ToDoItemViewModel>();
        }
    }
   
}