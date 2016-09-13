using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using ToDoClient.Infrastructure;
using ToDoClient.Models;
using ToDoClient.Services;

namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes todo requests.
    /// </summary>
    public class ToDosController : ApiController
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();
        private ToDosCollection localCollection = ToDosCollection.GetInstance();
        private OperationsCollection operationsCollection = OperationsCollection.GetInstance();

        /// <summary>
        /// Returns all todo-items for the current user.
        /// </summary>
        /// <returns>The list of todo-items.</returns>
        public IList<ToDoItemViewModel> Get()
        {
            var userId = userService.GetOrCreateUser();
            if (CallsSwitcher.IsFirstCallToGet)
            {
                CallsSwitcher.IsFirstCallToGet = false;
                localCollection.Load(todoService.GetItems(userId).ToList());
            }
            return localCollection.GetAll();
        }

        /// <summary>
        /// Updates the existing todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to update.</param>
        public void Put(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            localCollection.Update(todo);
            operationsCollection.Add(todo, Operation.Update);
        }

        /// <summary>
        /// Deletes the specified todo-item.
        /// </summary>
        /// <param name="id">The todo item identifier.</param>
        public void Delete(int id)
        {
            localCollection.Delete(id);
            operationsCollection.Add(new ToDoItemViewModel() {ToDoId = id}, Operation.Delete);
        }

        /// <summary>
        /// Creates a new todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to create.</param>
        public void Post(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            localCollection.Add(todo);
            operationsCollection.Add(todo, Operation.Create);
        }

        public void Patch()
        {
            Debug.WriteLine(">>> PATCH called");
            if (operationsCollection.IsNotEmpty())
            {
                operationsCollection.Sync(todoService);
            }          
        }
    }
}

