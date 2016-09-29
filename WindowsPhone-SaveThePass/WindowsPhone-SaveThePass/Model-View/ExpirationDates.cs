using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using WindowsPhone_SaveThePass.Model;

namespace WindowsPhone_SaveThePass.Model_View
{
    /// <summary>
    /// Notes: A different number will be used for Never propably some really big number.
    /// </summary>
    static class ExpirationDates
    {
        public static Dictionary<string, int> ExpDates { get { return expDates; } }

        static Dictionary<string, int> expDates = new Dictionary<string, int>()
        {
            {"Never", 0 },
            {"1 Month", 1 },
            {"2 Month", 2 },
            {"3 Month", 3 },
            {"6 Months", 6 },
        };

        /// <summary>
        /// Used to initialize a comboBox (I wanted it to be more abstract. Make it so!)
        /// </summary>
        /// <param name="cB"></param>
        public static void AddToComboBox(ComboBox cB)
        {
            cB.ItemsSource = expDates.Keys;
            cB.SelectedIndex = 0;
        }

        public static DateTime CalculateExpirationDate(string selectedMonth, DateTime savedDate)
        {
            DateTime expirationDate = DateTime.MinValue;
            int monthValue = expDates[selectedMonth];

            if (monthValue == expDates["Never"])
                expirationDate = DateTime.MinValue;
            else
                expirationDate = savedDate.AddMonths(monthValue);

            return expirationDate;
        }

        public static int DaysUntilExpiration(DateTime dateExpiring)
        {
            int daysToExpire = (int)(dateExpiring - DateTime.Today).TotalDays;

            return daysToExpire;
        }

        public static int DaysUntilExpiration(DateTime dateExpiring, out bool doesExpire)
        {
            int daysToExpire = 0;
            doesExpire = true;

            if (dateExpiring == DateTime.MinValue)
                doesExpire = false;
            else
                daysToExpire = (int)(dateExpiring - DateTime.Today).TotalDays;

            return daysToExpire;
        }
    }
}
