using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using ToDoClient.Infrastructure;
using ToDoClient.Models;

namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes todo requests.
    /// </summary>
    public class ToDosController : ApiController
    {
        private readonly LocalToDoService localService = LocalToDoService.GetInstance();
        
        /// <summary>
        /// Processes request of getting all todo notes from storage for current user.
        /// </summary>
        /// <returns>The list of to-do items.</returns>
        public IList<ToDoItemViewModel> Get()
        {
            return localService.GetAllToDos();
        }

        /// <summary>
        /// Processes request of making updates on existing todo note.
        /// </summary>
        /// <param name="todo">The to-do item that should be updated.</param>
        public void Put(ToDoItemViewModel todo)
        {
            localService.UpdateToDo(todo);
        }

        /// <summary>
        ///  Processes request of removing the specified todo-item.
        /// </summary>
        /// <param name="id">The todo item identifier.</param>
        public void Delete(ToDoItemViewModel todo)
        {
            localService.DeleteToDo(todo);
        }

        /// <summary>
        ///  Processes request of creating a new todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to create.</param>
        public void Post(ToDoItemViewModel todo)
        {
            localService.CreateNewToDo(todo);
        }

        /// <summary>
        ///  Processes request of synchronization with remote storage
        /// </summary>
        public void Patch()
        {
            localService.SynchronizeWithRemoteStorage();
        }
    }
}



