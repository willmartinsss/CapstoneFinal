using NAudio.Wave;
using System;
using System.Diagnostics;
using Windows.Storage;
using System.IO;
using NAudio.Extras;
using System.Threading.Tasks;

namespace CapstoneSpaceInvader.Managers
{
    public class AudioManager
    {
        private WaveOutEvent? _themeSoundPlayer;
        private WaveStream? _themeWaveReader;
        private LoopStream? _themeLoopStream;
        private Stream? _themeFileStream; // Mantém a referência para libertar depois

        private WaveOutEvent? _ufoSoundPlayer;
        private WaveStream? _ufoWaveReader;
        private LoopStream? _ufoLoopStream;
        private Stream? _ufoFileStream; // Mantém a referência para libertar depois

        private async Task<(WaveStream, Stream)> CreateWaveReader(string soundFileName)
        {
            var fileUri = new Uri($"ms-appx:///Assets/Sounds/{soundFileName}");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
            var stream = await storageFile.OpenStreamForReadAsync();
            return (new WaveFileReader(stream), stream);
        }

        public async void StartThemeMusic()
        {
            StopThemeMusic();
            try
            {
                _themeSoundPlayer = new WaveOutEvent();
                (_themeWaveReader, _themeFileStream) = await CreateWaveReader("theme.wav");
                _themeLoopStream = new LoopStream(_themeWaveReader);
                _themeSoundPlayer.Init(_themeLoopStream);
                _themeSoundPlayer.Play();
            }
            catch (Exception ex) { Debug.WriteLine($"Erro ao iniciar música tema: {ex.Message}"); }
        }

        public void StopThemeMusic()
        {
            _themeSoundPlayer?.Stop();
            _themeSoundPlayer?.Dispose();
            _themeSoundPlayer = null;
            _themeLoopStream?.Dispose();
            _themeLoopStream = null;
            _themeWaveReader?.Dispose();
            _themeWaveReader = null;
            _themeFileStream?.Dispose();
            _themeFileStream = null;
        }

        public async void StartUfoSound()
        {
            StopUfoSound();
            try
            {
                _ufoSoundPlayer = new WaveOutEvent();
                (_ufoWaveReader, _ufoFileStream) = await CreateWaveReader("ufo_highpitch.wav");
                _ufoLoopStream = new LoopStream(_ufoWaveReader);
                _ufoSoundPlayer.Init(_ufoLoopStream);
                _ufoSoundPlayer.Play();
            }
            catch (Exception ex) { Debug.WriteLine($"Erro ao iniciar som do OVNI: {ex.Message}"); }
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
            _ufoFileStream?.Dispose();
            _ufoFileStream = null;
        }

        public async void PlaySoundEffect(string soundFileName)
        {
            try
            {
                var (waveReader, stream) = await CreateWaveReader(soundFileName);
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
            catch (Exception ex) { Debug.WriteLine($"Erro ao tocar som '{soundFileName}': {ex.Message}"); }
        }
    }
}
