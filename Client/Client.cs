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
using System.Timers;
using System.Threading;

namespace Client
{
    public partial class Client : Form
    {
        IAPI api;
        User userSession;
        readonly AlterEventRepeater _evRepeater;
        delegate ListViewItem LvAddDelegate(ListViewItem lvItem);
        delegate void LVRemoveDelegate(ListViewItem lvItem);
        delegate void LVaddDelegate(DOrder order);
        delegate void LVupdateUIDelegate(DOrder order);
        delegate ListViewItem[] LVFindDelegate(string key, bool searchAllSubItems);
        delegate void ChCommDelegate(DOrder order);
        delegate void LvRemDelegate(DOrder order);
        AlterDelegate DelRepeater;

        public Client()
        {
            RemotingConfiguration.Configure("Client.exe.config", false);
            InitializeComponent();
            api = (IAPI)RemoteNew.New(typeof(IAPI));
            ExchangePanel.Parent = this;

            _evRepeater = new AlterEventRepeater();
            _evRepeater.alterEvent += new AlterDelegate(OperationHandler);
            DelRepeater = new AlterDelegate(_evRepeater.Repeater);
            api.alterEvent += DelRepeater;
        }

        public void OperationHandler(Operation op, DOrder order)
        {
            var delgUI = new LVupdateUIDelegate(updateUI);
            Invoke(delgUI, new object[] { order });

            if (userSession == null || !order.Source.Nickname.Equals(userSession.Nickname))
                return;

            switch (op)
            {
                case Operation.New:
                    var lvAdd = new LVaddDelegate(addHandler);
                    Invoke(lvAdd, new object[] { order });
                    break;
                case Operation.Change: //mudança no amount de uma ordem
                    var chChomm = new ChCommDelegate(updateOrder);
                    Invoke(chChomm, new object[] { order });
                    break;
                case Operation.Remove:
                    var lvRem = new LvRemDelegate(RemoveHandler);
                    Invoke(lvRem, new object[] { order });
                    break;
                case Operation.Notify:
                    //Notify User that they have sold X diginotes
                    chChomm = new ChCommDelegate(notifyUI);
                    Invoke(chChomm, new object[] { order });
                    break;
                case Operation.ChangeAll: //mudança do exchange value de todas as ordens
                    chChomm = new ChCommDelegate(ChangeHandler);
                    Invoke(chChomm, new object[] { order });
                    break;
            }
        }

        private void addHandler(DOrder order)
        {
            var lvAdd = new LvAddDelegate(itemListView.Items.Add);
            if(order.Type==OrderType.Sell)
                diginotesLbl.Text = (api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count).ToString();
            ListViewItem lvItem = new ListViewItem(new string[] { order.Id.ToString(), order.Type.ToString(), order.Value.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), order.Status.ToString(), order.Date.ToString() });
            Invoke(lvAdd, new object[] { lvItem });
        }

        private void notifyUI(DOrder order)
        {
            diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
            UpdateExchangePanel(api.ActiveOrders);
            MessageBox.Show("NotifyUI: found match->" + api.ActiveOrders.Exists(o => o.Id == order.Id));
            if(api.ActiveOrders.Exists(o => o.Id == order.Id)){
                DOrder aux = api.ActiveOrders.Find(o => o.Id == order.Id);
                MessageBox.Show("notifyUI\nOrder.Amount: " + order.Amount + "\naux.Amount: " + aux.Amount);
                MessageBox.Show("Order of id:" + order.Id + " has been updated.\n" + (order.Type == OrderType.Buy ? "Bought " : "Sold ") + (order.Amount - aux.Amount) + " diginotes\n");
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            api.ChangeAllUserOrders(this.userSession, api.ExchangeValue);
            foreach (ListViewItem item in itemListView.Items) //sets all order's exchange value to new value
            {
                item.SubItems[2].Text = api.ExchangeValue.ToString();
            }
        }

        private void updateUI(DOrder order)
        {
            Num_Buy_Order_System.Text = api.ActiveOrders.FindAll(o => o.Type == OrderType.Buy).Count.ToString();
            Num_Sell_Order_System.Text = api.ActiveOrders.FindAll(o => o.Type == OrderType.Sell).Count.ToString();
            ExchangeValueLbl.Text = String.Format("{0:0.00}", api.ExchangeValue);
            diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
        }

        private void updateOrder(DOrder order)
        {
            foreach (ListViewItem item in itemListView.Items) //remove orders with exchange values != from the new one
            {
                if (item.SubItems[0].Text.Equals(order.Id.ToString()))
                {
                    item.SubItems[3].Text = order.Amount.ToString();
                    item.SubItems[4].Text = (order.Value * order.Amount).ToString();
                }
            }
        }


        private void ChangeHandler(DOrder order)
        {
            string newValue = String.Format("{0:0.00}", api.ExchangeValue);
            if (!newValue.Equals(ExchangeValueLbl.Text)) //mudança de exchangeValue
            {
                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 1000;
                aTimer.Enabled = true;

                DialogResult dialogResult = MessageBox.Show("Would you accept the new exchange value:" + newValue + "?\n", "New exchange value!", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    aTimer.Stop();
                    api.ChangeAllUserOrders(this.userSession, api.ExchangeValue);
                    int amount;
                    foreach (ListViewItem item in itemListView.Items) //sets all order's exchange value to new value
                    {
                        amount = Int32.Parse(item.SubItems[3].ToString());
                        item.SubItems[2].Text = newValue.ToString();
                        item.SubItems[4].Text = (api.ExchangeValue * amount).ToString();
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    aTimer.Stop();
                    api.DeleteAllUserOrders(this.userSession);
                    foreach (ListViewItem item in itemListView.Items) //remove orders with exchange values != from the new one
                    {
                        if (!item.SubItems[2].ToString().Equals(newValue.ToString()))
                        {
                            item.Remove();
                        }
                    }
                }
            }
        }

        private void RemoveHandler(DOrder order)
        {
            //MessageBox.Show("RemoveHandler: "+order.ToString());
            notifyUI(order);
            itemListView.Clear();
            //MessageBox.Show("Active orders:" + api.ActiveOrders.Count);
            List<DOrder> aux = api.ActiveOrders.FindAll(o => o.Source.Nickname.Equals(userSession.Nickname));
            foreach (DOrder o in aux)
            {
                var lvAdd = new LvAddDelegate(itemListView.Items.Add);
                ListViewItem lvItem = new ListViewItem(new string[] { o.Id.ToString(), o.Type.ToString(), o.Value.ToString(), o.Amount.ToString(), (o.Value * o.Amount).ToString(), o.Status.ToString(), o.Date.ToString() });
                Invoke(lvAdd, new object[] { lvItem });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                User aux = new User(textBox1.Text, textBox2.Text, textBox3.Text);
                int result = api.RegisterUser(ref aux);
                if (result == 1)
                {
                    MessageBox.Show("Nickname has been taken, please choose another.", "Diginote Exchange System");
                }
                else
                {
                    MessageBox.Show("Registration has been a success!", "Diginote Exchange System");
                }
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            catch (Exception error)
            {
                MessageBox.Show("Something went wrong:\n" + error.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((userSession = api.ValidateUser(textBox4.Text, textBox5.Text)) != null)
            {
                UserLbl.Text = userSession.Nickname;
                diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
                UpdateExchangePanel(api.ActiveOrders);
                showExchangePanel();
            }
            else
            {
                MessageBox.Show("User login invalid or already in use!", "Diginote Exchange System");
            }
        }

        private void showExchangePanel()
        {
            loginPanel.Visible = false;
            registerPanel.Visible = false;
            ExchangePanel.Visible = true;
        }

        private void showInitialPanel()
        {
            loginPanel.Visible = true;
            registerPanel.Visible = true;
            ExchangePanel.Visible = false;
        }

        private void UpdateExchangePanel(List<DOrder> orders)
        {
            Num_Buy_Order_System.Text = orders.FindAll(order => order.Type == OrderType.Buy).Count.ToString();
            Num_Sell_Order_System.Text = orders.FindAll(order => order.Type == OrderType.Sell).Count.ToString();
            List<DOrder> aux = orders.FindAll(order => order.Source.Nickname.Equals(userSession.Nickname));
            foreach (DOrder order in aux)
            {
                var lvAdd = new LvAddDelegate(itemListView.Items.Add);
                ListViewItem lvItem = new ListViewItem(new string[] { order.Id.ToString(), order.Type.ToString(), order.Value.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), order.Status.ToString(), order.Date.ToString() });
                Invoke(lvAdd, new object[] { lvItem });
            }

            ExchangeValueLbl.Text = String.Format("{0:0.00}", api.ExchangeValue);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OrderType orderAction;
                int amount;

                if (!Enum.TryParse(this.comboBox1.GetItemText(this.comboBox1.SelectedItem), true, out orderAction))
                {
                    MessageBox.Show("Order action not allowed", "Diginote Exchange System");
                }
                else if (!Int32.TryParse(this.textBox6.Text, out amount))
                {
                    MessageBox.Show("Order amount parse error", "Diginote Exchange System");
                }
                else
                {
                    if(orderAction == OrderType.Sell && amount > userSession.wallet.FindAll(o => o.IsForSale==false).Count)
                    {
                        MessageBox.Show("You have insufficient diginotes\nto perform that action.", "Diginote Exchange System");
                    }
                    else
                    {
                        DOrder tempOrder = new DOrder(userSession, amount, api.ExchangeValue, orderAction, DateTime.Now);
                        api.RegisterOrder(ref tempOrder);
                    }
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception error:\n" + exception.Message, "Diginote Exchange System");
            }
            finally
            {
                this.comboBox1.SelectedItem = null;
                this.textBox6.Text = "";
            }
        }

        private void logout(object sender, FormClosedEventArgs e)
        {
            api.logout(ref userSession);
            api.alterEvent -= DelRepeater;
        }

        private void itemListView_ItemActivate(object sender, EventArgs e)
        {
            int id = Int32.Parse(itemListView.SelectedItems[0].SubItems[0].Text.ToString());
            string tipo = itemListView.SelectedItems[0].SubItems[1].Text.ToString();
            double valor = Double.Parse(itemListView.SelectedItems[0].SubItems[2].Text.ToString());

            OrderEditor editor = new OrderEditor(id, tipo, valor);
            editor.ShowDialog(this);

            //logica de negocio
            if (editor.updated == 1)
            {
                if (editor.type == "Sell" && editor.value > valor)
                {
                    MessageBox.Show("Value must be lower or equal to " + valor, "Diginote Exchange System");
                    return;
                }
                if (editor.type == "Buy" && editor.value < valor)
                {
                    MessageBox.Show("Value must be greater or equal to " + valor, "Diginote Exchange System");
                    return;
                }
                //update order
                int quantidade = Int32.Parse(itemListView.SelectedItems[0].SubItems[3].Text.ToString());
                OrderType tipoOrdem = (OrderType)Enum.Parse(typeof(OrderType), tipo);
                DateTime tempo = DateTime.Parse(itemListView.SelectedItems[0].SubItems[6].Text.ToString());
                DOrder tempOrder = new DOrder(userSession, quantidade, editor.value, tipoOrdem, tempo);
                tempOrder.Id = id;
                itemListView.SelectedItems[0].SubItems[2].Text = editor.value.ToString();
                MessageBox.Show("Valid update, new order: "+tempOrder.ToString());
                api.EditOrder(tempOrder);
            }
            else if (editor.updated == 2)
            {
                int quantidade = Int32.Parse(itemListView.SelectedItems[0].SubItems[3].Text.ToString());
                OrderType tipoOrdem = (OrderType)Enum.Parse(typeof(OrderType), tipo);
                DateTime tempo = DateTime.Parse(itemListView.SelectedItems[0].SubItems[6].Text.ToString());
                DOrder tempOrder = new DOrder(userSession, quantidade, valor, tipoOrdem, tempo);
                tempOrder.Id = id;
                //MessageBox.Show("Cancel:\n" + tempOrder.ToString());
                itemListView.SelectedItems[0].Remove();
                api.CancelOrder(tempOrder);
            }

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