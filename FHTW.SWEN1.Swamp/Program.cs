using System;



namespace FHTW.SWEN1.Swamp
{
    /// <summary>This is the program class for the application.</summary>
    public static class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // main entry point                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            HttpSvr svr = new HttpSvr();
            svr.Incoming += _Svr_Incoming;

            svr.Run();
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // event handlers                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Processes an incoming HTTP request.</summary>
        private static void _Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
