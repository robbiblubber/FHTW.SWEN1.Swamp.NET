using System;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Threading;



namespace FHTW.SWEN1.Swamp
{
    /// <summary>This is the program class for the application.</summary>
    public static class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Database connection.</summary>
        private static IDbConnection _Cn;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // main entry point                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            InitDb();

            HttpSvr svr = new HttpSvr();
            svr.Incoming += _Svr_Incoming;

            svr.Run();
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes the database connection.</summary>
        public static void InitDb()
        {
            _Cn = new SQLiteConnection("Data Source=swamp.db;Version=3;");
            _Cn.Open();
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static methods                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Reads messages from database.</summary>
        /// <param name="msg">Message number.</param>
        /// <returns>Message text.</returns>
        private static string _ReadMessages(int msg = -1)
        {
            IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM MESSAGES";
            
            if(msg >= 0) 
            {
                cmd.CommandText += " WHERE ID = :id";
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = ":id";
                p.Value = msg;
                cmd.Parameters.Add(p);
            }
            IDataReader re = cmd.ExecuteReader();

            StringBuilder rval = new StringBuilder();
            while (re.Read())
            {
                rval.Append("[");
                rval.Append(re.GetInt32(0));
                rval.Append("] ");
                rval.AppendLine(re.GetString(re.GetOrdinal("DATA")));
            }

            re.Close();
            re.Dispose();
            cmd.Dispose();

            return rval.ToString();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // event handlers                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Processes an incoming HTTP request.</summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        public static void _Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_Svr_Incoming, e);
        }


        /// <summary>Processes an incoming HTTP request.</summary>
        /// <param name="evt">Event arguments.</param>
        public static void _Svr_Incoming(object evt)
        {
            HttpSvrEventArgs e = (HttpSvrEventArgs) evt;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(e.PlainMessage);
            Console.ForegroundColor = ConsoleColor.White;

            if(e.Path == "/messages")
            {                                                                   // no message id provided
                if(e.Method == "POST")
                {                                                               // POST: add new message
                    IDbCommand cmd = _Cn.CreateCommand();                       // create database command, insert message into database
                    cmd.CommandText = "INSERT INTO MESSAGES (DATA) VALUES (:m)";
                    IDataParameter p = cmd.CreateParameter();                   // make and bind parameter for message body
                    p.ParameterName = ":m";
                    p.Value = e.Payload;                    
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    cmd = _Cn.CreateCommand();                                  // read autoincremented message ID
                    cmd.CommandText = "SELECT LAST_INSERT_ROWID()";
                    int id = Convert.ToInt32(cmd.ExecuteScalar());              // put ID into variable
                    cmd.Dispose();

                    Console.WriteLine("Saved message \"{0}\" as {1}", e.Payload.Replace("\n", "").Replace("\r", ""), id);
                    e.Reply(200, id.ToString());                                // create reply
                }
                else if(e.Method == "GET")
                {
                    Console.WriteLine("Showed all messages.");
                    e.Reply(200, _ReadMessages());
                }
            }
            else if(e.Path.StartsWith("/messages/"))
            {
                int msg = -1;
                int.TryParse(e.Path.Substring(10), out msg);

                if(msg == -1)
                {
                    Console.WriteLine("Request malformed.");
                    e.Reply(400);
                }
                else
                {
                    if(e.Method == "GET")
                    {
                        string data = _ReadMessages(msg);

                        if(string.IsNullOrEmpty(data))
                        {
                            Console.WriteLine("Message #" + msg + " not available.");
                            e.Reply(404);
                        }
                        else
                        {
                            Console.WriteLine("Showed message #" + msg + ".");
                            e.Reply(200, _ReadMessages(msg));
                        }
                    }
                    else if(e.Method == "PUT")
                    {
                        IDbCommand cmd = _Cn.CreateCommand();                       // create database command, insert message into database
                        cmd.CommandText = "UPDATE MESSAGES SET DATA = :m WHERE ID = :id";
                        IDataParameter p = cmd.CreateParameter();                   // make and bind parameter for message body
                        p.ParameterName = ":m";
                        p.Value = e.Payload;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = msg;
                        cmd.Parameters.Add(p);

                        int n = cmd.ExecuteNonQuery();
                        cmd.Dispose();

                        if(n < 1)
                        {
                            Console.WriteLine("Failed to update nonexistent message.");
                            e.Reply(404);
                        }
                        else
                        {
                            Console.WriteLine("Updated message #" + msg + ".");
                            e.Reply(200);
                        }
                    }
                    else if(e.Method == "DELETE")
                    {
                        IDbCommand cmd = _Cn.CreateCommand();                       // create database command, insert message into database
                        cmd.CommandText = "DELETE FROM MESSAGES WHERE ID = :id";
                        IDataParameter p = cmd.CreateParameter();                   // make and bind parameter for message body
                        p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = msg;
                        cmd.Parameters.Add(p);

                        int n = cmd.ExecuteNonQuery();
                        cmd.Dispose();

                        if (n < 1)
                        {
                            Console.WriteLine("Failed to delete nonexistent message.");
                            e.Reply(404);
                        }
                        else
                        {
                            Console.WriteLine("Deleted message #" + msg + ".");
                            e.Reply(200);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Rejected message.");
                e.Reply(400);
            }

            Console.WriteLine();
        }
    }
}
