/* Sources & Inspirations: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example */
/* Sources & Inspirations: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace NGGServer
{ 
    [Serializable]
    public class PlayerLimit : Exception
    {
        public PlayerLimit(): base() { }
        public PlayerLimit(string message): base(message) { }
        public PlayerLimit(string message, Exception inner) : base(message, inner) { }
        
        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected PlayerLimit(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ValueException : Exception
    {
        public ValueException(): base() { }
        public ValueException(string message): base(message) { }
        public ValueException(string message, Exception inner): base(message, inner) { }
        
        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected ValueException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class GameObject
    {
        // Reference the Games By ID
        private int _gameId;
        
        // Generic Settings for the Game
        private static int _maxNum;
        private static int _minNum;
        private int _maxPlayers = 5;

        private readonly Dictionary<string, int> _settings = new Dictionary<string, int>();
        
        // Bool to detect if a Game is in Win state.
        public static bool HasWon = false;
        
        // Number of current players.
        private int _currentPlayers = 0;

        private static int _numberToGuess;

        public GameObject(int gameId, int maxNum, int minNum)
        {
            _gameId = gameId;
            SetMinNumber(minNum);
            SetMaxNumber(maxNum);
            _numberToGuess = new Random().Next(_minNum, _maxNum);
            _settings.Add("MaxNum", maxNum);
            _settings.Add("MinNum", minNum);
            _settings.Add("MaxPlayers", _maxPlayers);
        }
        public GameObject(int gameId, int maxNum, int minNum, int maxPlayers)
        {
            _gameId = gameId;
            _maxPlayers = maxPlayers;
            SetMinNumber(minNum);
            SetMaxNumber(maxNum);
            _numberToGuess = new Random().Next(_minNum, _maxNum);
            _settings.Add("MaxNum", maxNum);
            _settings.Add("MinNum", minNum);
            _settings.Add("MaxPlayers", _maxPlayers);
        }

        public void NewPlayer()
        {
            if (_currentPlayers + 1 <= _maxPlayers)
            {
                _currentPlayers++;
            }
            else
            {
                throw new PlayerLimit($"This game is full. Current Players: {_currentPlayers}");
            }
        }

        private void SetMaxNumber(int newMaxNum)
        {
            if (newMaxNum >= _minNum)
            {
                _maxNum = newMaxNum;
                _settings["MaxNum"] = newMaxNum;
            }
            else
            {
                throw new ValueException("Maximum Random Number cannot be higher than Minimum Random Number.");
            }
        }

        private void SetMinNumber(int newMinNum)
        {
            if (newMinNum <= _maxNum)
            {
                _minNum = newMinNum;
                _settings["MinNum"] = newMinNum;
            }
            else
            {
                throw new ValueException("Minimum Random Number cannot be lower than Maximum Random Number.");
            }
        }

        public Dictionary<string, int> GetSettings()
        {
            return _settings;
        }

        public bool CheckWin(int guess)
        {
            if (guess != _numberToGuess) return false;
            HasWon = true;
            return true;
        }
        
    }
    public class StateObject
    {
        // Size of buffer read
        public const int BufferSize = 1024;
        
        // The receiving buffer
        public readonly byte[] Buffer = new byte[BufferSize];
        
        // The data string
        public readonly StringBuilder Sb = new();
        
        // The ID of the Game that the state is associated with.
        public int GameId;
        
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