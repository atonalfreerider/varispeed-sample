using NAudio.Wave;
using VarispeedDemo.SoundTouch;

namespace VarispeedDemo;

public partial class MainForm : Form
{
    WaveOutEvent? wavePlayer;
    VarispeedSampleProvider? speedControl;
    AudioFileReader? reader;

    public MainForm()
    {
        InitializeComponent();
        timer1.Interval = 500;
        timer1.Start();
        Closing += OnMainFormClosing;

        comboBoxModes.Items.Add("Speed");
        comboBoxModes.Items.Add("Tempo");
        comboBoxModes.SelectedIndex = 0;

        EnableControls(false);
            
    }

    void OnMainFormClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        wavePlayer?.Dispose();
        speedControl?.Dispose();
        reader?.Dispose();
    }

    void OnButtonPlayClick(object? sender, EventArgs e)
    {
        if (wavePlayer == null)
        {
            wavePlayer = new WaveOutEvent();
            wavePlayer.PlaybackStopped += WavePlayerOnPlaybackStopped;
        }
        if (speedControl == null)
        {
            LoadFile();
            if (speedControl == null) return;
        }
            
        wavePlayer.Init(speedControl);
            
        wavePlayer.Play();
        EnableControls(true);
    }

    void WavePlayerOnPlaybackStopped(object? sender, StoppedEventArgs stoppedEventArgs)
    {
        if (stoppedEventArgs.Exception != null)
        {
            MessageBox.Show(stoppedEventArgs.Exception.Message, "Playback Stopped Unexpectedly");
        }
        EnableControls(false);
    }

    void EnableControls(bool isPlaying)
    {
        buttonPlay.Enabled = !isPlaying;
        buttonLoad.Enabled = !isPlaying;
        buttonStop.Enabled = isPlaying;
        comboBoxModes.Enabled = !isPlaying;
    }

    static string SelectFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "MP3 Files|*.mp3";
        return (ofd.ShowDialog() == DialogResult.OK ? ofd.FileName : null) ?? string.Empty;
    }

    void OnButtonStopClick(object? sender, EventArgs e)
    {
        wavePlayer?.Stop();
    }

    void OnTrackBarPlaybackRateScroll(object? sender, EventArgs e)
    {
        speedControl.PlaybackRate = 0.5f + trackBarPlaybackRate.Value*0.1f;
        labelPlaybackSpeed.Text = $"x{speedControl.PlaybackRate:F2}";
    }

    void OnButtonLoadClick(object? sender, EventArgs e)
    {
        LoadFile();
    }

    void LoadFile()
    {
        reader?.Dispose();
        speedControl?.Dispose();
        reader = null;
        speedControl = null;

        string? file = SelectFile();
        if (string.IsNullOrEmpty(file)) return;
        reader = new AudioFileReader(file);
        DisplayPosition();
        trackBarPlaybackPosition.Value = 0;
        trackBarPlaybackPosition.Maximum = (int) (reader.TotalTime.TotalSeconds + 0.5);
        bool useTempo = comboBoxModes.SelectedIndex == 1;
        speedControl = new VarispeedSampleProvider(reader, 100, new SoundTouchProfile(useTempo, false));
    }

    void timer1_Tick(object? sender, EventArgs e)
    {
        if (reader != null)
        {
            trackBarPlaybackPosition.Value = (int) reader.CurrentTime.TotalSeconds;
            DisplayPosition();
        }
    }

    void DisplayPosition()
    {
        labelPosition.Text = reader?.CurrentTime.ToString("mm\\:ss");
    }

    void trackBarPlaybackPosition_Scroll(object? sender, EventArgs e)
    {
        if (reader != null)
        {
            reader.CurrentTime = TimeSpan.FromSeconds(trackBarPlaybackPosition.Value);
            speedControl?.Reposition();
        }
    }

    void comboBoxModes_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (speedControl != null)
        {
            bool useTempo = comboBoxModes.SelectedIndex == 1;
            speedControl.SetSoundTouchProfile(new SoundTouchProfile(useTempo, false));
        }
    }
}