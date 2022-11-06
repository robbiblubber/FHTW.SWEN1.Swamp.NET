using System;
using System.Collections.Generic;



namespace FHTW.SWEN1.Swamp
{
    /// <summary>This class provides event arguments for an HTTP server.</summary>
    public class HttpSvrEventArgs: EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="tcp">HTTP message received from TCP listener.</param>
        public HttpSvrEventArgs(string tcp)
        {
            string[] lines = tcp.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
            bool inheaders = true;
            List<HttpHeader> headers = new List<HttpHeader>();

            for(int i = 0; i < lines.Length; i++)
            {
                if(i == 0)
                {
                    string[] inq = lines[0].Split(" ");
                    Method = inq[0];
                    Path = inq[1];
                }
                else if(inheaders)
                {
                    if(string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new HttpHeader(lines[i])); }
                }
                else
                {
                    Payload += (lines[i] + "\r\n");
                }

                Headers = headers.ToArray();
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Get the HTTP method.</summary>
        public string Method
        {
            get; private set;
        }


        /// <summary>Gets the URL path.</summary>
        public string Path
        {
            get; private set;
        }


        /// <summary>Gets the HTTP headers.</summary>
        public HttpHeader[] Headers
        {
            get; private set;
        }


        /// <summary>Gets the HTTP payload.</summary>
        public string Payload
        {
            get; private set;
        }
    }
}
