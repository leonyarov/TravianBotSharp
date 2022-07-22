﻿using MainCore.Models.Runtime;
using MainCore.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFUI.Models;

namespace WPFUI.ViewModels.Tabs
{
    public class DebugViewModel : ReactiveObject
    {
        public DebugViewModel()
        {
            _taskManager = App.GetService<ITaskManager>();
            _logManager = App.GetService<ILogManager>();
            _databaseEvent = App.GetService<IDatabaseEvent>();

            _databaseEvent.AccountSelected += OnAccountSelected;
            _databaseEvent.TaskUpdated += OnTasksUpdate;
            _databaseEvent.LogUpdated += OnLogsUpdate;
        }

        private void OnAccountSelected(int accountId)
        {
            _accountId = accountId;
            OnTasksUpdate(accountId);
            OnLogsUpdate(accountId);
        }

        private async void OnTasksUpdate(int accountId)
        {
            if (!Active) return;
            if (_accountId != accountId) return;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Tasks.Clear();
                foreach (var item in _taskManager.GetTaskList(accountId))
                {
                    Tasks.Add(new TaskModel()
                    {
                        Task = item.Name,
                        ExecuteAt = item.ExecuteAt,
                        Stage = item.Stage,
                    });
                }
            });
        }

        private async void OnLogsUpdate(int accountId)
        {
            if (!Active) return;

            if (_accountId != accountId) return;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Logs.Clear();
                foreach (var item in _logManager.GetLog(accountId))
                {
                    Logs.Add(item);
                }
            });
        }

        private int _accountId = -1;

        private readonly ILogManager _logManager;
        private readonly ITaskManager _taskManager;
        private readonly IDatabaseEvent _databaseEvent;

        public ObservableCollection<TaskModel> Tasks { get; } = new();

        public ObservableCollection<LogMessage> Logs { get; } = new();

        public bool Active { get; set; }
    }
}