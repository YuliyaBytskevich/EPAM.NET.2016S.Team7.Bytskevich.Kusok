using System;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    [Serializable]
    public class Command: ICloneable
    {
        public ToDoItemViewModel Item { get; set; }
        public Operation Operation { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Command Clone()
        {
            var result = new Command
            {
                Item = new ToDoItemViewModel
                {
                    ToDoId = Item.ToDoId,
                    UserId = Item.UserId,
                    IsCompleted = Item.IsCompleted,
                    Name = Item.Name
                },
                Operation = Operation
            };
            return result;
        }
    }
}