﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// An abstract class for servers
    /// </summary>
    public abstract class Server
    {
        #region Fields
        IPAddress iPAddress;
        int port;
        int buffer_size = 16;
        bool running;
        TcpListener tcpListener;
        #endregion

        #region Properties
        /// <summary>
        /// Access to the IP address of a Server instance. Cannot be changed when the Server is running
        /// </summary>
        public IPAddress IPAddress { get => iPAddress; set { if (!running) iPAddress = value; else throw new Exception("You cannot change IP address when server is running"); } }
        
        /// <summary>
        /// Access to the port of a server instance. Cannot be changed when the Server is running. Checks if given number can be accepted
        /// </summary>
        public int Port { get => port; set {
                int previous = port;
                if (!running) port = value; else throw new Exception("You cannot change port when server is running");
                if (!checkPort())
                {
                    port = previous;
                    throw new Exception("Invalid port value.");
                }
            }
        }

        /// <summary>
        /// This property gives access to the buffer size of a server instance. Property can't be changed when the Server is running. Setting invalid size numbers will cause an exception. 
        /// </summary>
        public int Buffer_size
        {
            get => buffer_size; set
            {
                if (value < 0 || value > 1024 * 1024 * 64) throw new Exception("Invalid buffer size");
                if (!running) buffer_size = value; else throw new Exception("You cannot change buffer size when server is running");
            }
        }

        protected TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }
        //protected TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }
       // protected NetworkStream Stream { get => stream; set => stream = value; }
        #endregion

        #region Constructors
        /// <summary>
        /// A default constructor. It doesn't start the server. Invalid port numbers will thrown an exception.
        /// </summary>
        /// <param name="IP">IP address of the server instance.</param>
        /// <param name="port">Port number of the server instance.</param>
        public Server(string IP, int port)
        {
            running = false;
            Port = port;

            if (ValidateIPv4(IP))
            {
                iPAddress = IPAddress.Parse(IP);
            }
            else
            {
                throw new Exception("Invalid IP value.");
            }

            if (!checkPort())
            {
                Port = 2048;
                throw new Exception("Invalid port value. 2048 was set.");
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// This function will return false if Port is set to a value lower than 1024 or higher than 49151.
        /// </summary>
        /// <returns>An information wether the set Port value is valid.</returns>
        protected bool checkPort()
        {
            if (port < 1024 || port > 49151) return false;
            return true;
        }
        /// <summary>
        /// This function will return false if ipString is not a valid ip address.
        /// </summary>
        /// <param name="ipString"></param>
        /// <returns>An information wether the set IPAddress value is valid.</returns>
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        /// <summary>
        /// This function starts the listener.
        /// </summary>
        protected void StartListening()
        {
            TcpListener = new TcpListener(IPAddress, Port);
            TcpListener.Start();
        }

        /// <summary>
        /// This function waits for the Client connection.
        /// </summary>
        protected abstract void AcceptClient();
        /// <summary>
        /// This function implements Echo and transmits the data between server and client.
        /// </summary>
        protected abstract void Run(TcpClient tcpClient);
        /// <summary>
        /// This function fires off the default server behaviour. It interrupts the program.
        /// </summary>
        public abstract void Start();
        #endregion
    }
}

