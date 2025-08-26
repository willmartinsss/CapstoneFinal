using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SpaceInvaders.Models;
using System;

namespace SpaceInvaders.Views
{
    public class GameStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is GameStateType currentState && parameter is string targetState)
            {
                return Enum.TryParse<GameStateType>(targetState, out var target) && currentState == target 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
