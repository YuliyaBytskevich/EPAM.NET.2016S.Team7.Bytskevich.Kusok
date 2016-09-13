using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class OperationsCollection
    {
       
    }

    public struct ToDoOperation
    {
        public ToDoItemViewModel Item;
        public Operation Operation;
    }

    public enum Operation
    {
        GetAll,
        Create,
        Update,
        Delete
    }
}