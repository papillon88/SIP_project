using System;
using Ozeki.Media;
using Ozeki.Media.MediaHandlers;

namespace Noise_Reduction
{
    class Program
    {
        static Microphone microphone;
        static Speaker speaker;
        static MediaConnector connector;
        static AudioQualityEnhancer audioProcessor;

        static void Main(string[] args)
        {
            microphone = Microphone.GetDefaultDevice();
            speaker = Speaker.GetDefaultDevice();
            connector = new MediaConnector();
            audioProcessor = new AudioQualityEnhancer();

            audioProcessor.NoiseReductionLevel = NoiseReductionLevel.Medium;
            audioProcessor.SetEchoSource(speaker);

            connector.Connect(microphone, audioProcessor);
            connector.Connect(audioProcessor, speaker);

            microphone.Start();
            speaker.Start();

            Console.ReadLine();
        }
    }
}
