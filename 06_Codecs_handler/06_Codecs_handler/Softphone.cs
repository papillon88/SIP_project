﻿using System;
using System.Collections.Generic;
using System.Threading;
using Ozeki.Media.Codec;
using Ozeki.Media.MediaHandlers;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;

namespace _06_Codecs_handler
{
    /// <summary>
    /// Basic softphone logic.
    /// </summary>
    /// <remarks>
    /// This class is used to introduce how to declare, define and initialize a softphone,
    /// how to handle some of it's events, and use some of it's functions.
    /// The Program.cs uses this class to create a softphone,
    /// uses the functions and events declared here as public.
    /// </remarks>
    class Softphone
    {
        ISoftPhone softphone;                   // softphone object
        IPhoneLine phoneLine;                   // phone line object
        IPhoneCall call;                        // the call object
        Microphone microphone;
        Speaker speaker;
        MediaConnector connector;               // connects the devices to each other (eg. microphone, speaker, mediaSender, mediaReceiver)
        PhoneCallAudioSender mediaSender;       // after connected with the microphone, this will be attached to the call
        PhoneCallAudioReceiver mediaReceiver;   // after connected with the speaker, this will be attached to the call

        bool incomingCall;  // indicates wheter we have an incoming call (so, the phone is ringing)

        #region Events

        /// <summary>
        /// Occurs when an incoming call received.
        /// </summary>
        public event EventHandler IncomingCall;

        /// <summary>
        /// Occurs when the registration state of the phone line has changed.
        /// </summary>
        public event EventHandler<RegistrationStateChangedArgs> PhoneLineStateChanged;

        /// <summary>
        /// Occurs when the state of the call has changed.
        /// </summary>
        public event EventHandler<CallStateChangedArgs> CallStateChanged;

        #endregion


        /// <summary>
        /// Default constructor, initalizes the softphone with deafult parameters.
        /// </summary>
        public Softphone()
        {
            softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);

            microphone = Microphone.GetDefaultDevice();
            speaker = Speaker.GetDefaultDevice();
            connector = new MediaConnector();
            mediaSender = new PhoneCallAudioSender();
            mediaReceiver = new PhoneCallAudioReceiver();

            incomingCall = false;
        }

        /// <summary>
        /// Registers the SIP account to the PBX. 
        /// Calls cannot be made while the SIP account is not registered.
        /// If the SIP account requires no registration, the RegisterPhoneLine() must be called too to register the SIP account to the ISoftPhone.
        /// </summary>
        public void Register(bool registrationRequired, string displayName, string userName, string authenticationId, string registerPassword, string domainHost, int domainPort)
        {
            try
            {
                // To register to a PBX, we need to create a SIP account
                var account = new SIPAccount(registrationRequired, displayName, userName, authenticationId, registerPassword, domainHost, domainPort);
                Console.WriteLine("\nCreating SIP account {0}", account);

                // With the SIP account and the NAT configuration, we can create a phoneline.
                phoneLine = softphone.CreatePhoneLine(account);
                Console.WriteLine("Phoneline created.");
                // The phoneline has states, we need to handle the event, when it is being changed.
                phoneLine.RegistrationStateChanged += phoneLine_PhoneLineStateChanged;

                // If our phoneline is created, we can register that.
                softphone.RegisterPhoneLine(phoneLine);

                // For further information about the calling of the ConnectMedia(), please check the implementation of this method.
                ConnectMedia();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration: " + ex.ToString());
            }
        }


        /// <summary>
        /// This will be called when the registration state of the phone line has changed.
        /// </summary>
        private void phoneLine_PhoneLineStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            DispatchAsync(() =>
            {
                var handler = PhoneLineStateChanged;
                if (handler != null)
                    handler(this, e);
            });
        }


        /// <summary>
        /// Starts the capturing and playing audio/video devices.
        /// Other devices can be used (and started), for example: WebCamera or WaveStreamPlayback.
        /// </summary>
        private void StartDevices()
        {
            if (microphone != null)
            {
                microphone.Start();
            }

            if (speaker != null)
            {
                speaker.Start();
            }
        }


        /// <summary>
        /// Stops the capturing and playing audio/video devices.
        /// Other devices can be stopped, for example: WebCamera.
        /// </summary>
        private void StopDevices()
        {
            if (microphone != null)
            {
                microphone.Stop();
            }

            if (speaker != null)
            {
                speaker.Stop();
            }
        }


        #region Media handling guide
        /*
         To send our voice through the microphone to the other client's speaker, we need to connect them.
         We send our voice through the mediaSender, and we get the other client's voice through the mediaSender to our speaker object. 
          
         To disconnect these handlers, we will use the DisconnectMedia() method.
		           
         It is possible to use other mediahandlers with the connector, for example we can connect a WaveStreamPlayback or an MP3StreamPlayback object to the MediaSender, so we can play music/voice
         during the call. For exmaple: when can create an IVR (Interactive Voice Response), we can create voice recorder etc.
         
         For example:
         We can connect an .mp3 file player (which plays an mp3 file into the voice call) by the "connector.Connect(Mp3StreamPlayback, mediaSender);  " line.
         (We should also create an MP3StreamPlayback object: "MP3StreamPlayback Mp3StreamPlayback; "
         and we need to tell to this object the details (what to play into the speaker, etc.))
         */
        #endregion

        /// <summary>
        /// Connects the audio handling devices to each other.
        /// The audio data will flow from the source to the destination.
        /// </summary>
        private void ConnectMedia()
        {
            if (microphone != null)
            {
                connector.Connect(microphone, mediaSender);
            }

            if (speaker != null)
            {
                connector.Connect(mediaReceiver, speaker);
            }
        }


        /// <summary>
        /// Disconnects the audio handling devices from each other.
        /// </summary>
        private void DisconnectMedia()
        {
            if (microphone != null)
            {
                connector.Disconnect(microphone, mediaSender);
            }

            if (speaker != null)
            {
                connector.Disconnect(mediaReceiver, speaker);
            }

            // You can close all of the connections by using: connector.Dispose();
        }


        /// <summary>
        /// Subscribes to the events of a call to receive notifications such as the state of the call has changed.
        /// In this sample subscribes only to the state changed and error occurred events.
        /// </summary>
        private void WireUpCallEvents()
        {
            call.CallStateChanged += (call_CallStateChanged);
        }


        /// <summary>
        /// Unsubscribes from the events of a call.
        /// </summary>
        private void WireDownCallEvents()
        {
            call.CallStateChanged -= (call_CallStateChanged);
        }


        /// <summary>
        /// This will be called when the state of the call call has changed.
        /// </summary>
        /// <remarks>
        /// In this sample only three states will be handled: Answered, InCall, Ended
        /// 
        /// Answered: when the call has been answered, the audio devices will be started and attached to the call.
        /// It is required to comminicate with the other party and hear them.
        /// The devices are connected at softphone initialization time,
        /// so no need to connect them every time when a call is being answered.
        /// 
        /// InCall: when the call is in an active state, the audio deveices will be started.
        /// 
        /// Ended: when the call ends, the audio devices will be stopped and detached from the call.
        /// </remarks>
        private void call_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            // the call has been answered
            if (e.State == CallState.Answered)
            {
                StartDevices();

                mediaReceiver.AttachToCall(call);
                mediaSender.AttachToCall(call);
            }

            // the call is in active communication state
            // IMPORTANT: this state can occur multiple times. for example when answering the call or the call has been taken off hold.
            if (e.State == CallState.InCall)
            {
                StartDevices();
            }

            // the call has ended
            if (e.State.IsCallEnded())
            {
                if (call != null)
                {
                    CallFinished();
                }
            }

            DispatchAsync(() =>
            {
                var handler = CallStateChanged;
                if (handler != null)
                    handler(this, e);
            });
        }


        /// <summary>
        /// Starts calling the specified number.
        /// In this sample an outgoing call can be made if there is no current call (outgoing or incoming) on the phone line.
        /// </summary>
        public void StartCall(string numberToDial)
        {
            if (call == null)
            {
                call = softphone.CreateCallObject(phoneLine, numberToDial);
                WireUpCallEvents();

                // To make a call simply call the Start() method of the call object.
                call.Start();
            }
        }


        /// <summary>
        /// Hangs up the current call.
        /// </summary>
        public void HangUp()
        {
            if (call != null)
            {
                call.HangUp();
                call = null;
            }
        }


        /// <summary>
        /// If the call ends, we won't need our speaker and microphone anymore to communicate,
        /// until we enter into a call again, so we are calling the StopDevices() method.
        /// The mediaHandlers are getting detached from the call object
        /// (since we are not using our microphone and speaker, we have no media to send).
        /// We won't need the call's events anymore, becouse our call is about to be ended,
        /// and with setting the call to null, we are ending it.
        /// </summary>
        public void CallFinished()
        {
            StopDevices();

            mediaReceiver.Detach();
            mediaSender.Detach();

            WireDownCallEvents();

            call = null;
        }


        /// <summary>
        /// This method is used to solve the task blockings.
        /// </summary>
        private void DispatchAsync(Action action)
        {
            var task = new WaitCallback(o => action.Invoke());
            ThreadPool.QueueUserWorkItem(task);
        }


        /// <summary>
        /// This method returns the available codecs.
        /// </summary>
        public IEnumerable<CodecInfo> Codecs()
        {
            return softphone.Codecs;
        }


        /// <summary>
        /// This method enables the codecs received as parameters.
        /// </summary>
        public void EnableCodec(int codec)
        {
            softphone.EnableCodec(codec);
        }


        /// <summary>
        /// This method disables the codecs received as parameters.
        /// </summary>
        public void DisableCodec(int codec)
        {
            softphone.DisableCodec(codec);
        }
    }
}
