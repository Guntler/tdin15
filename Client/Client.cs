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

namespace Client
{
    public partial class Client : Form
    {
        IAPI api;
        User userSession;
        readonly AlterEventRepeater _evRepeater;
        delegate ListViewItem LvAddDelegate(ListViewItem lvItem);
        delegate void LVRemoveDelegate(ListViewItem lvItem);
        delegate ListViewItem[] LVFindDelegate(string key, bool searchAllSubItems);
        delegate void ChCommDelegate(DOrder order);
        delegate void LvRemDelegate(DOrder order);

        public Client()
        {
            RemotingConfiguration.Configure("Client.exe.config", false);
            InitializeComponent();
            api = (IAPI)RemoteNew.New(typeof(IAPI));
            ExchangePanel.Parent = this;

            _evRepeater = new AlterEventRepeater();
            _evRepeater.alterEvent += new AlterDelegate(OperationHandler);
            api.alterEvent += new AlterDelegate(_evRepeater.Repeater);
        }

        public void OperationHandler(Operation op, DOrder order)
        {
            if (userSession != null && !order.Source.Nickname.Equals(userSession.Nickname))
                return;

            switch (op)
            {
                case Operation.New:
                    var lvAdd = new LvAddDelegate(itemListView.Items.Add);
                    ListViewItem lvItem = new ListViewItem(new string[] { order.Id.ToString(), order.Type.ToString(), order.Value.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), order.Status.ToString(), order.Date.ToString() });
                    Invoke(lvAdd, new object[] { lvItem });
                    break;
                case Operation.Change:
                    var chChomm = new ChCommDelegate(ChangeHandler);
                    Invoke(chChomm, new object[] { order });
                    break;
                case Operation.Remove:
                    var lvRem = new LvRemDelegate(RemoveHandler);
                    Invoke(lvRem, new object[] { order });
                    break;
                case Operation.Notify:
                    //Notify User that they have sold X diginotes
                    DOrder aux = api.ActiveOrders.Find(o=> o.Id == order.Id);
                    MessageBox.Show("Order of id:"+order.Id+" has been updated.\n Sold "+(order.Amount-aux.Amount)+" diginotes\n");
                    break;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            api.ChangeAllUserOrders(this.userSession, api.ExchangeValue);
        }

        private void ChangeHandler(DOrder order)
        {
            if (!api.ExchangeValue.ToString().Equals(ExchangeValueLbl.ToString())) //mudança de exchangeValue
            {
                double newValue = api.ExchangeValue;
                ExchangeValueLbl.Text = String.Format("{0:0.00}", newValue);

                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 1000;
                aTimer.Enabled = true;

                DialogResult dialogResult = MessageBox.Show("Would you accept the new exchange value:" + newValue + "?\n", "New exchange value!", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    aTimer.Stop();
                    api.ChangeAllUserOrders(this.userSession, newValue);
                    var lvFind = new LVFindDelegate(itemListView.Items.Find);
                    ListViewItem lvItem = new ListViewItem(new string[] { order.Id.ToString(), order.Type.ToString(), order.Value.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), order.Status.ToString(), order.Date.ToString() });
                    Invoke(lvFind, new object[] { lvItem });

                    foreach (ListViewItem item in itemListView.Items)
                    {
                        item.SubItems[2].Text = newValue.ToString();
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    aTimer.Stop();
                    api.DeleteAllUserOrders(this.userSession);
                    var lvRemove = new LVRemoveDelegate(itemListView.Items.Remove);
                    foreach (ListViewItem item in itemListView.Items)
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
            foreach (ListViewItem item in itemListView.Items)
            {
                if (order.Id.ToString().Equals(item.Text))
                {
                    item.Remove();
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                User aux = new User(textBox1.Text, textBox2.Text, textBox3.Text);
                int result = api.RegisterUser(ref aux);
            }
            catch (Exception error)
            {
                MessageBox.Show("Something went wrong:\n" + error.Message);
            }
            finally
            {
                //clear text
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((userSession = api.ValidateUser(textBox4.Text, textBox5.Text)) != null)
            {
                UserLbl.Text = userSession.Nickname;
                diginotesLbl.Text = userSession.wallet.Count.ToString();
                Console.WriteLine(api.ActiveOrders.Count);
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
                    this.comboBox1.SelectedItem = null;
                    MessageBox.Show("Order action not allowed", "Diginote Exchange System");
                }
                if (!Int32.TryParse(this.textBox6.Text, out amount))
                {
                    this.textBox6.Text = "";
                    MessageBox.Show("Order amount parse error", "Diginote Exchange System");
                }
                else
                {
                    DOrder tempOrder = new DOrder(userSession, amount, api.ExchangeValue, orderAction, DateTime.Now);
                    api.RegisterOrder(ref tempOrder);
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception error:\n" + exception.Message, "Diginote Exchange System");
            }
            finally
            {

            }
        }

        private void logout(object sender, FormClosedEventArgs e)
        {
            api.logout(ref userSession);
        }

        private void itemListView_ItemActivate(object sender, EventArgs e)
        {
            int id = Int32.Parse(itemListView.SelectedItems[0].SubItems[0].Text.ToString());
            string tipo = itemListView.SelectedItems[0].SubItems[1].Text.ToString();
            double valor = Double.Parse(itemListView.SelectedItems[0].SubItems[2].Text.ToString());

            OrderEditor editor = new OrderEditor(id, tipo, valor);
            editor.ShowDialog(this);

            //logica de negocio
            if (editor.updated)
            {
                if (editor.type == "Sell" && editor.value > valor)
                {
                    MessageBox.Show("Value must be lower or equal to " + valor, "Diginote Exchange System");
                }
                else if (editor.type == "Buy" && editor.value < valor)
                {
                    MessageBox.Show("Value must be greater or equal to " + valor, "Diginote Exchange System");
                }
                //update order
                int quantidade = Int32.Parse(itemListView.SelectedItems[0].SubItems[3].Text.ToString());
                OrderType tipoOrdem = (OrderType)Enum.Parse(typeof(OrderType), tipo);
                DateTime tempo = DateTime.Parse(itemListView.SelectedItems[0].SubItems[6].Text.ToString());
                DOrder tempOrder = new DOrder(userSession, quantidade, editor.value, tipoOrdem, tempo);
                tempOrder.Id = id;
                MessageBox.Show("Update:\n" + tempOrder.ToString());
                api.EditOrder(tempOrder);
            }
            else
            {
                int quantidade = Int32.Parse(itemListView.SelectedItems[0].SubItems[3].Text.ToString());
                OrderType tipoOrdem = (OrderType)Enum.Parse(typeof(OrderType), tipo);
                DateTime tempo = DateTime.Parse(itemListView.SelectedItems[0].SubItems[6].Text.ToString());
                DOrder tempOrder = new DOrder(userSession, quantidade, valor, tipoOrdem, tempo);
                tempOrder.Id = id;
                MessageBox.Show("Cancel:\n" + tempOrder.ToString());
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