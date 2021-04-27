using System;
using System.Windows.Forms;

namespace WinForms.Windows
{
    public partial class Form_Enter : Form
    {
        private Form_Server formServer;
        private Form_Client formClient;

        public Form_Enter()
        {
            InitializeComponent();
        }


        private void Form_Enter_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedIndex)
            {
                case 0:
                    labelError.Text = "";
                    formServer = new Form_Server();
                    formServer.Show();
                    this.Hide();
                    break;

                case 1:
                    var test = textBox1.Text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (test.Length == 4)
                    {
                        foreach (var word in test)
                        {
                            if (int.TryParse(word, out _))
                            {
                                continue;
                            }
                            else
                            {
                                labelError.Text = "Неправильный IP";
                                return;
                            }
                        }
                        formClient = new Form_Client(textBox1.Text);
                        formClient.Show();
                        this.Hide();
                        labelError.Text = "";
                        return;
                    }
                    labelError.Text = "Неправильный IP";
                    labelError.Text = "";
                    break;

                default: labelError.Text = "Не выбран режим"; break;


            }
        }
    }
}
