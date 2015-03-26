﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Collections;

namespace Client
{
    public partial class Client : Form
    {
        IAPI api;
        readonly AlterEventRepeater _evRepeater;
        delegate ListViewItem LVAddDelegate(ListViewItem lvItem);

        delegate void ChCommDelegate(DOrder order);

        public Client()
        {
            RemotingConfiguration.Configure("Client.exe.config", false);
            InitializeComponent();
            api = (IAPI)RemoteNew.New(typeof(IAPI));
            ExchangePanel.Parent = this;

            _evRepeater = new AlterEventRepeater();
            _evRepeater.AlterEvent += new AlterDelegate(OperationHandler);
            api.AlterEvent += new AlterDelegate(_evRepeater.Repeater);
        }

        public void OperationHandler(Operation op, DOrder order)
        {
            LVAddDelegate lvAdd;
            ChCommDelegate chComm;

            switch (op)
            {
                case Operation.New:
                    lvAdd = new LVAddDelegate(listView.Items.Add);
                    ListViewItem lvItem = new ListViewItem(new string[] { order.Type.ToString(), order.Amount.ToString(), (order.Value*order.Amount).ToString(), "not yet implemented"});
                    Invoke(lvAdd, new object[] { lvItem });
                    break;
                case Operation.Change:
                    chComm = new ChCommDelegate(ChangeOrder);
                    Invoke(chComm, new object[] { order });
                    break;
            }
        }

        private void ChangeOrder(DOrder order)
        {
            var i = api.ActiveOrders.FindIndex(o => order.Id == o.Id);
            if(i>-1) api.ActiveOrders[i] = order;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                User aux = new User(textBox1.Text, textBox2.Text, textBox3.Text);
                int result = api.RegisterUser(ref aux);
                MessageBox.Show("Hello World! Here is the output of the register action: " + result);
                api.GetUserByName("NONEEE");
                api.GetUserByName(textBox2.Text);


                //api.removeUserByName(textBox2.Text);

                api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);

                DOrder tempOrder = new DOrder(aux, 5, 5.0, OrderType.Buy);
                api.RegisterOrder(ref tempOrder);
                api.DeleteOrder(tempOrder);
                api.RegisterOrder(ref tempOrder);


                DTransaction tempTrans = new DTransaction(aux, 5.0, tempOrder);
                api.RegisterTransaction(tempTrans);
                api.DeleteTransaction(tempTrans);
                api.RegisterTransaction(tempTrans);

                api.GetOrder(tempOrder.Id);
                api.GetTransaction(tempTrans.Order.Id);

                //Só se vai buscar as orders no login bem sucedido
                //orders = api.ActiveOrders;
                //MessageBox.Show("Number of orders:"+orders.Count);
            }
            catch (Exception error)
            {
                MessageBox.Show("Something went wrong:\n" + error.Message);
            }
            finally {
                //clear text
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //login user
            if (api.ValidateUser(textBox4.Text, textBox5.Text))
            {
                MessageBox.Show("User login valid!", "Form1");
                List<DOrder> _orders = api.ActiveOrders;
                foreach (DOrder order in _orders)
                {
                    ListViewItem lvItem = new ListViewItem(new string[] { order.Type.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), "not yet implemented" });
                    listView.Items.Add(lvItem);
                }
                showExchangePanel();
            }
            else
            {
                MessageBox.Show("User login invalid!", "Form1");
            }
        }

        private void showExchangePanel()
        {
            loginPanel.Visible = false;
            registerPanel.Visible = false;
            ExchangePanel.Visible = true;
            //IAPI.getActiveTransactions();
        }

        private void showInitialPanel()
        {
            loginPanel.Visible = true;
            registerPanel.Visible = true;
            ExchangePanel.Visible = false;
        }
    }
}

/* Mechanism for instanciating a remote object through its interface, using the config file */

class RemoteNew
{
    private static Hashtable types = null;

    private static void InitTypeTable()
    {
        types = new Hashtable();
        foreach (WellKnownClientTypeEntry entry in RemotingConfiguration.GetRegisteredWellKnownClientTypes())
            types.Add(entry.ObjectType, entry);
    }

    public static object New(Type type)
    {
        if (types == null)
            InitTypeTable();
        WellKnownClientTypeEntry entry = (WellKnownClientTypeEntry)types[type];
        if (entry == null)
            throw new RemotingException("Type not found!");
        return RemotingServices.Connect(type, entry.ObjectUrl);
    }
}