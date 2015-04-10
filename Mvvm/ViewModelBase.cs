using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace MVVM
{
    public abstract class ViewModelBase : NotificationObject, IDataErrorInfo
    {
        #region Properties

        private readonly Dictionary<string, object> _propertyCache = new Dictionary<string, object>();
        private readonly Dictionary<string, string> _errorCache = new Dictionary<string, string>();

        protected R GetValue<R>(Expression<Func<R>> property, Func<R> initialValue = null)
        {
            this.VerifyAccess();

            var propertyName = PropertyNameHelper.ExtractName(property);

            object value = null;
            if (_propertyCache.TryGetValue(propertyName, out value) == false)
            {
                if (initialValue != null)
                    value = initialValue();
                else
                    value = default(R);

                var viewModelBase = value as ViewModelBase;
                if (viewModelBase != null)
                {
                    viewModelBase.RaiseOnPropertyChanged(() => viewModelBase.Errors, () => this.RaisePropertyChanged(() => this.Errors));
                }

                this._propertyCache[propertyName] = value;
                this.RaisePropertyChanged(propertyName);
            }

            return (R)value;
        }

        protected void SetValue<R>(Expression<Func<R>> property, R newValue, Func<R, string> getError = null)
        {
            this.VerifyAccess();

            var oldValue = GetValue(property);

            if ((object.Equals(oldValue, newValue) != true))
            {
                var propertyName = PropertyNameHelper.ExtractName(property);

                this._propertyCache[propertyName] = newValue;

                if (getError != null)
                {
                    var error = getError(newValue);

                    string oldError;
                    if (_errorCache.TryGetValue(propertyName, out oldError) == false)
                        oldError = string.Empty;

                    if (oldError != error)
                    {
                        _errorCache[propertyName] = error;
                        this.RaisePropertyChanged(() => this.Errors);
                    }
                }

                this.RaisePropertyChanged(propertyName);
            }
        }

        protected Command GetCommand(
            string commandText,
            Action execute,
            Func<bool> canExecute = null,
            params Expression<Func<object>>[] invalidateCanExecuteOnPropertyChange)
        {
            this.VerifyAccess();

            var commandId = execute.Method.Name;

            object command = null;
            if (_propertyCache.TryGetValue(commandId, out command))
            {
                return (Command)command;
            }
            else
            {
                var cmd = new BindableCommand(commandText, execute, canExecute);
                this._propertyCache[commandId] = cmd;

                if (invalidateCanExecuteOnPropertyChange != null && invalidateCanExecuteOnPropertyChange.Any())
                {
                    foreach (var entry in invalidateCanExecuteOnPropertyChange)
                    {
                        this.RaiseOnPropertyChanged(entry, () => cmd.InvalidateCanExecute());
                    }
                }
                else
                {
                    if (canExecute != null)
                        this.RaiseOnPropertyChanged(() => cmd.InvalidateCanExecute());
                }

                return cmd;
            }
        }

        protected void RaiseOnPropertyChanged(Action action)
        {
            this.VerifyAccess();

            this.PropertyChanged += (s, a) =>
            {
                action();
            };

            action();
        }

        protected void RaiseOnPropertyChanged<T>(Expression<Func<T>> propertyExpression, Action action)
        {
            this.VerifyAccess();

            var propertyName = PropertyNameHelper.ExtractName(propertyExpression);

            this.PropertyChanged += (s, a) =>
                {
                    if (a.PropertyName == propertyName)
                        action();
                };

            if (_propertyCache.ContainsKey(propertyName))
                action();
        }

        #endregion

        #region IDataErrorInfo

        public virtual IEnumerable<string> Errors
        {
            get
            {
                return _errorCache.Values.Where(err => string.IsNullOrEmpty(err) == false)
                    .Concat(_propertyCache.Values.OfType<ViewModelBase>().SelectMany(vm => vm.Errors));
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                return Errors.FirstOrDefault();
            }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;
                if (_errorCache.TryGetValue(propertyName, out error))
                    return error;
                else
                    return string.Empty;
            }
        }

        #endregion

        #region VerifyAccess
        private readonly System.Threading.Thread _ownerThread = System.Threading.Thread.CurrentThread;
        protected void VerifyAccess()
        {
            if (_ownerThread != System.Threading.Thread.CurrentThread)
                throw new InvalidOperationException();
        }
        #endregion
    }
}
