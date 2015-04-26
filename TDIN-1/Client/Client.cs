using System;
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
        delegate void timerDelegate(System.Timers.Timer aTimer);
        delegate void timerEventDelegate();
        Popup questionBox;
        System.Timers.Timer aTimer;
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

            if (userSession == null || (!order.Source.Nickname.Equals(userSession.Nickname) && !op.Equals(Operation.ChangeAll)))
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
                    chChomm = new ChCommDelegate(ChangeAllHandler);
                    Invoke(chChomm, new object[] { order });
                    break;
            }
        }

        private void addHandler(DOrder order)
        {
            UpdateExchangePanel();
        }

        private void notifyUI(DOrder order)
        {
            diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
            UpdateExchangePanel();
            MessageBox.Show("Order of id:" + order.Id + " has been updated.\n" + (order.Type == OrderType.Buy ? "Bought " : "Sold ") + order.Amount + " diginotes\n", "Diginote Exchange System");
        }

        private void updateUI(DOrder order)
        {
            Num_Buy_Order_System.Text = api.ActiveOrders.FindAll(o => o.Type == OrderType.Buy).Count.ToString();
            Num_Sell_Order_System.Text = api.ActiveOrders.FindAll(o => o.Type == OrderType.Sell).Count.ToString();
            ExchangeValueLbl.Text = String.Format("{0:0.00}", api.ExchangeValue);
            diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
        }

        private void updateOrder(DOrder ord)
        {
            itemListView.Items.Clear();
            var lista = api.ActiveOrders.FindAll(order => order.Source.Nickname.Equals(userSession.Nickname));
            foreach (DOrder o in lista)
            {
                itemListView.Items.Add(new ListViewItem(new string[] { o.Id.ToString(), o.Type.ToString(), o.Value.ToString(), o.Amount.ToString(), (o.Value * o.Amount).ToString(), o.Status.ToString(), o.Date.ToString() }));
            }
        }


        private void ChangeAllHandler(DOrder order)
        {
            string newValue = String.Format("{0:0.00}", api.ExchangeValue);
            var aux = api.GetActiveOrders().FindAll(o=> o.Source.Nickname.Equals(userSession.Nickname) && !o.Value.Equals(api.ExchangeValue));
            if (aux.Count>0) //mudança de exchangeValue
            {
                questionBox = new Popup("Would you accept the new exchange value: " + api.ExchangeValue + "?\n");
                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 60000;
                aTimer.Enabled = true;
                aTimer.Start();
                questionBox.ShowDialog(this);

                if (questionBox.accept)
                {
                    aTimer.Stop();
                    api.ChangeAllUserOrders(this.userSession, api.ExchangeValue);
                    UpdateExchangePanel();
                }
                else
                {
                    aTimer.Stop();
                    api.DeleteAllUserOrders(this.userSession);
                    UpdateExchangePanel();
                }
            }
        }

        void OnTimedEvent(object source, ElapsedEventArgs e) {
            aTimer.Stop();
            var chChomm = new timerEventDelegate(closeBox);
            Invoke(chChomm, new object[] {});
        }

        void closeBox()
        {
            questionBox.Close();
            api.ChangeAllUserOrders(this.userSession, api.ExchangeValue);
            UpdateExchangePanel();
        }

        private void RemoveHandler(DOrder ord)
        {
            diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
            UpdateExchangePanel();
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
                MessageBox.Show("Something went wrong:\n" + error.Message, "Diginote Exchange System");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((userSession = api.ValidateUser(textBox4.Text, textBox5.Text)) != null)
            {
                UserLbl.Text = userSession.Nickname;
                diginotesLbl.Text = api.GetDiginotesByUser(this.userSession).FindAll(o => o.IsForSale == false).Count.ToString();
                UpdateExchangePanel();
                showExchangePanel();
                ChangeAllHandler(new DOrder(this.userSession,0,0,OrderType.Sell,DateTime.Now)); //force update value of order
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

        private void UpdateExchangePanel()
        {
            Num_Buy_Order_System.Text = api.ActiveOrders.FindAll(order => order.Type == OrderType.Buy).Count.ToString();
            Num_Sell_Order_System.Text = api.ActiveOrders.FindAll(order => order.Type == OrderType.Sell).Count.ToString();
            var aux = api.ActiveOrders.FindAll(order => order.Source.Nickname.Equals(userSession.Nickname));
            itemListView.Items.Clear();
            var lista = api.ActiveOrders.FindAll(order => order.Source.Nickname.Equals(userSession.Nickname));
            foreach (DOrder o in lista)
            {
                itemListView.Items.Add(new ListViewItem(new string[] { o.Id.ToString(), o.Type.ToString(), o.Value.ToString(), o.Amount.ToString(), (o.Value * o.Amount).ToString(), o.Status.ToString(), o.Date.ToString() }));
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
                    if (orderAction == OrderType.Sell && amount > userSession.wallet.FindAll(o => o.IsForSale == false).Count)
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
                api.EditOrder(tempOrder);
            }
            else if (editor.updated == 2)
            {
                int quantidade = Int32.Parse(itemListView.SelectedItems[0].SubItems[3].Text.ToString());
                OrderType tipoOrdem = (OrderType)Enum.Parse(typeof(OrderType), tipo);
                DateTime tempo = DateTime.Parse(itemListView.SelectedItems[0].SubItems[6].Text.ToString());
                DOrder tempOrder = new DOrder(userSession, quantidade, valor, tipoOrdem, tempo);
                tempOrder.Id = id;
                itemListView.SelectedItems[0].Remove();
                api.CancelOrder(tempOrder);
            }

        }

        private void label12_Click(object sender, EventArgs e)
        {

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