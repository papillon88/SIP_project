using System;
using Ozeki.Network;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;

namespace SIP_Encryption
{
    class Program
    {
        static ISoftPhone softphone;
        static IPhoneLine phoneLine;

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
                var phoneLineConfig = new PhoneLineConfiguration(account);
                phoneLineConfig.TransportType = TransportType.Tls;
                phoneLine = softphone.CreatePhoneLine(phoneLineConfig);
                phoneLine.RegistrationStateChanged += line_RegStateChanged;
                softphone.RegisterPhoneLine(phoneLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration: " + ex.ToString());
            }
        }

        static void line_RegStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            if (e.State == RegState.NotRegistered || e.State == RegState.Error)
                Console.WriteLine("Registration failed!");

            if (e.State == RegState.RegistrationSucceeded)
                Console.WriteLine("Now TLS is encrypted within your softphone!");
        }
    }
}