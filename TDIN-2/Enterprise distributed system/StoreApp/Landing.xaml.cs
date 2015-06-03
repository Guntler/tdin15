﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for Landing.xaml
    /// </summary>
    public partial class Landing : Window
    {

        public Landing()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Purchase dialog = new Purchase()
            {
                Title = "Purchase",
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //if stock<=0...
            Shippings dialog = new Shippings()
            {
                Title = "Notifications",
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
            }

        }
    }
}
