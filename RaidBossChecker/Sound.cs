using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace RaidBossChecker
{
    public static class Sound
    {
        /// <summary>
        /// To add new music just ADD new music file to folder 'Sounds'
        /// Make sure that file Build action is 'Embedded resource'
        /// </summary>
     
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow; // object main window to have access to controls from main window
        public static string soundFullName; // name of sound that is selected and is using
        private static string soundFormat = ".mp3";  // do not need to change otherwise want to use another format (.wav)
        public static List<string> soundNames = new List<string>();
        
        public static MediaPlayer mediaPlayer = new MediaPlayer(); //Create a new MediaPlayer object

        // This is only for getting short names for ComboBox
        public static void LoadSoundList()
        {
            // Take all filenames from folder 'Sounds' to list in format 'name.mp3'
            foreach (string name in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.Contains(".Sounds.")).ToList())
            {
                // Getting every filename from folder 'Sounds' and changing name from 'name.mp3' just to 'name'
                string newName = name.Replace(Assembly.GetExecutingAssembly().GetName().Name + ".Sounds.".ToString(), string.Empty).Replace(soundFormat, string.Empty);
                // Add filename to List as a string
                soundNames.Add(newName);
            }
        }

        public static void SaveSoundToDisk() // method with bug
        {
            // This takes a text from ComboBox Sound Name to parameter 'soundFullName' 
            // and then it's used by another methods also.
            // This is in use only in buttons 'Test' and 'Start'
            soundFullName = mainWindow.cboxSound.Text;

            if (File.Exists(Path.Combine(Path.GetTempPath(), String.Concat(soundFullName, soundFormat))) != true) // if file is already exist it temp folder then skip fileStream
            {
                    // This sets up a new temporary file in the %temp% location called "sound.wav"
                    using (FileStream fileStream = File.Create(Path.GetTempPath() + String.Concat(soundFullName, soundFormat)))
                    {
                        // This them looks into the assembly and finds the embedded resource
                        // inside the RaidBossChecher project, under the sounds folder called sound.wav
                        // PLEASE NOTE: this coulde be different
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("RaidBossChecker.Sounds." + String.Concat(soundFullName, soundFormat)).CopyTo(fileStream);
                    }
            }
        }

        public static void StartPlayTest()
        {
            // Open the temp WAV file saved in the temp location and called "sound.wav"
            mediaPlayer.Open(new Uri(Path.Combine(Path.GetTempPath(), String.Concat(soundFullName, soundFormat))));
            // Get current volume value from volume slider
            mediaPlayer.Volume = Convert.ToDouble(mainWindow.sliderVolume.Value);
            // Start the music playing
            mediaPlayer.Play();
        }

        public static void StartPlaySoundLoop()
        {
            // Open the temp WAV file saved in the temp location and called "sound.wav"
            mediaPlayer.Open(new Uri(Path.Combine(Path.GetTempPath(), String.Concat(soundFullName, soundFormat))));
            // Add an event handler for when the media has ended, this way
            // the music can be played on a loop
            mediaPlayer.MediaEnded += new EventHandler(Sound_Ended);
            // Get current volume value from volume slider
            mediaPlayer.Volume = Convert.ToDouble(mainWindow.sliderVolume.Value);
            // Start the music playing
            mediaPlayer.Play();
        }

        public static void Sound_Ended(object sender, EventArgs e)
        {
            // Set the music back to the beginning
            mediaPlayer.Position = TimeSpan.FromMilliseconds(1);
            // Play the music
            mediaPlayer.Play();
        }
        public static void StopSound()
        {
            // Delete loop EventHandler from Media Player object
            mediaPlayer.MediaEnded -= Sound_Ended;
            // Stops the music 
            mediaPlayer.Stop();
            // Disposes of the MediaPlayer object
            mediaPlayer.Close();
        }
        public static void DeleteSoundFromDisk()
        {
            File.Delete(Path.Combine(Path.GetTempPath(), String.Concat(soundFullName, soundFormat)));
        }

        public static void DeleteSounds()
        {

            foreach (string name in soundNames)
            {
                File.Delete(Path.Combine(Path.GetTempPath(), String.Concat(name, soundFormat)));

            }

        }
    }
}
