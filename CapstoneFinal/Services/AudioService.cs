using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using NAudio.Wave;
using NAudio.Extras;

namespace SpaceInvaders.Services
{
    public class AudioService
    {
        private MediaPlayer? _themePlayer;
        private WaveOutEvent? _ufoSoundPlayer;
        private WaveStream? _ufoWaveReader;
        private LoopStream? _ufoLoopStream;
        
        public void StartThemeMusic()
        {
            try
            {
                _themePlayer ??= new MediaPlayer { IsLoopingEnabled = true };
                _themePlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/theme.ram"));
                _themePlayer.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing theme: {ex.Message}");
            }
        }
        
        public void StopThemeMusic()
        {
            _themePlayer?.Pause();
        }
        
        public async Task StartUfoSoundAsync()
        {
            StopUfoSound();
            try
            {
                _ufoSoundPlayer = new WaveOutEvent();
                var fileUri = new Uri("ms-appx:///Assets/sounds/ufo_highpitch.wav");
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
                var stream = await storageFile.OpenAsync(FileAccessMode.Read);
                
                _ufoWaveReader = new WaveFileReader(stream.AsStreamForRead());
                _ufoLoopStream = new LoopStream(_ufoWaveReader);
                _ufoSoundPlayer.Init(_ufoLoopStream);
                _ufoSoundPlayer.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting UFO sound: {ex.Message}");
            }
        }
        
        public void StopUfoSound()
        {
            _ufoSoundPlayer?.Stop();
            _ufoSoundPlayer?.Dispose();
            _ufoSoundPlayer = null;
            
            _ufoLoopStream?.Dispose();
            _ufoLoopStream = null;
            
            _ufoWaveReader?.Dispose();
            _ufoWaveReader = null;
        }
        
        public async Task PlaySoundAsync(string fileName)
        {
            try
            {
                var fileUri = new Uri($"ms-appx:///Assets/sounds/{fileName}");
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
                var stream = await storageFile.OpenAsync(FileAccessMode.Read);
                
                var waveReader = new WaveFileReader(stream.AsStreamForRead());
                var waveOut = new WaveOutEvent();
                
                waveOut.PlaybackStopped += (s, a) =>
                {
                    waveReader.Dispose();
                    stream.Dispose();
                    waveOut.Dispose();
                };
                
                waveOut.Init(waveReader);
                waveOut.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing sound '{fileName}': {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            StopThemeMusic();
            StopUfoSound();
            _themePlayer?.Dispose();
        }
    }
}
