﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;



namespace FHTW.SWEN1.Swamp
{
    /// <summary>A delegate that represents a method that will handle an incoming HTTP message event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments that contain the event data.</param>
    public delegate void IncomingEventHandler(object sender, HttpSvrEventArgs e);



    /// <summary>This class implements a great HTTP server.</summary>
    public sealed class HttpSvr
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private TcpListener _Listener;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Occurs when a HTTP message is received.</summary>
        public event IncomingEventHandler Incoming;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the server.</summary>
        /// <remarks>Probably won't stay this way forever.</remarks>
        public void Run()
        {
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12000);
            _Listener.Start();

            byte[] buf = new byte[256];
            int n;
            string data;

            while(true)
            {
                TcpClient client = _Listener.AcceptTcpClient();                 // wait for a client to connect

                NetworkStream stream = client.GetStream();                      // get the client stream
                
                data = "";
                while(stream.DataAvailable || (data == ""))
                {                                                               // read and decode stream
                    n = stream.Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new HttpSvrEventArgs(data, client));
            }
        }
    }
}
