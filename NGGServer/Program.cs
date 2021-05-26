/* Sources & Inspirations: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NGGServer
{
    public class StateObject
    {
        // Size of buffer read
        public const int BufferSize = 1024;
        
        // The receiving buffer
        public readonly byte[] Buffer = new byte[BufferSize];
        
        // The data string
        public readonly StringBuilder Sb = new();
        
        // Clients socket
        public Socket ClientSocket;
    }

    public static class AsynchronousSocketListener
    {
        private static readonly ManualResetEvent AllDone = new(false);

        public static void StartServer()
        {
            // Establish the local endpoint for the socket.
            // The DNS name of the server.
            // Run the listener
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            
            // Create the socket
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            
            // Bind the socket, listen for connections
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to a un-signaled state.
                    AllDone.Reset();

                    // Start Asynchronously listening for connections.
                    Console.WriteLine("Listening for connections.");
                    listener.BeginAccept(
                        AcceptCallback, listener);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            AllDone.Set();
            
            // Get the socket that handles client request.
            Socket listener = (Socket) ar.AsyncState;
            if (listener != null)
            {
                Socket handler = listener.EndAccept(ar);
                // Create the state object
                StateObject state = new StateObject();
                state.ClientSocket = handler;
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReadCallback, state);
            }
            else
            {
                throw new NullReferenceException("Socket Object is Null.");
            }
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the async socket
            StateObject state = (StateObject) ar.AsyncState;
            if (state != null)
            {
                Socket handler = state.ClientSocket;
            
                // Read data from the client socket
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There could be more data, so we store the data we have so far.
                    state.Sb.Append(Encoding.ASCII.GetString(
                        state.Buffer, 0, bytesRead));
                
                    // Check for EOF if its not here, we continue reading.
                    var content = state.Sb.ToString();
                    if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                    {
                        // Continue with game logic
                        // TODO Game logic.
                    }
                    else
                    {
                        // Data isn't finished, so continue reading the data in. (this should barely run)
                        handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                            ReadCallback, state);
                    }
                }
            }
            else
            {
                throw new NullReferenceException("State object is null.");
            }
        }

        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            
            // Begin the transmission of the socket objects. 
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state obkject
                Socket handler = (Socket) ar.AsyncState;

                // Complete sending data to remote device
                if (handler != null)
                {
                    int bytesSent = handler.EndSend(ar);
                    Console.WriteLine($"Sent {bytesSent} bytes to the client.");
                }
                else
                {
                    throw new NullReferenceException("Socket Object is Null.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Currently doesn't require an instance, because everything is stored inside a 
            // "StateObject"
            AsynchronousSocketListener.StartServer();
        }
    }
}