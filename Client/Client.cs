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

namespace Client
{
    public partial class Client : Form
    {
        IAPI api;
        User userSession;
        readonly AlterEventRepeater _evRepeater;
        delegate ListViewItem LVAddDelegate(ListViewItem lvItem);
        delegate void ChCommDelegate(DOrder order);
        delegate void LVRemDelegate(DOrder order);

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
            Console.WriteLine("STUFFFFFF");
            Console.WriteLine(api.ActiveOrders.Count);
            //uncomment if not treating creator or order like a regular user
            //and updating the rest of the user's orders automatically
            if (/*!order.Source.Nickname.Equals(userSession.Nickname) &&*/
                        (!order.Value.Equals(api.ExchangeValue)))
            {
                //Create prompt to change the current value of your orders

                //User says yes or times out
                //api.ChangeAllUserOrders(userSession,newValue);

                //User says no
                //api.DeleteAllUserOrders(userSession);
            }

            if (userSession != null && !order.Source.Nickname.Equals(userSession.Nickname))
                return;

            switch (op)
            {
                case Operation.New:

                    var lvAdd = new LVAddDelegate(itemListView.Items.Add);
                    ListViewItem lvItem = new ListViewItem(new string[] { order.Id.ToString(), order.Type.ToString(), order.Value.ToString(), order.Amount.ToString(), (order.Value * order.Amount).ToString(), order.Status.ToString(), order.Date.ToString() }); 
                    Invoke(lvAdd, new object[] { lvItem });
                    break;
                case Operation.Change:
                    var chComm = new ChCommDelegate(ChangeOrder);
                    Invoke(chComm, new object[] { order });
                    break;
                case Operation.Remove:

                    break;
            }
        }

        private void ChangeOrder(DOrder order)
        {
            var i = api.ActiveOrders.FindIndex(o => order.Id == o.Id);

            if (order.Status.Equals(OrderStatus.Active))
            {
                if (i > -1) api.ActiveOrders[i] = order;
            }
            else
            {
                /*var lvRem = new LVRemDelegate(listView.Items.Remove);
                Invoke(lvRem, new object[] { order });*/
                api.ActiveOrders.RemoveAt(i);
            }
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

                /*api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);
                api.RegisterDiginote(aux);*/

                DOrder tempOrder = new DOrder(aux, 5, 5.0, OrderType.Buy, DateTime.Now);
                api.RegisterOrder(ref tempOrder);
                /*api.DeleteOrder(tempOrder);
                api.RegisterOrder(ref tempOrder);*/


                /*DTransaction tempTrans = new DTransaction(aux, 5.0, tempOrder);
                api.RegisterTransaction(tempTrans);
                api.DeleteTransaction(tempTrans);
                api.RegisterTransaction(tempTrans);

                api.GetOrder(tempOrder.Id);
                api.GetTransaction(tempTrans.Order.Id);*/

                tempOrder.Amount = 1333;
                api.EditOrder(tempOrder);

                //Só se vai buscar as orders no login bem sucedido
                //orders = api.ActiveOrders;
                //MessageBox.Show("Number of orders:"+orders.Count);
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
            //login user
            if ((userSession = api.ValidateUser(textBox4.Text, textBox5.Text)) != null)
            {
                MessageBox.Show("User login valid!", "Diginote Exchange System");
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
            //IAPI.getActiveTransactions();
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
                var lvAdd = new LVAddDelegate(itemListView.Items.Add);
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
                double value;

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
                if (!Double.TryParse(this.textBox7.Text, out value))
                {
                    this.textBox7.Text = "";
                    MessageBox.Show("Order value parse error", "Diginote Exchange System");
                }
                else
                {
                    DOrder tempOrder = new DOrder(userSession, amount, value, orderAction, DateTime.Now);
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
                OrderType tipoOrdem = (OrderType) Enum.Parse(typeof(OrderType), tipo);
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
                MessageBox.Show("Cancel:\n"+tempOrder.ToString());
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