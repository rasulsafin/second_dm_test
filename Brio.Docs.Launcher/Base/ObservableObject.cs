using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Brio.Docs.Launcher.Base
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Установить значение хранящего поля свойства и вызвать событие <see cref="PropertyChanged"/> при его обновлении.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="backField">Поле, хранящее значение свойства.</param>
        /// <param name="value">Новое значение свойства.</param>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>true, если значение действительно было установлено.</returns>
        protected virtual bool SetProperty<T>(ref T backField, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backField, value))
                return false;
            backField = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
