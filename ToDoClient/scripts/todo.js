var tasksManager = function() {

    var isFirstLoad = true;

    // appends a row to the tasks table.
    // @parentSelector: selector to append a row to.
    // @obj: task object to append.
    var appendRow = function(parentSelector, obj) {
        var tr = $("<tr data-id='" + obj.ToDoId + "'></tr>");
        tr.append("<td><input type='checkbox' class='completed' " + (obj.IsCompleted ? "checked" : "") + "/></td>");
        tr.append("<td class='name' >" + obj.Name + "</td>");
        tr.append("<td><button class='delete'>Delete</button></td>");
        $(parentSelector).append(tr);
    }

    // adds all tasks as rows (deletes all rows before).
    // @parentSelector: selector to append a row to.
    // @tasks: array of tasks to append.
    var displayTasks = function (parentSelector, tasks) {
        $(parentSelector).empty();
        $.each(tasks, function(i, item) {
            appendRow(parentSelector, item);
        });
        $("#waitLoading").fadeOut();
    };

    // starts loading tasks from server.
    // @returns a promise.
    var loadTasks = function () {
        if (isFirstLoad) {
            $("#waitLoading").fadeIn();
            isFirstLoad = false;
        }
        return $.getJSON("/api/todos");
    };

    // starts creating a task on the server.
    // @isCompleted: indicates if new task should be completed.
    // @name: name of new task.
    // @return a promise.
    var createTask = function(isCompleted, name) {
        return $.post("/api/todos",
        {
            IsCompleted: isCompleted,
            Name: name
        });
    };

    // starts updating a task on the server.
    // @id: id of the task to update.
    // @isCompleted: indicates if the task should be completed.
    // @name: name of the task.
    // @return a promise.
    var updateTask = function(id, isCompleted, name) {
        return $.ajax(
        {
            url: "/api/todos",
            type: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                ToDoId: id,
                IsCompleted: isCompleted,
                Name: name
            })
        });
    };

    // starts deleting a task on the server.
    // @taskId: id of the task to delete.
    // @return a promise.
    var deleteTask = function (taskId) {
        return $.ajax({
            url: "/api/todos/" + taskId,
            type: "DELETE"
        });
    };

    var syncTasks = function (button) {
        $("#waitSyncing").fadeIn();
        $("#sync").text("Syncing ...");
        $("#sync").prop("disabled", true);
        return $.ajax({
            url: "/api/todos/",
            type: "PATCH"
        });
    }

    // returns public interface of task manager.
    return {
        loadTasks: loadTasks,
        displayTasks: displayTasks,
        createTask: createTask,
        deleteTask: deleteTask,
        updateTask: updateTask,
        syncTasks: syncTasks
    };
}();


$(function () {

    var $createButton = $("#newCreate");
    var $nameInput = $("#newName");
    var $todosTable = $("#tasks > tbody");
    var $syncButton = $("#sync");
    var $counter = $(".global-container p");
    var commandsCounter = 0;

    // add new task button click handler
    $createButton.click(function () {
        var name = $nameInput[0].value;

        tasksManager.createTask(false, name)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
                commandsCounter++;
                $counter.text("unsynced changes: " + commandsCounter);
            });
    });

    // bind update task checkbox click handler
    $todosTable.on("change", ".completed", function () {
        var tr = $(this).parent().parent();
        var taskId = tr.attr("data-id");
        var isCompleted = tr.find(".completed")[0].checked;
        var name = tr.find(".name").text();
        
        tasksManager.updateTask(taskId, isCompleted, name)
            .then(tasksManager.loadTasks)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
                commandsCounter++;
                $counter.text("unsynced changes: " + commandsCounter);
            });
    });

    // bind delete button click for future rows
    $todosTable.on("click", ".delete", function () {
        var taskId = $(this).parent().parent().attr("data-id");
        tasksManager.deleteTask(taskId)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
                commandsCounter++;
                $counter.text("unsynced changes: " + commandsCounter);
            });
    });

    $syncButton.click(function () {
        tasksManager.syncTasks()
            .done(function() {
                $("#waitSyncing").fadeOut();
                $("#sync").text("Synchronize");
                $syncButton.prop("disabled", false);
                commandsCounter = 0;
                $counter.text("unsynced changes: 0");
            });
    });

    // load all tasks on startup
    tasksManager.loadTasks()
        .done(function (tasks) {
            tasksManager.displayTasks("#tasks > tbody", tasks); 
        });
});