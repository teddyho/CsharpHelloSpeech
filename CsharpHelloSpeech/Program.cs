using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;

namespace CsharpHelloSpeech
{
    class Program
    {
        public static async Task TranslationWithMicrophoneAsync()
        {
            // Creates an instance of a speech factory with specified
            // subscription key and service region. Replace with your own subscription key
            // and service region (e.g., "westus").
            var factory = SpeechFactory.FromSubscription ( "3ab1fa82f9744473a04a428051889166", "eastasia" );

            // Sets source and target languages
            string fromLanguage = "zh-Hans";
            List<string> toLanguages = new List<string> () { "en-US" };

            // Sets voice name of synthesis output.
            const string GermanVoice = "de-DE-Hedda";

            // Creates a translation recognizer using microphone as audio input, and requires voice output.
            using (var recognizer = factory.CreateTranslationRecognizer ( fromLanguage, toLanguages, GermanVoice ))
            {
                // Subscribes to events.
                //recognizer.IntermediateResultReceived += (s, e) => {
                //    Console.WriteLine ( $"\nPartial result: recognized in {fromLanguage}: {e.Result.Text}." );
                //    if (e.Result.TranslationStatus == TranslationStatus.Success)
                //    {
                //        foreach (var element in e.Result.Translations)
                //        {
                //            Console.WriteLine ( $"    Translated into {element.Key}: {element.Value}" );
                //        }
                //    }
                //    else
                //    {
                //        Console.WriteLine ( $"    Translation failed. Status: {e.Result.TranslationStatus.ToString ()}, FailureReason: {e.Result.FailureReason}" );
                //    }
                //};

                recognizer.FinalResultReceived += (s, e) => {
                    if (e.Result.RecognitionStatus != RecognitionStatus.Recognized)
                    {
                        Console.WriteLine ( $"\nFinal result: Status: {e.Result.RecognitionStatus.ToString ()}, FailureReason: {e.Result.RecognitionFailureReason}." );
                        return;
                    }
                    else
                    {
                        Console.WriteLine ( $"\nFinal result: Status: {e.Result.RecognitionStatus.ToString ()}, recognized text in {fromLanguage}: {e.Result.Text}." );
                        if (e.Result.TranslationStatus == TranslationStatus.Success)
                        {
                            foreach (var element in e.Result.Translations)
                            {
                                Console.WriteLine ( $"    Translated into {element.Key}: {element.Value}" );
                            }
                        }
                        else
                        {
                            Console.WriteLine ( $"    Translation failed. Status: {e.Result.TranslationStatus.ToString ()}, FailureReason: {e.Result.FailureReason}" );
                        }
                    }
                };

                recognizer.SynthesisResultReceived += (s, e) =>
                {
                    if (e.Result.Status == SynthesisStatus.Success)
                    {
                        Console.WriteLine ( $"Synthesis result received. Size of audio data: {e.Result.Audio.Length}" );
                        using (var m = new MemoryStream ( e.Result.Audio ))
                        {
                            SoundPlayer simpleSound = new SoundPlayer ( m );
                            simpleSound.PlaySync ();
                        }
                    }
                    else if (e.Result.Status == SynthesisStatus.SynthesisEnd)
                    {
                        Console.WriteLine ( $"Synthesis result: end of synthesis result." );
                    }
                    else
                    {
                        Console.WriteLine ( $"Synthesis error. Status: {e.Result.Status.ToString ()}, Failure reason: {e.Result.FailureReason}" );
                    }
                };

                recognizer.RecognitionErrorRaised += (s, e) => {
                    Console.WriteLine ( $"\nAn error occurred. Status: {e.Status.ToString ()}" );
                };

                recognizer.OnSessionEvent += (s, e) => {
                    Console.WriteLine ( $"\nSession event. Event: {e.EventType.ToString ()}." );
                };

                // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                Console.WriteLine ( "Say something..." );
                await recognizer.StartContinuousRecognitionAsync ().ConfigureAwait ( false );

                do
                {
                    Console.WriteLine ( "Press Enter to stop" );
                } while (Console.ReadKey ().Key != ConsoleKey.Enter);

                await recognizer.StopContinuousRecognitionAsync ().ConfigureAwait ( false );
            }
        }

        static void Main(string[] args)
        {
            TranslationWithMicrophoneAsync ().Wait ();
        }
    }
}