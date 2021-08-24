using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chatik
{
    public partial class Form1 : Form
    {
        bool alive = false;//Будет ли работать поток для приема
        UdpClient client;
        const int localport = 8005; //Порт для приема
        const int remoteport = 8005; //Порт для отправки
        const int ttl = 20;
        IPAddress groupAddress;
        const string host = "235.5.5.1";
        string userName;
        public Form1()
        {
            InitializeComponent();

            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            richTextBox1.ReadOnly = true;

            groupAddress = IPAddress.Parse(host);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExitChat();
        }

        private void ExitChat()
        {
            string mess = userName + " с грустью покидает этот мега чатик";
            byte[] data = Encoding.Unicode.GetBytes(mess);
            client.Send(data, data.Length, host, remoteport);
            client.DropMulticastGroup(groupAddress);

            alive = false;
            client.Close();

            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e) //Join
        {
            userName = textBox1.Text;
            textBox1.ReadOnly = true;
            groupAddress = IPAddress.Parse(host);

            try
            {
                client = new UdpClient(localport);
                //Присоединяемся к групповой рассылке
                client.JoinMulticastGroup(groupAddress, ttl);

                

                //Запускаем задачу на прием сообщений
                Task receiveTask = new Task(ReceiveMessage);
                receiveTask.Start();

                //Отправляем первое сообщение о входе нового пользователя
                string mess = userName + " присоединился в мега чатик";
                byte[] data = Encoding.Unicode.GetBytes(mess);
                client.Send(data, data.Length, host, remoteport);

                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Метод приема сообщений
        private void ReceiveMessage()
        {
            alive = true;
            try
            {
                while(alive)
                {
                    IPEndPoint remoteIP = null;
                    byte[] data = client.Receive(ref remoteIP);
                    string mess = Encoding.Unicode.GetString(data);

                    //Добавляем полученное сообщение в текстовое поле
                    this.Invoke(new MethodInvoker(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        richTextBox1.Text = time + " " + mess + "\r\n" + richTextBox1.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e) //Send
        {
            try
            {
                string mess = String.Format("{0}: {1}", userName, richTextBox2.Text);
                byte[] data = Encoding.Unicode.GetBytes(mess);
                client.Send(data, data.Length, host, remoteport);
                richTextBox2.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
