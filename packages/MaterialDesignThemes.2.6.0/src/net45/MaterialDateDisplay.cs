﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Calendar = System.Windows.Controls.Calendar;

namespace MaterialDesignThemes.Wpf
{
    public class MaterialDateDisplay : Control
    {
        static MaterialDateDisplay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialDateDisplay), new FrameworkPropertyMetadata(typeof(MaterialDateDisplay)));
        }

        public MaterialDateDisplay()
        {
            SetCurrentValue(DisplayDateProperty, DateTime.Today);
        }

        public static readonly DependencyProperty DisplayDateProperty = DependencyProperty.Register(
            nameof(DisplayDate), typeof(DateTime), typeof(MaterialDateDisplay), new PropertyMetadata(default(DateTime), DisplayDatePropertyChangedCallback));

        private static void DisplayDatePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((MaterialDateDisplay)dependencyObject).UpdateComponents();
        }

        public DateTime DisplayDate
        {
            get { return (DateTime)GetValue(DisplayDateProperty); }
            set { SetValue(DisplayDateProperty, value); }
        }

        private static readonly DependencyPropertyKey ComponentOneContentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ComponentOneContent), typeof(string), typeof(MaterialDateDisplay),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ComponentOneContentProperty =
            ComponentOneContentPropertyKey.DependencyProperty;

        public string ComponentOneContent
        {
            get { return (string)GetValue(ComponentOneContentProperty); }
            private set { SetValue(ComponentOneContentPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ComponentTwoContentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ComponentTwoContent), typeof(string), typeof(MaterialDateDisplay),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ComponentTwoContentProperty =
            ComponentTwoContentPropertyKey.DependencyProperty;

        public string ComponentTwoContent
        {
            get { return (string)GetValue(ComponentTwoContentProperty); }
            private set { SetValue(ComponentTwoContentPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ComponentThreeContentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ComponentThreeContent), typeof(string), typeof(MaterialDateDisplay),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ComponentThreeContentProperty =
            ComponentThreeContentPropertyKey.DependencyProperty;

        public string ComponentThreeContent
        {
            get { return (string)GetValue(ComponentThreeContentProperty); }
            private set { SetValue(ComponentThreeContentPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsDayInFirstComponentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsDayInFirstComponent), typeof(bool), typeof(MaterialDateDisplay),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsDayInFirstComponentProperty =
            IsDayInFirstComponentPropertyKey.DependencyProperty;

        public bool IsDayInFirstComponent
        {
            get { return (bool)GetValue(IsDayInFirstComponentProperty); }
            private set { SetValue(IsDayInFirstComponentPropertyKey, value); }
        }

        private void UpdateComponents()
        {
            var culture = Language.GetSpecificCulture();
            var dateTimeFormatInfo = culture.GetDateFormat();
            var minDateTime = dateTimeFormatInfo.Calendar.MinSupportedDateTime;
            var maxDateTime = dateTimeFormatInfo.Calendar.MaxSupportedDateTime;

            if (DisplayDate < minDateTime)
            {
                SetDisplayDateOfCalendar(minDateTime);

                // return to avoid second formatting of the same value
                return;
            }

            if (DisplayDate > maxDateTime)
            {
                SetDisplayDateOfCalendar(maxDateTime);

                // return to avoid second formatting of the same value
                return;
            }

            ComponentOneContent = DisplayDate.ToString(dateTimeFormatInfo.MonthDayPattern.Replace("MMMM", "MMM"), culture).ToTitleCase(culture); //Day Month following culture order. We don't want the month to take too much space
            ComponentTwoContent = DisplayDate.ToString("ddd,", culture).ToTitleCase(culture);   // Day of week first
            ComponentThreeContent = DisplayDate.ToString("yyyy", culture).ToTitleCase(culture); // Year always top
        }

        private void SetDisplayDateOfCalendar(DateTime displayDate)
        {
            Calendar calendarControl = this.GetVisualAncestry().OfType<Calendar>().FirstOrDefault();

            if (calendarControl != null)
            {
                calendarControl.DisplayDate = displayDate;
            }
        }
    }
}
