using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MVVM
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            var action = this.PropertyChanged;
            if (action != null)
                action(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = PropertyNameHelper.ExtractName(propertyExpression);
            this.RaisePropertyChanged(propertyName);
        }

        protected void SetValueAndRaiseNotify<T, R>(ref T target, T value, Expression<Func<R>> raisePropertyChange)
        {
            if ((object.Equals(target, value) != true))
            {
                target = value;
                this.RaisePropertyChanged(raisePropertyChange);
            }
        }

        #endregion

    }
}
