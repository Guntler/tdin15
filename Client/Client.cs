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
        public Client()
        {
            RemotingConfiguration.Configure("Client.exe.config", false);
            InitializeComponent();
            api = (IAPI)RemoteNew.New(typeof(IAPI));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            User aux = new User(textBox1.Text, textBox2.Text, textBox3.Text);
            int result = api.registerUser(aux);
            MessageBox.Show("Hello World! Here is the output of the register action: "+result);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //login user
            if (api.validateUser(textBox4.Text, textBox5.Text))
                MessageBox.Show("User login valid!");
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