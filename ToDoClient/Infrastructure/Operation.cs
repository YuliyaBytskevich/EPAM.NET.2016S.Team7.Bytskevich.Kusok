using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoClient.Infrastructure
{
    /// <summary>
    /// Enumerates the allowable operations
    /// </summary>
    public enum Operation
    {
        GetAll,
        Create,
        Update,
        Delete
    }
}