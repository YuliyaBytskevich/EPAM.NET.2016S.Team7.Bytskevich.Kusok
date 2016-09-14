using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Xml.Serialization;
using ToDoClient.Models;
using ToDoClient.Services;
using System.Threading;

namespace ToDoClient.Infrastructure
{
    public class CommandsCollection
    {

        private static CommandsCollection instance;
        private List<Command> commands;
        private readonly string fileName;
        private readonly string tempFileName;
        private readonly XmlSerializer formatter = new XmlSerializer(typeof(List<Command>));
        private readonly ReaderWriterLockSlim modifyCommandsLock = new ReaderWriterLockSlim();

        private CommandsCollection()
        {
            commands = new List<Command>();
            fileName = "E:/operations.xml"; // ACHTUNG! HARDCODE'S HERE!
            tempFileName = "E:/operations_temp.xml"; // ACHTUNG! HARDCODE'S HERE!
        }

        public static CommandsCollection GetInstance()
        {
            return instance ?? (instance = new CommandsCollection());
        }

        /// <summary>
        /// Adds a new command to the collection
        /// </summary>
        /// <param name="item">The todo item.</param>
        /// <param name="operation">The operation.</param>
        public void Add(ToDoItemViewModel item, Operation operation)
        {
            modifyCommandsLock.EnterWriteLock();
            try
            {
                switch (operation)
                {
                    case Operation.Update:
                        int numOfUnnessesaryCreates = commands.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Create);
                        commands.RemoveAll(x => x.Item.ToDoId == item.ToDoId && x.Operation == Operation.Update);
                        commands.Add(numOfUnnessesaryCreates > 0
                            ? new Command() {Item = item, Operation = Operation.Create}
                            : new Command() {Item = item, Operation = Operation.Update});
                        break;
                    case Operation.Delete:
                        int countRemovedItems = this.commands.RemoveAll(x => x.Item.ToDoId == item.ToDoId); 
                        if (countRemovedItems == 0)
                        {
                            commands.Add(new Command() { Item = item, Operation = Operation.Delete });
                        }
                        break;
                    default:
                        commands.Add(new Command() { Item = item, Operation = operation });
                        break;
                }
                RefreshLocalFile(fileName, commands);
            }
            finally
            {
                modifyCommandsLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sinchronization with remote cloud.
        /// </summary>
        /// <param name="remote">Service that processes requests to a remote cloud.</param>
        public void Sync(ToDoService remote)
        {
            var commandsToBeSynced = commands.Select(x => x.Clone()).ToList();
            var remainingCommands = commandsToBeSynced.Select(x => x.Clone()).ToList();
            File.Copy(fileName, tempFileName, true);
            commands.Clear();
            RefreshLocalFile(fileName, commands);
            foreach (var command in commandsToBeSynced)
            {
                switch (command.Operation)
                {
                    case Operation.Create:
                        remote.CreateItem(command.Item);
                        break;
                    case Operation.Update:
                        remote.UpdateItem(command.Item);
                        break;
                    case Operation.Delete:
                        remote.DeleteItem(command.Item.ToDoId);
                        break;
                }
                remainingCommands.RemoveAll(x => x.Item.ToDoId == command.Item.ToDoId && x.Operation == command.Operation);
                RefreshLocalFile(tempFileName, remainingCommands);
            }
            var notSyncedCommands = new List<Command>();
            using (Stream s = new FileStream(tempFileName, FileMode.Open))
            {
                notSyncedCommands = (List<Command>)formatter.Deserialize(s);
            }
            if (notSyncedCommands.Count > 0)
            {
                modifyCommandsLock.EnterWriteLock();
                try
                {
                    commands = commands.Concat(notSyncedCommands).ToList();
                    RefreshLocalFile(fileName, commands);
                }
                finally
                {
                    modifyCommandsLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Loads from local file.
        /// </summary>
        public void LoadCommandsFromLocalFile()
        {
            this.modifyCommandsLock.EnterWriteLock();
            try
            {
                if (File.Exists(fileName))
                {
                    using (Stream s = new FileStream(this.fileName, FileMode.Open))
                    {
                        this.commands = (List<Command>)this.formatter.Deserialize(s);
                    }
                }
            }
            finally
            {
                this.modifyCommandsLock.ExitWriteLock();
            }
        }

        public int GetNumberOfUnsyncedCommands()
        {
            return commands.Count();
        }

        public bool IsNotEmpty()
        {
            return commands.Any();
        }

        /// <summary>
        /// Refreshes the local file according to actual operations collection state
        /// </summary>
        private void RefreshLocalFile(string filePath, List<Command> actualList)
        {
            using (FileStream s = File.Create(filePath))
            {
                formatter.Serialize(s, actualList);
            }
        }

    }
}