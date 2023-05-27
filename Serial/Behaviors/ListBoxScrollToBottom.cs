using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows.Controls;

namespace Serial.Behaviors
{
    public class ListBoxScrollToBottom : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            //ICollectionView类型才有CollectionChanged事件
            ((ICollectionView)AssociatedObject.Items).CollectionChanged += CollectionChangedHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            ((ICollectionView)AssociatedObject.Items).CollectionChanged -= CollectionChangedHandler;
        }

        private void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject.HasItems)
            {
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
            }
        }
    }
}