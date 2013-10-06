using System.Windows;
using System.Windows.Controls;

namespace Cloudsdale.Controls {
    public class AlternatableContent : ContentPresenter {
        public static DependencyProperty UseAlternateProperty = DependencyProperty.Register
            ("UseAlternante", typeof(bool), typeof(AlternatableContent), new PropertyMetadata(PropertyChanged));
        public bool UseAlternate {
            get { return (bool)GetValue(UseAlternateProperty); }
            set { SetValue(UseAlternateProperty, value); }
        }
        public static DependencyProperty PrimaryTemplateProperty = DependencyProperty.Register
            ("PrimaryTemplate", typeof(DataTemplate), typeof(AlternatableContent), new PropertyMetadata(PropertyChanged));
        public DataTemplate PrimaryTemplate {
            get { return (DataTemplate)GetValue(PrimaryTemplateProperty); }
            set { SetValue(PrimaryTemplateProperty, value); }
        }
        public static DependencyProperty SecondaryTemplateProperty = DependencyProperty.Register
            ("SecondaryTemplate", typeof(DataTemplate), typeof(AlternatableContent), new PropertyMetadata(PropertyChanged));
        public DataTemplate SecondaryTemplate {
            get { return (DataTemplate)GetValue(SecondaryTemplateProperty); }
            set { SetValue(SecondaryTemplateProperty, value); }
        }

        private static void PropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var template = (AlternatableContent)dependencyObject;

            template.ContentTemplate = template.UseAlternate ? template.SecondaryTemplate : template.PrimaryTemplate;
        }
    }
}
