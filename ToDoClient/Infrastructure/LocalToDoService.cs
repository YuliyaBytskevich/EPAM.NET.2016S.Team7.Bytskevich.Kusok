using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoClient.Models;
using ToDoClient.Services;

namespace ToDoClient.Infrastructure
{
    /// <summary>
    /// Processing calls that work with local collection of todo notes.
    /// </summary>
    public class LocalToDoService
    {
        private static LocalToDoService instance;
        private static bool IsFirstCall = true;

        private readonly UserService userService = new UserService();
        private readonly ToDoService todoService = new ToDoService();
        private readonly ToDosCollection items = ToDosCollection.GetInstance();
        private readonly CommandsCollection commands = CommandsCollection.GetInstance();

        private LocalToDoService() { }

        public static LocalToDoService GetInstance()
        {
            return instance ?? (instance = new LocalToDoService());
        }

        public IList<ToDoItemViewModel> GetAllToDos()
        {
            var userId = userService.GetOrCreateUser();
            if (IsFirstCall)
            {
                IsFirstCall = false;
                items.Load(todoService.GetItems(userId).ToList());
                commands.LoadCommandsFromLocalFile();
            }
            return items.GetAll();
        }

        public void CreateNewToDo(ToDoItemViewModel item)
        {
            item.UserId = userService.GetOrCreateUser();
            items.Add(item);
            commands.Add(item, Operation.Create);
        }

        public void UpdateToDo(ToDoItemViewModel item)
        {
            item.UserId = userService.GetOrCreateUser();
            items.Update(item);
            commands.Add(item, Operation.Update);
        }

        public void DeleteToDo(int id)
        {
            items.Delete(id);
            commands.Add(new ToDoItemViewModel() { ToDoId = id }, Operation.Delete);
        }

        public void SynchronizeWithRemoteStorage()
        {
            if (commands.IsNotEmpty())
            {
                commands.Sync(todoService);
            }
        }
    }
}