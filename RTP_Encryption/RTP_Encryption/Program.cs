using System;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;

namespace RTP_Encryption
{
    class Program
    {
        static ISoftPhone softphone;   // softphone object
        static IPhoneLine phoneLine;   // phoneline object

        private static void Main(string[] args)
        {
            // Create a softphone object with RTP port range 5000-10000
            softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);

            // SIP account registration data, (supplied by your VoIP service provider)
            var registrationRequired = true;
            var userName = "iamboss";
            var displayName = "iamboss";
            var authenticationId = "iamboss";
            var registerPassword = "qwerty";
            var domainHost = "sip.linphone.org";
            var domainPort = 5060;

            var account = new SIPAccount(registrationRequired, displayName, userName, authenticationId, registerPassword, domainHost, domainPort);

            // Send SIP regitration request
            RegisterAccount(account);

            // Prevents the termination of the application
            Console.ReadLine();
        }

        static void RegisterAccount(SIPAccount account)
        {
            try
            {
                phoneLine = softphone.CreatePhoneLine(account);
                phoneLine.Config.SRTPMode = SRTPMode.Force;
                phoneLine.RegistrationStateChanged += sipAccount_RegStateChanged;
                softphone.RegisterPhoneLine(phoneLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration: " + ex);
            }
        }

        static void sipAccount_RegStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            if (e.State == RegState.Error || e.State == RegState.NotRegistered)
                Console.WriteLine("Registration failed!");

            if (e.State == RegState.RegistrationSucceeded)
                Console.WriteLine("Registration succeeded - RTP Connection is encrypted!");
        }
    }
}
