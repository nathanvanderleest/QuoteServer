using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WinServices
{
    public class QuoteServer
    {
        private TcpListener listener;
        private int port;
        private string filename;
        private List<string> quotes;
        private Random random;
        private Thread listenerThread;

        public QuoteServer()
            : this("quotes.txt")
        {

        }

        public QuoteServer(string filename)
            : this(filename, 7890)
        {

        }

        public QuoteServer(string filename, int port)
        {
            this.filename = filename;
            this.port = port;
        }

        protected void ReadQuotes()
        {
            quotes = File.ReadAllLines(filename).ToList();
            random = new Random();
        }

        protected string GetRandomQuoteOfTheDay()
        {
            int index = random.Next(0, quotes.Count);
            return quotes[index];
        }

        public void Start()
        {
            ReadQuotes();
            listenerThread = new Thread(ListenerThread);
            listenerThread.IsBackground = false;
            listenerThread.Name = "Listener";
            listenerThread.Start();
        }
        protected void ListenerThread()
        {
            try
            {
                IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(ipaddress, port);
                listener.Start();
                while (true)
                {
                    Socket clientSocket = listener.AcceptSocket();
                    string message = GetRandomQuoteOfTheDay();
                    UnicodeEncoding encoder = new UnicodeEncoding();
                    byte[] buffer = encoder.GetBytes(message);
                    clientSocket.Send(buffer, buffer.Length, 0);
                    clientSocket.Close();
                }
            }
            catch (SocketException ex)
            {
                Trace.TraceError(String.Format("QuoteServer {0}", ex.Message));
            }
        }

        public void Stop()
        {
            listener.Stop();
        }

        public void Suspend()
        {
            listener.Stop();
        }

        public void Resume()
        {
            listener.Start();
        }

        public void RefreshQuotes()
        {
            ReadQuotes();
        }

    }
}
