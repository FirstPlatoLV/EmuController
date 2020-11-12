using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EmuController.DevManager.Behaviors
{
    public class MaxCharactersBehavior : Behavior<UIElement>
    {
        public int MaxCharacters { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
        }

        private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ((System.Windows.Controls.TextBox)e.OriginalSource).Text.Length >= MaxCharacters;
        }
    }
}
