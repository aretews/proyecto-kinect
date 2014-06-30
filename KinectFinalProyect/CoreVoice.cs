using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Speech.Recognition;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;

namespace KinectFinalProyect
{
    class CoreVoice
    {
        private string keyWord;
        private RecognizerInfo recognizer;
        private SpeechRecognitionEngine speechRecognitionEngine;
        private KinectSensor sensor;
        private Choices choices;

        public void DefineKeyWord(string word) {
            keyWord = word;
        }
        public void setSensor(KinectSensor sensor) {

            this.sensor = sensor;
        }

        public string getKeyWord(){
            return keyWord;
        }

        private Thread getThreadAudioRecognize() { 
            Thread newAudio = new Thread(StartAudioStream);

            return newAudio;
        }

        public void DefineChoices() {
            choices = new Choices();
            choices.Add(getKeyWord());
        }

        public Choices getChoices() {
            return choices;
        }

        private void StartAudioStream(){
            //sample 16 bits
            // canal 1 de audio
            //bits por segundo 32000

            speechRecognitionEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public RecognizerInfo getRecognizer() {
            return recognizer;
        }
        public  void InitializeRecognizer()
        {
            /*===================================================================*\
             * int timeMiliseconds --> tiempo que estara disponible el microfono
             *                         para interpretar lo que se le diga.
             * las condiciones de paro son:
             *   1) el usuario pasando ciertos segundos no ha dicho nada
             *   2) no reconocio lo que dijo
             *   3) concluyo la accion
             * 
            \*====================================================================*/


            foreach (var recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) &&
                    "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    this.recognizer = recognizer;
                }
            }
          

        }


    }
}
