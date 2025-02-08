﻿using System.Windows;
using System.Windows.Controls;

namespace MsSql.ClassGenerator.Ui.ViewModel.Controls;
/// <summary>
/// Interaction logic for HeadlineControl.xaml
/// </summary>
public partial class HeadlineControl : UserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="HeadlineControl"/>.
    /// </summary>
    public HeadlineControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The dependency property of <see cref="HeadlineText"/>.
    /// </summary>
    public static readonly DependencyProperty HeadlineTextProperty = DependencyProperty.Register(
        nameof(HeadlineText), typeof(string), typeof(HeadlineControl), new PropertyMetadata("HEADLINE"));

    /// <summary>
    /// Gets or sets the headline text.
    /// </summary>
    public string HeadlineText
    {
        get => (string)GetValue(HeadlineTextProperty);
        set => SetValue(HeadlineTextProperty, value);
    }

    /// <summary>
    /// The dependency property of <see cref="IconKind"/>.
    /// </summary>
    public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(
        nameof(IconKind), typeof(string), typeof(HeadlineControl), new PropertyMetadata("AngleDoubleRight"));

    /// <summary>
    /// Gets or sets the icon kind.
    /// </summary>
    public string IconKind
    {
        get => (string)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
}
