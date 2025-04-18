using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace SimpleFileManager.WPFApp.Behavior;

public class ListViewSortBehavior : Behavior<ListView>
{
    private ListSortDirection _sortDirection;
    private string _sortPropertyName = "";

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(HeaderClicked));
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(HeaderClicked));
    }

    private void HeaderClicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is GridViewColumnHeader header && header.Column != null )
        {
            string? propertyName = header.Column.GetValue(SortPropertyNameProperty) as string;
            if (propertyName is null || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            if (_sortPropertyName == propertyName)
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            else
            {
                _sortPropertyName = propertyName;
                _sortDirection = ListSortDirection.Ascending;
            }

            Sort(propertyName, _sortDirection);
        }
    }

    private void Sort(string propertyName, ListSortDirection direction)
    {
        var files = AssociatedObject.ItemsSource;
        var collectionView = CollectionViewSource.GetDefaultView(files);


        collectionView.SortDescriptions.Clear();
        collectionView.SortDescriptions.Add(new SortDescription(propertyName, direction));
    }

    public static readonly DependencyProperty SortPropertyNameProperty =
        DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(ListViewSortBehavior));

    public static string GetSortPropertyName(DependencyObject obj)
    {
        return (string)obj.GetValue(SortPropertyNameProperty);
    }

    public static void SetSortPropertyName(DependencyObject obj, string value)
    {
        obj.SetValue(SortPropertyNameProperty, value);
    }
}