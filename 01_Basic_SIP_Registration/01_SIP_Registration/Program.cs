using System;
#region Ozeki VoIP SIP SDK Namespaces
using Ozeki.VoIP;
#endregion

namespace _01_SIP_Registration
{
    /// <summary>
    /// This class will introduce the use of a softphone and handes console events and user interactions.
    /// </summary>
    /// <remarks>
    /// In this sample the following features of the SDK will be presented:
    /// - Register a SIP account to the PBX
    /// </remarks>
    class Program
    {
        private static Softphone mySoftphone;   // softphone object

        /// <summary>
        /// The entry point of the program.
        /// - initializes the softphone
        /// - shows a greeting message
        /// - registers the SIP account
        /// </summary>
        private static void Main(string[] args)
        {
            InitSoftphone();
            ShowGreetingMessage();
            ReadRegisterInfos();

            // prevents the termination of the application
            BlockExit();
        }


        /// <summary>
        /// Initializes the softphone logic and subscribes to its events to get notifications from it.
        /// (eg. the registration state of the phone line has changed)
        /// </summary>
        private static void InitSoftphone()
        {
            mySoftphone = new Softphone();
            mySoftphone.PhoneLineStateChanged += mySoftphone_PhoneLineStateChanged;
        }


        /// <summary>
        /// This will be called when the registration state of the phone line has changed.
        /// </summary>
        /// <remarks>
        /// We need to be notified if our phone line is available to use, but we can also follow it's states.
        /// </remarks>
        static void mySoftphone_PhoneLineStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            Console.WriteLine("Phone line state changed: {0}", e.State);

            if (e.State == RegState.Error || e.State == RegState.NotRegistered)
                ReadRegisterInfos();

            if (e.State == RegState.RegistrationSucceeded)
            {
                Console.WriteLine("Registration succeeded - Online!");
            }
        }


        /// <summary>
        /// Writes a simple greeting message to the console.
        /// </summary>
        private static void ShowGreetingMessage()
        {
            Console.WriteLine("This is a simple Ozeki VoIP SIP SDK demo written in C#.");
            Console.WriteLine("It can be used to register to a PBX by using SIP account.");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();
        }


        /// <summary>
        /// Reads the SIP account information from the standard user input (some of these have default values).
        /// If the softphone cannot be registered, the user will be asked about the correct informations again.
        /// </summary>
        private static void ReadRegisterInfos()
        {
            var registrationRequired = true;
            Console.WriteLine("\nPlease set up Your SIP account:\n");

            // Asks, if a registration is required to the PBX. The default value is true.
            Console.Write("Please set if the registration is required (true/false) (default: true): ");
            var regRequired = Read("Registration required", false);
            if (regRequired.ToLower() == "false" || regRequired.ToLower() == "no" ||
                 regRequired.ToLower() == "n")
            {
                registrationRequired = false;
            }
            else
            {
                Console.WriteLine("Registration set to required.");
            }


            // The SIP account needs and authentication ID, and some names as well.
            Console.Write("Please set Your authentication ID: ");
            var authenticationId = Read("Authentication ID", true);

            // If the user only presses the Enter button, the username will be the same as the authentication ID
            Console.Write("Please set Your username (default: " + authenticationId + "): ");
            var userName = Read("Username", false);
            if (string.IsNullOrEmpty(userName))
                userName = authenticationId;

            // If the user only presses the Enter button, the display name will be the same as the authentication ID
            Console.Write("Please set Your name to be displayed (default: " + authenticationId + "): ");
            var displayName = Read("Display name", false);
            if (string.IsNullOrEmpty(displayName))
                displayName = authenticationId;

            // The registration password needs to be entered.
            Console.Write("Please set Your registration password: ");
            var registerPassword = Read("Password", true);

            // Domain name as a string, for example an IP adress.
            Console.Write("Please set the domain name: ");
            var domainHost = Read("Domain name", true);

            // Port number with the as 5060 default value.
            Console.Write("Please set the port number (default: 5060): ");
            int domainPort;
            string port = Read("Port", false);
            if (string.IsNullOrEmpty(port))
            {
                domainPort = 5060;
            }
            else
            {
                domainPort = Int32.Parse(port);
            }
            Console.WriteLine("\nCreating SIP account and trying to register....\n");

            // When every information has been given, we are trying to register to the PBX with the softphone's Register() method.
            mySoftphone.Register(registrationRequired, displayName, userName, authenticationId, registerPassword,
                                 domainHost, domainPort);
        }


        /// <summary>
        /// A helper method for reading the inputs. Even handles, if an information is necessary.
        /// </summary>
        private static string Read(string inputName, bool readWhileEmpty)
        {

            while (true)
            {
                string input = Console.ReadLine();

                if (!readWhileEmpty)
                    return input;

                if (!string.IsNullOrEmpty(input))
                    return input;

                Console.WriteLine(inputName + " cannot be empty.");
                Console.Write(inputName + ": ");
            }
        }


        /// <summary>
        /// Won't let the application to exit.
        /// </summary>
        private static void BlockExit()
        {
            Console.ReadLine();
        }
    }
}
