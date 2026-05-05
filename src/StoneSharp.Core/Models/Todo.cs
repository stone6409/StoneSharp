using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoneSharp.Core.Models
{
    /// <summary>
    /// 表示一个待办事项
    /// </summary>
    public class Todo
    {
        /// <summary>
        /// 待办事项的唯一标识符
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// 待办事项的内容（命令式形式，如"Run tests"）
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// 进行时形式（如"Running tests"）
        /// </summary>
        [JsonPropertyName("activeForm")]
        public string ActiveForm { get; set; }

        /// <summary>
        /// 待办事项状态
        /// </summary>
        [JsonPropertyName("status")]
        public TodoStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Todo()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = TodoStatus.Pending;
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        /// <param name="content">待办事项内容</param>
        /// <param name="activeForm">进行时形式</param>
        public Todo(string content, string activeForm) : this()
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ActiveForm = activeForm ?? throw new ArgumentNullException(nameof(activeForm));
        }

        /// <summary>
        /// 更新待办事项状态
        /// </summary>
        /// <param name="status">新状态</param>
        public void UpdateStatus(TodoStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 更新待办事项内容
        /// </summary>
        /// <param name="content">新内容</param>
        /// <param name="activeForm">新进行时形式</param>
        public void UpdateContent(string content, string activeForm)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ActiveForm = activeForm ?? throw new ArgumentNullException(nameof(activeForm));
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 检查待办事项是否已完成
        /// </summary>
        /// <returns>如果状态为Completed则返回true</returns>
        public bool IsCompleted() => Status == TodoStatus.Completed;

        /// <summary>
        /// 检查待办事项是否进行中
        /// </summary>
        /// <returns>如果状态为InProgress则返回true</returns>
        public bool IsInProgress() => Status == TodoStatus.InProgress;

        /// <summary>
        /// 检查待办事项是否待处理
        /// </summary>
        /// <returns>如果状态为Pending则返回true</returns>
        public bool IsPending() => Status == TodoStatus.Pending;
    }

    /// <summary>
    /// 待办事项状态枚举
    /// </summary>
    public enum TodoStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        [JsonPropertyName("pending")]
        Pending,

        /// <summary>
        /// 进行中
        /// </summary>
        [JsonPropertyName("in_progress")]
        InProgress,

        /// <summary>
        /// 已完成
        /// </summary>
        [JsonPropertyName("completed")]
        Completed
    }

    /// <summary>
    /// 待办事项列表
    /// </summary>
    public class TodoList : List<Todo>
    {
        /// <summary>
        /// 获取所有待处理的待办事项
        /// </summary>
        public IEnumerable<Todo> PendingTodos => this.Where(t => t.Status == TodoStatus.Pending);

        /// <summary>
        /// 获取所有进行中的待办事项
        /// </summary>
        public IEnumerable<Todo> InProgressTodos => this.Where(t => t.Status == TodoStatus.InProgress);

        /// <summary>
        /// 获取所有已完成的待办事项
        /// </summary>
        public IEnumerable<Todo> CompletedTodos => this.Where(t => t.Status == TodoStatus.Completed);

        /// <summary>
        /// 检查是否所有待办事项都已完成
        /// </summary>
        public bool AllCompleted => this.All(t => t.Status == TodoStatus.Completed);

        /// <summary>
        /// 检查是否有进行中的待办事项
        /// </summary>
        public bool HasInProgress => this.Any(t => t.Status == TodoStatus.InProgress);

        /// <summary>
        /// 获取进行中的待办事项数量
        /// </summary>
        public int InProgressCount => this.Count(t => t.Status == TodoStatus.InProgress);

        /// <summary>
        /// 获取待处理的待办事项数量
        /// </summary>
        public int PendingCount => this.Count(t => t.Status == TodoStatus.Pending);

        /// <summary>
        /// 获取已完成的待办事项数量
        /// </summary>
        public int CompletedCount => this.Count(t => t.Status == TodoStatus.Completed);

        /// <summary>
        /// 根据ID查找待办事项
        /// </summary>
        /// <param name="id">待办事项ID</param>
        /// <returns>找到的待办事项，如果未找到则返回null</returns>
        public Todo FindById(string id) => this.FirstOrDefault(t => t.Id == id);

        /// <summary>
        /// 根据内容查找待办事项
        /// </summary>
        /// <param name="content">待办事项内容</param>
        /// <returns>找到的待办事项，如果未找到则返回null</returns>
        public Todo FindByContent(string content) => this.FirstOrDefault(t => t.Content == content);

        /// <summary>
        /// 开始处理一个待办事项
        /// </summary>
        /// <param name="id">待办事项ID</param>
        /// <returns>如果成功开始则返回true</returns>
        public bool StartTodo(string id)
        {
            var todo = FindById(id);
            if (todo == null || todo.Status != TodoStatus.Pending)
                return false;

            // 确保只有一个进行中的待办事项
            foreach (var item in InProgressTodos)
            {
                item.UpdateStatus(TodoStatus.Pending);
            }

            todo.UpdateStatus(TodoStatus.InProgress);
            return true;
        }

        /// <summary>
        /// 完成一个待办事项
        /// </summary>
        /// <param name="id">待办事项ID</param>
        /// <returns>如果成功完成则返回true</returns>
        public bool CompleteTodo(string id)
        {
            var todo = FindById(id);
            if (todo == null || todo.Status != TodoStatus.InProgress)
                return false;

            todo.UpdateStatus(TodoStatus.Completed);
            return true;
        }

        /// <summary>
        /// 添加新的待办事项
        /// </summary>
        /// <param name="content">待办事项内容</param>
        /// <param name="activeForm">进行时形式</param>
        /// <returns>新创建的待办事项</returns>
        public Todo AddTodo(string content, string activeForm)
        {
            var todo = new Todo(content, activeForm);
            Add(todo);
            return todo;
        }

        /// <summary>
        /// 移除待办事项
        /// </summary>
        /// <param name="id">待办事项ID</param>
        /// <returns>如果成功移除则返回true</returns>
        public bool RemoveTodo(string id)
        {
            var todo = FindById(id);
            if (todo == null)
                return false;

            return Remove(todo);
        }

        /// <summary>
        /// 清除所有已完成的待办事项
        /// </summary>
        public void ClearCompleted()
        {
            RemoveAll(t => t.Status == TodoStatus.Completed);
        }

        /// <summary>
        /// 获取下一个待处理的待办事项
        /// </summary>
        /// <returns>下一个待处理的待办事项，如果没有则返回null</returns>
        public Todo GetNextPendingTodo() => PendingTodos.FirstOrDefault();

        /// <summary>
        /// 获取当前进行中的待办事项
        /// </summary>
        /// <returns>当前进行中的待办事项，如果没有则返回null</returns>
        public Todo GetCurrentInProgressTodo() => InProgressTodos.FirstOrDefault();
    }
}