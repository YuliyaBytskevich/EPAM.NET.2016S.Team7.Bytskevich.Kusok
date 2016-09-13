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
        private int currentId = 0;

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
                localCollection.Items = todoService.GetItems(userId).ToList();
            }
            return localCollection.Items;
        }

        /// <summary>
        /// Updates the existing todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to update.</param>
        public void Put(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            var toBeEdited = localCollection.Items.Find(x => x.ToDoId == todo.ToDoId);
            toBeEdited.IsCompleted = todo.IsCompleted;
            toBeEdited.Name = todo.Name;
            // todoService.UpdateItem(todo);
            // TODO: update item in local storage
        }

        /// <summary>
        /// Deletes the specified todo-item.
        /// </summary>
        /// <param name="id">The todo item identifier.</param>
        public void Delete(int id)
        {
            localCollection.Items.RemoveAll(x => x.ToDoId == id);
            //todoService.DeleteItem(id);
            // TODO: delete from local storage
        }

        /// <summary>
        /// Creates a new todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to create.</param>
        public void Post(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            localCollection.Items.Add(todo);
            //todoService.CreateItem(todo);
            // TODO: create new item in local storage
        }
    }
}

