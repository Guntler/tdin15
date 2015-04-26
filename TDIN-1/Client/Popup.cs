using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Popup : Form
    {
        string text;
        public bool accept {get; private set;}
        public Popup(string text)
        {
            this.text = text;
            InitializeComponent();
            TextLbl.Text = text;
            this.accept = true;
        }

        private void AcceptBtn_Click(object sender, EventArgs e)
        {
            accept = true;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            accept = false;
            this.Close();
        }
    }
}
