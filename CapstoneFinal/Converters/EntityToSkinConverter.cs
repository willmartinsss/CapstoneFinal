using Microsoft.UI.Xaml.Data;
using SpaceInvaders.Models;
using System;

namespace SpaceInvaders.Views
{
    public class EntityToSkinConverter : IValueConverter
    {
        // A anotação 'object?' informa que o retorno pode ser nulo.
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is GameEntity entity)
            {
                return entity.GetSkin();
            }
            return null;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
