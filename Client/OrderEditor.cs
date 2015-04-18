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
    public partial class OrderEditor : Form
    {
        public int id { get; private set; }
        public string type { get; private set; }
        public double value { get; private set; }
        public bool updated { get; private set; }
        public OrderEditor()
        {
            InitializeComponent();
        }

        public OrderEditor(int id, string tipo, double valor)
        {
            InitializeComponent();
            this.id = id;
            this.type = tipo;
            this.value = valor;
            this.updated = false;
            idLbl.Text = this.id.ToString();
            Order_Type.Text = this.type;
            textBox1.Text = this.value.ToString();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            double aux;
            if (!Double.TryParse(this.textBox1.Text, out aux))
            {
                this.textBox1.Text = "";
                MessageBox.Show("Order value parse error", "OrderEditor");
            }
            this.value = aux;
            this.updated = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.updated = false;
            this.Close();
        }
    }
}
