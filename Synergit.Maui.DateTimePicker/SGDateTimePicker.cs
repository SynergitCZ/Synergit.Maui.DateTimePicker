using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Reflection;

namespace Synegit.Maui.DateTimePicker;

public class SGDateTimePicker : ContentView
{
    private readonly Entry dateTimePickerEntry;
    private readonly Image dateTimePickerIcon;
    private readonly Border dateTimePickerBorder;
    private bool isSetIconCalledForFirstTime = false;
    const double defaultHeightRequest = 40;
    private static Page Page => Application.Current?.MainPage ?? throw new ObjectDisposedException("Application.Current");
    
    public SGDateTimePicker()
    {
        base.Padding = 0;
        base.BackgroundColor = Colors.Transparent;
        base.HeightRequest = defaultHeightRequest;

        dateTimePickerEntry = new SGDateTimePickerEntry()
        {
            IsReadOnly = true,
            Margin = 0,
            BackgroundColor = Colors.Transparent,
            FontSize = this.FontSize,
            TextColor = this.TextColor,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = this.VerticalTextAlignment
        };

        dateTimePickerIcon = new Image
        {
            BackgroundColor = this.IconBackgroundColor,
            Aspect = Aspect.AspectFit,
            Margin = new Thickness(0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += (s, e) =>
        {
            OnDatePickerClicked(s, e);
        };
        dateTimePickerIcon.GestureRecognizers.Add(tapGestureRecognizer);

        var dateTimePickerGrid = new Microsoft.Maui.Controls.Grid
        {
            Margin = 0,
            Padding = 0,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions= LayoutOptions.Fill,
            VerticalOptions= LayoutOptions.Fill,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
            }
        };

        dateTimePickerGrid.SetColumn(dateTimePickerEntry, 0);
        dateTimePickerGrid.SetColumn(dateTimePickerIcon, 1);

        dateTimePickerGrid.Add(dateTimePickerEntry);
        dateTimePickerGrid.Add(dateTimePickerIcon);

        var dateTimePickerStackLayout = new StackLayout
        {
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        dateTimePickerStackLayout.Add(dateTimePickerGrid);

        dateTimePickerIcon.SetBinding(Image.WidthRequestProperty, new Binding("Height", source: dateTimePickerStackLayout));

        dateTimePickerIcon.SetBinding(Image.HeightRequestProperty, new Binding("Height", source: dateTimePickerStackLayout));

        dateTimePickerEntry.SetBinding(Entry.HeightRequestProperty, new Binding("Height", source: dateTimePickerStackLayout));

        dateTimePickerBorder = new Border
        {
            BackgroundColor = this.BackgroundColor,
            Stroke = this.BorderColor,
            StrokeThickness = this.BorderWidth,
            Content = dateTimePickerStackLayout,
            Margin = new Thickness(0),
            Padding = new Thickness(0)
        };

        var dateTimePickerWrapperStackLayout = new StackLayout
        {
            Margin = 0,
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        dateTimePickerWrapperStackLayout.Add(dateTimePickerBorder);

        dateTimePickerBorder.SetBinding(Border.HeightRequestProperty, new Binding("Height", source: dateTimePickerWrapperStackLayout));

        Content = dateTimePickerWrapperStackLayout;

        if (!isSetIconCalledForFirstTime)
            SetCalendarIcon();
    }

    public static async Task<object> OpenCalendarAsync(ISGDateTimePickerOptions options)
    {
        SGDateTimePickerPopup popupControl = new(options)
        {
            HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center,
            VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center
        };
        var popupResultTask = new PopupResultTask<PopupResult>();

        try
        {
            var result = await Page.ShowPopupAsync(popupControl);

            if (result is PopupResult popupResult)
            {
                popupResultTask.SetResult(popupResult);
            }
            else
            {               
                popupResultTask.SetResult(null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return await popupResultTask.Result;
    }

    #region Bindable properties

    public static readonly BindableProperty NullableDateTimeProperty = BindableProperty.Create(
        nameof(NullableDateTime),
        typeof(DateTime?),
        typeof(SGDateTimePicker),
        null,
        defaultBindingMode: BindingMode.TwoWay,
        null,
        (bindable, oldValue, newValue) =>
        {
            var self = (SGDateTimePicker)bindable;
            var oldNullableDateTime = (DateTime?)oldValue;
            var newNullableDateTime = (DateTime?)newValue;

            self.dateTimePickerEntry.Text = newNullableDateTime?.ToString(self.Format);

            //Date changed event
            bool isDateTimeChanged = false;
            if (self.Mode == PickerModes.Date && oldNullableDateTime?.Date != newNullableDateTime?.Date)
            {
                isDateTimeChanged = true;
            }
            else if (self.Mode == PickerModes.DateTime && (oldNullableDateTime?.Date != newNullableDateTime?.Date || oldNullableDateTime?.TimeOfDay != newNullableDateTime?.TimeOfDay))
            {
                isDateTimeChanged = true;
            }
            else if (self.Mode == PickerModes.Time && oldNullableDateTime?.TimeOfDay != newNullableDateTime?.TimeOfDay)
            {
                isDateTimeChanged = true;
            }

            if (isDateTimeChanged)
                self.SGDateTimeChanged?.Invoke(self, new DateTimeChangedEventArgs(oldNullableDateTime, newNullableDateTime));
        }
    );

    public DateTime? NullableDateTime
    {
        get { return (DateTime?)GetValue(NullableDateTimeProperty); }
        set
        {
            SetValue(NullableDateTimeProperty, value);
        }
    }

    public static readonly BindableProperty ToolButtonsStyleProperty = BindableProperty.Create(
        nameof(ToolButtonsStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        defaultValue: null, 
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
        }
    );

    public Style ToolButtonsStyle
    {
        get { return (Style)GetValue(ToolButtonsStyleProperty); }
        set { SetValue(ToolButtonsStyleProperty, value); }
    }

    public static readonly BindableProperty ModeProperty = BindableProperty.Create(
        nameof(Mode),
        typeof(PickerModes),
        typeof(SGDateTimePicker),
        PickerModes.Date,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (newValue is string strValue)
            {
                if (PickerModes.TryParse(strValue, out PickerModes pickerMode))
                {
                    newValue = pickerMode;
                }
            }
            ((SGDateTimePicker)bindable).SetCalendarIcon();
        }
    );

    public PickerModes Mode
    {
        get { return (PickerModes)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
        nameof(MinDate), 
        typeof(DateTime?), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            newValue = ParseDateTime(newValue);
        }
    );

    public DateTime? MinDate
    {
        get { return (DateTime?)GetValue(MinDateProperty); }
        set
        {
            SetValue(MinDateProperty, value);
        }
    }

    public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
        nameof(MaxDate), 
        typeof(DateTime?), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            newValue = ParseDateTime(newValue);
        }
    );

    public DateTime? MaxDate
    {
        get { return (DateTime?)GetValue(MaxDateProperty); }
        set
        {
            SetValue(MaxDateProperty, value);
        }
    }

    public static readonly BindableProperty BodyBackgroundColorProperty = BindableProperty.Create(
        nameof(BodyBackgroundColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        null,
        defaultBindingMode: BindingMode.OneWay
    );

    public Color BodyBackgroundColor
    {
        get { return (Color)GetValue(BodyBackgroundColorProperty); }
        set
        {
            SetValue(BodyBackgroundColorProperty, value);
        }
    }

    public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(
        nameof(HeaderBackgroundColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        null,
        defaultBindingMode: BindingMode.OneWay
    );

    public Color HeaderBackgroundColor
    {
        get { return (Color)GetValue(HeaderBackgroundColorProperty); }
        set
        {
            SetValue(HeaderBackgroundColorProperty, value);
        }
    }

    public static readonly BindableProperty HeaderForeColorProperty = BindableProperty.Create(
        nameof(HeaderForeColor), 
        typeof(Color), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public Color HeaderForeColor
    {
        get { return (Color)GetValue(HeaderForeColorProperty); }
        set
        {
            SetValue(HeaderForeColorProperty, value);
        }
    }

    public static readonly BindableProperty DayStyleProperty = BindableProperty.Create(
        nameof(DayStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public Style DayStyle
    {
        get { return (Style)GetValue(DayStyleProperty); }
        set
        {
            SetValue(DayStyleProperty, value);
        }
    }

    public static readonly BindableProperty DisabledDayStyleProperty = BindableProperty.Create(
        nameof(DisabledDayStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public Style DisabledDayStyle
    {
        get { return (Style)GetValue(DisabledDayStyleProperty); }
        set
        {
            SetValue(DisabledDayStyleProperty, value);
        }
    }

    public static readonly BindableProperty OtherMonthDayStyleProperty = BindableProperty.Create(
        nameof(OtherMonthDayStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public Style OtherMonthDayStyle
    {
        get { return (Style)GetValue(OtherMonthDayStyleProperty); }
        set
        {
            SetValue(OtherMonthDayStyleProperty, value);
        }
    }

    public static readonly BindableProperty SelectedDayStyleProperty = BindableProperty.Create(
        nameof(SelectedDayStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null
    );

    public Style SelectedDayStyle
    {
        get { return (Style)GetValue(SelectedDayStyleProperty); }
        set
        {
            SetValue(SelectedDayStyleProperty, value);
        }
    }

    public static readonly BindableProperty DayNamesStyleProperty = BindableProperty.Create(
        nameof(DayNamesStyle),
        typeof(Style),
        typeof(SGDateTimePicker),
        null,
        defaultBindingMode: BindingMode.OneWay
    );

    public Style DayNamesStyle
    {
        get { return (Style)GetValue(DayNamesStyleProperty); }
        set
        {
            SetValue(DayNamesStyleProperty, value);
        }
    }

    public static readonly BindableProperty WeekNumberStyleProperty = BindableProperty.Create(
        nameof(WeekNumberStyle), 
        typeof(Style), 
        typeof(SGDateTimePicker), 
        null, 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public Style WeekNumberStyle
    {
        get { return (Style)GetValue(WeekNumberStyleProperty); }
        set
        {
            SetValue(WeekNumberStyleProperty, value);
        }
    }

    public static readonly BindableProperty OkButtonTextProperty = BindableProperty.Create(
        nameof(OkButtonText),
        typeof(string),
        typeof(SGDateTimePicker),
        "OK",
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public string OkButtonText
    {
        get { return (string)GetValue(OkButtonTextProperty); }
        set
        {
            SetValue(OkButtonTextProperty, value);
        }
    }


    public static readonly BindableProperty ClearButtonTextProperty = BindableProperty.Create(
        nameof(ClearButtonText), 
        typeof(string), 
        typeof(SGDateTimePicker), 
        "Clear", 
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public string ClearButtonText
    {
        get { return (string)GetValue(ClearButtonTextProperty); }
        set
        {
            SetValue(ClearButtonTextProperty, value);
        }
    }

    public static readonly BindableProperty CancelButtonTextProperty = BindableProperty.Create(
        nameof(CancelButtonText),
        typeof(string),
        typeof(SGDateTimePicker),
        "Cancel",
        defaultBindingMode: BindingMode.OneWay, 
        null, 
        (b, o, n) => {}
    );

    public string CancelButtonText
    {
        get { return (string)GetValue(CancelButtonTextProperty); }
        set
        {
            SetValue(CancelButtonTextProperty, value);
        }
    }

    public static readonly BindableProperty ShowWeekNumbersProperty = BindableProperty.Create(
        nameof(ShowWeekNumbers),
        typeof(bool),
        typeof(SGDateTimePicker),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWay
    );

    public bool ShowWeekNumbers
    {
        get { return (bool)GetValue(ShowWeekNumbersProperty); }
        set { SetValue(ShowWeekNumbersProperty, value); }
    }

    public static readonly BindableProperty ShowOtherMonthDaysProperty = BindableProperty.Create(
        nameof(ShowOtherMonthDays),
        typeof(bool),
        typeof(SGDateTimePicker),
        defaultValue: true,
        defaultBindingMode: BindingMode.OneWay
    );

    public bool ShowOtherMonthDays
    {
        get { return (bool)GetValue(ShowOtherMonthDaysProperty); }
        set { SetValue(ShowOtherMonthDaysProperty, value); }
    }

    public static readonly BindableProperty ForeColorProperty = BindableProperty.Create(
        nameof(ForeColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay
    );

    public Color ForeColor
    {
        get { return (Color)GetValue(ForeColorProperty); }
        set { SetValue(ForeColorProperty, value); }
    }


    public static readonly BindableProperty ActivityIndicatorColorProperty = BindableProperty.Create(
        nameof(ActivityIndicatorColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay
    );

    public Color ActivityIndicatorColor
    {
        get { return (Color)GetValue(ActivityIndicatorColorProperty); }
        set { SetValue(ActivityIndicatorColorProperty, value); }
    }

    public static readonly BindableProperty ShowClearButtonProperty = BindableProperty.Create(
        nameof(ShowClearButton),
        typeof(bool),
        typeof(SGDateTimePicker),
        defaultValue: true,
        defaultBindingMode: BindingMode.OneWay
    );

    public bool ShowClearButton
    {
        get { return (bool)GetValue(ShowClearButtonProperty); }
        set { SetValue(ShowClearButtonProperty, value); }
    }

    public static readonly BindableProperty CloseOnOutsideClickProperty = BindableProperty.Create(
        nameof(CloseOnOutsideClick),
        typeof(bool),
        typeof(SGDateTimePicker),
        defaultValue: false,
        defaultBindingMode: BindingMode.OneWay
    );

    public bool CloseOnOutsideClick
    {
        get { return (bool)GetValue(CloseOnOutsideClickProperty); }
        set { SetValue(CloseOnOutsideClickProperty, value); }
    }


    public static readonly BindableProperty FormatProperty = BindableProperty.Create(
        nameof(Format), 
        typeof(string), 
        typeof(SGDateTimePicker), 
        null,
        defaultBindingMode: BindingMode.OneWay,
        null, 
        (b, o, n) =>
        {
            var self = (SGDateTimePicker)b;
            var oldValue = (string)o;
            var newValue = (string)n;
            if (oldValue != newValue)
            {
                self.dateTimePickerEntry.Text = self.NullableDateTime?.ToString(self.Format);
            }
        }
    );

    public string Format
    {
        get
        {
            var format = (string)GetValue(FormatProperty);
            if (string.IsNullOrEmpty(format))
            {
                if (Mode == PickerModes.Time)
                    format = "t";
                else if (Mode == PickerModes.DateTime)
                    format = "g";
                else
                    format = "d";
            }
            return format;
        }
        set
        {
            SetValue(FormatProperty, value);
        }
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(SGDateTimePicker),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((SGDateTimePicker)bindable).SetCalendarIcon();
        }
    );

    public ImageSource Icon
    {
        get { return (ImageSource)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public static readonly BindableProperty IconBackgroundColorProperty = BindableProperty.Create(
        nameof(IconBackgroundColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: Colors.Transparent,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (newValue != null)
                ((SGDateTimePicker)bindable).dateTimePickerIcon.BackgroundColor = (Color)newValue;
        }
    );

    public Color IconBackgroundColor
    {
        get { return (Color)GetValue(IconBackgroundColorProperty); }
        set
        {
            SetValue(IconBackgroundColorProperty, value);
        }
    }


    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(SGDateTimePicker),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((SGDateTimePicker)bindable).dateTimePickerEntry.Placeholder = (string)newValue ?? string.Empty;
        }
    );

    public string Placeholder
    {
        get { return (string)GetValue(PlaceholderProperty); }
        set { SetValue(PlaceholderProperty, value); }
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(SGDateTimePicker),
        defaultValue: 14d,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            double fontSize = 14d;
            if (newValue is double)
                fontSize = (double)newValue;
            else if (newValue is string)
                double.TryParse((string)newValue, out fontSize);

            ((SGDateTimePicker)bindable).dateTimePickerEntry.FontSize = fontSize;
        }
    );

    public double FontSize
    {
        get { return (double)GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: Colors.Black,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is Color textColor)
            {
                nullableDateTimePickerBindable.dateTimePickerEntry.TextColor = textColor;
            }
        }
    );

    public Color TextColor
    {
        get { return (Color)GetValue(TextColorProperty); }
        set { SetValue(TextColorProperty, value); }
    }

    public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: Colors.White,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is Color backgroundColor)
            {
                nullableDateTimePickerBindable.dateTimePickerBorder.BackgroundColor = backgroundColor;
            }
        }
    );

    public new Color BackgroundColor
    {
        get { return (Color)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(SGDateTimePicker),
        defaultValue: Colors.Transparent,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is Color borderColor)
            {
                nullableDateTimePickerBindable.dateTimePickerBorder.Stroke = borderColor;
            }
        }
    );

    public Color BorderColor
    {
        get { return (Color)GetValue(BorderColorProperty); }
        set { SetValue(BorderColorProperty, value); }
    }

    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
        nameof(BorderWidth),
        typeof(double),
        typeof(SGDateTimePicker),
        defaultValue: 0d,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (newValue is double)
            {
                ((SGDateTimePicker)bindable).dateTimePickerBorder.StrokeThickness = (double)newValue;
            }

        }
    );

    public double BorderWidth
    {
        get { return (double)GetValue(BorderWidthProperty); }
        set { SetValue(BorderWidthProperty, value); }
    }

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(Thickness),
        typeof(SGDateTimePicker),
        defaultValue: new Thickness(0),
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is Thickness cornerRadius)
            {
                nullableDateTimePickerBindable.dateTimePickerBorder.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new Microsoft.Maui.CornerRadius(cornerRadius.Left, cornerRadius.Top, cornerRadius.Right, cornerRadius.Bottom)
                };
            }
        }
    );

    public Thickness CornerRadius
    {
        get { return (Thickness)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public new static readonly BindableProperty PaddingProperty = BindableProperty.Create(
        nameof(Padding),
        typeof(Thickness),
        typeof(SGDateTimePicker),
        defaultValue: new Thickness(0),
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is Thickness padding)
            {
                nullableDateTimePickerBindable.dateTimePickerBorder.Padding = new Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }
    );
    
    public new Thickness Padding
    {
        get { return (Thickness)GetValue(PaddingProperty); }
        set { SetValue(PaddingProperty, value); }
    }


    public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(
        nameof(VerticalTextAlignment),
        typeof(TextAlignment),
        typeof(SGDateTimePicker),
        defaultValue: TextAlignment.Center,
        defaultBindingMode: BindingMode.OneWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SGDateTimePicker nullableDateTimePickerBindable && newValue is TextAlignment verticalTextAlignment)
            {
                nullableDateTimePickerBindable.dateTimePickerEntry.VerticalTextAlignment = verticalTextAlignment;
            }
        }
    );

    public TextAlignment VerticalTextAlignment
    {
        get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
        set { SetValue(VerticalTextAlignmentProperty, value); }
    }
    
    #endregion // Bindable properties

    private async void OnDatePickerClicked(object sender, EventArgs e)
    {
        if (!base.IsEnabled)
            return;

        await OpenCalendarPopupAsync();
    }

    bool isPopupOpen = false;

    private async Task OpenCalendarPopupAsync()
    {
        if (isPopupOpen)
            return;

        isPopupOpen = true;

        try
        {
            var options = new SGDateTimePickerOptions
            {
                NullableDateTime = this.NullableDateTime,
                Mode = this.Mode,
                MinDate = this.MinDate,
                MaxDate = this.MaxDate,
                OkButtonText = this.OkButtonText,
                CancelButtonText = this.CancelButtonText,
                ClearButtonText = this.ClearButtonText,
                ForeColor = this.ForeColor,
                BodyBackgroundColor = this.BodyBackgroundColor,
                HeaderForeColor = this.HeaderForeColor,
                HeaderBackgroundColor = this.HeaderBackgroundColor,
                ToolButtonsStyle = this.ToolButtonsStyle,
                DayStyle = this.DayStyle,
                DisabledDayStyle = this.DisabledDayStyle,
                OtherMonthDayStyle = this.OtherMonthDayStyle,
                SelectedDayStyle = this.SelectedDayStyle,
                DayNamesStyle = this.DayNamesStyle,
                ShowWeekNumbers = this.ShowWeekNumbers,
                WeekNumberStyle = this.WeekNumberStyle,
                ShowOtherMonthDays = this.ShowOtherMonthDays,
                ActivityIndicatorColor = this.ActivityIndicatorColor,
                ShowClearButton = this.ShowClearButton,
                CloseOnOutsideClick = this.CloseOnOutsideClick
            };

            var result = await OpenCalendarAsync(options);
            if (result is PopupResult popupResult && popupResult.ButtonResult != PopupButtons.Cancel)
            {
                NullableDateTime = popupResult.NullableDateTime;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            isPopupOpen = false;
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(base.IsEnabled):
                dateTimePickerBorder.IsEnabled = base.IsEnabled;
                dateTimePickerIcon.IsEnabled = base.IsEnabled;
                break;
        }
    }


    string imgName;
    ImageSource imgSource;
    
    private void SetCalendarIcon()
    {
        isSetIconCalledForFirstTime = true;

        if (Icon != null)
        {
            if (!object.Equals(imgSource, Icon))
            {
                imgSource = Icon;
                dateTimePickerIcon.Source = Icon;
            }
        }
        else
        {
            string imageName = "date.png";

            if (Mode == PickerModes.DateTime)
                imageName = "date_time.png";
            else if (Mode == PickerModes.Time)
                imageName = "time.png";

            if (imgName != imageName)
            {
                imgName = imageName;

                dateTimePickerIcon.Source = ImageSource.FromResource($"Synergit.Maui.DateTimePicker.Images.{imageName}", typeof(SGDateTimePicker).GetTypeInfo().Assembly);
            }
        }
    }

    private static DateTime? ParseDateTime(object objectValue)
    {
        DateTime? dateValue = null;
        if (objectValue is DateTime)
        {
            dateValue = (DateTime?)objectValue;
        }
        else if (objectValue is string strValue)
        {
            if (DateTime.TryParse(strValue, out DateTime outputDate))
            {
                dateValue = outputDate;
            }
        }
        return dateValue;
    }

    public event EventHandler<DateTimeChangedEventArgs> SGDateTimeChanged;
}
