﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using MongoDB.Driver;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GUI : Window
    {
        public GUI()
        {
            InitializeComponent();
            this.ShowLogin();
        }

        public void ShowLogin()
        {
            this.ContentHolder.Content = new LoginView(this);
        }

        public void ShowMain()
        {
            this.ContentHolder.Content = new MainView(this);
        }
    }
}