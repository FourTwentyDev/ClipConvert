using System;
using System.Windows.Input;

namespace FileConvertor.UI.Commands
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Event that is raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">Action to execute</param>
        /// <param name="canExecute">Predicate to determine if the command can be executed</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines if the command can be executed
        /// </summary>
        /// <param name="parameter">Parameter for the command</param>
        /// <returns>True if the command can be executed, false otherwise</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">Parameter for the command</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// A generic command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    /// <typeparam name="T">Type of the command parameter</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// Event that is raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">Action to execute</param>
        /// <param name="canExecute">Predicate to determine if the command can be executed</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines if the command can be executed
        /// </summary>
        /// <param name="parameter">Parameter for the command</param>
        /// <returns>True if the command can be executed, false otherwise</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter is T t ? t : default);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">Parameter for the command</param>
        public void Execute(object? parameter)
        {
            _execute(parameter is T t ? t : default);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
