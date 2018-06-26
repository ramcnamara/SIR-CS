using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace InverseBinding
{
   public class InvertedBinding : INotifyPropertyChanged, IDisposable
   {
      #region Private Fields
      private readonly object _dataSource;
      private readonly EventInfo _changedEvent;
      private readonly MethodInfo _getAccessor;
      private readonly MethodInfo _setAccessor;
      #endregion

      #region Constructors
      protected InvertedBinding(object dataSource, string dataSourceBoundPropertyName)
      {
         if ((dataSource == null) || (dataSourceBoundPropertyName == null))
         {
            throw new ArgumentNullException();
         }

         _dataSource = dataSource;

         EventInfo propertyChangedEvent = 
            _dataSource.GetType().GetEvent(string.Format("{0}Changed", dataSourceBoundPropertyName));
         if ((propertyChangedEvent != null) && (typeof(EventHandler).IsAssignableFrom(propertyChangedEvent.EventHandlerType)))
         {
            _changedEvent = propertyChangedEvent;
            _changedEvent.AddEventHandler(_dataSource, new EventHandler(OnDataSourcePropertyChanged));
         }

         PropertyInfo dataSourceBoundProperty = 
            _dataSource.GetType().GetProperty(dataSourceBoundPropertyName);
         if (dataSourceBoundProperty == null)
         {
            throw new MissingMemberException(string.Format(
                                                "Could not find property '{0}.{1}'",
                                                _dataSource.GetType().FullName, dataSourceBoundPropertyName));
         }

         _getAccessor = dataSourceBoundProperty.GetGetMethod();
         if (_getAccessor == null)
         {
            throw new MissingMethodException(string.Format(
                                                "No get accessor for '{0}'", dataSourceBoundProperty.Name));
         }
         if (!typeof(bool).IsAssignableFrom(_getAccessor.ReturnType))
         {
            throw new ArgumentException(
               string.Format(
                  "Class only works on boolean properties, '{0}' is not of type bool", dataSourceBoundProperty.Name));
         }

         _setAccessor = dataSourceBoundProperty.GetSetMethod();
      }
      #endregion

      /// <summary>
      ///  Create an "inverted binding".
      /// </summary>
      /// <param name="dataSource">Data source to bind to.</param>
      /// <param name="propertyName">Property name to inversely bind to.</param>
      /// <returns>Binding object.  Can be added to 
      ///  <see cref="Control.DataBindings"/>.</returns>
      /// <remarks>NOTE: if binding to a readonly property or if there is not a suitable
      ///  <c><paramref name="propertyName"/>Changed</c> event, binding may not
      ///  update as expected.</remarks>
      /// <exception cref="ArgumentNullException">Thrown if a required parameter is 
      ///  <c>null</c>.</exception>
      /// <exception cref="MissingMemberException">Thrown if <paramref name="propertyName"/>
      ///  refers to a property that cannot be found.</exception>
      /// <exception cref="MissingMethodException">Thrown if attempting to use a 
      ///  set-only property.</exception>
      /// <exception cref="ArgumentException">Thrown if attempting to use on non-<c>bool</c>
      ///  property.</exception>
      public static Binding Create(object dataSource, string propertyName)
      {
         return new Binding(propertyName, new InvertedBinding(dataSource, propertyName), "InvertedProperty");
      }

      public bool InvertedProperty
      {
         get
         {
            return !GetDataBoundValue();
         }
         set
         {
            if (_setAccessor == null)
            {
               // nothing to do since no one will get notified.
               return;
            }

            bool curVal = InvertedProperty;
            
            // a little bit of trickery here, we only want to change the value if IS the same
            // rather than the conventional if it's different
            if (curVal == value)
            {
               _setAccessor.Invoke(_dataSource, new object[] { !value });
               if (PropertyChanged != null)
               {
                  PropertyChanged(this, new PropertyChangedEventArgs("InvertedProperty"));
               }
            }
         }
      }

      #region INotifyPropertyChanged Members
      public event PropertyChangedEventHandler PropertyChanged;
      #endregion

      #region IDisposable Members
      public void Dispose()
      {
         if (_changedEvent != null)
         {
            _changedEvent.RemoveEventHandler(_dataSource, new EventHandler(OnDataSourcePropertyChanged));
         }
      }
      #endregion

      private void OnDataSourcePropertyChanged(object sender, EventArgs e)
      {
         // refresh our property (which may trigger our PropertyChanged event)
         InvertedProperty = !GetDataBoundValue();
      }

      private bool GetDataBoundValue()
      {
         return (bool) _getAccessor.Invoke(_dataSource, null);
      }
   }
}