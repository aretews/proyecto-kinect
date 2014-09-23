using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace motion
{
    class CSpeech
    {
        //métodos
        public CSpeech()
        {

            synthesizer = new SpeechSynthesizer();

            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                Console.WriteLine("voz: " + voice.VoiceInfo.Name);

            }
            synthesizer.Volume = 100;
            synthesizer.Rate = -2;


        }

        public void generateVoice(String word)
        {
           synthesizer.SpeakAsync(word);
        }//---------------------------------------------------------

        //variables
        private SpeechSynthesizer synthesizer;
    }
}
