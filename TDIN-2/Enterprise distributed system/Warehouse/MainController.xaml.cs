﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainController.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private GUI _parent;
        public static ObservableCollection<Message> MessageList = new ObservableCollection<Message>();

        public MainView(GUI parent)
        {
            this._parent = parent;
            InitializeComponent();
        }
    }
}
