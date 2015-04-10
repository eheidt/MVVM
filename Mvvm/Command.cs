using System;

namespace MVVM
{
    public class Command
    {
        public string Text { get; private set; }

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        internal protected Command(string text, Action execute, Func<bool> canExecute = null)
        {
            this.Text = text;
            this._execute = execute;
            this._canExecute = canExecute;

            if (this._canExecute == null)
                this.CanExecute = true;
        }

        public void Execute()
        {
            this._execute();
        }

        public bool CanExecute
        {
            get;
            private set;
        }

        public void InvalidateCanExecute()
        {
            if (this._canExecute != null)
            {
                var canExecute = this._canExecute();

                if (this.CanExecute != canExecute)
                {
                    this.CanExecute = canExecute;

                    var action = this.CanExecuteChanged;
                    if (action != null)
                        action(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }

    internal class BindableCommand : Command, System.Windows.Input.ICommand //The ICommand Dependency is hidden, because we want no UI Dependency in the ViewModels!!!
    {
        public BindableCommand(string text, Action execute, Func<bool> canExecute = null)
            : base(text, execute, canExecute)
        {
        }

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            return this.CanExecute;
        }

        event EventHandler System.Windows.Input.ICommand.CanExecuteChanged
        {
            add
            {
                this.CanExecuteChanged += value;
            }
            remove
            {
                this.CanExecuteChanged -= value;
            }
        }

        void System.Windows.Input.ICommand.Execute(object parameter)
        {
            this.Execute();
        }
    }
}
