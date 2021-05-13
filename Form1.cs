using NAudio.Wave;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertToPhoneSoundFile
{
    public partial class Form1 : Form
    {  private string _currentFile;

        public Form1()
        {
            Program.StartupInit();
            Log.Information($"Started Sound Manager....");
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = Properties.Settings.Default.ConvertOptions;
            this.textBox2.Text = Properties.Settings.Default.SaveInDirectory;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.CloseAndFlush();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Run();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            this.PlaySoundFile();
        }

        private void Run()
        {
            this.btnPlay.Visible = false;
            Log.Information($" Started Conversion Run");

            var fileDialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Multiselect = false,
                Title = "בחר קובץ שמע...",
                Filter = "קובצי שמע|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.ogg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV;*.OGG"
            };
            if (fileDialog.ShowDialog(this) != DialogResult.OK ||
                !File.Exists(fileDialog.FileName)) return;

            this.label1.Text = $"ממיר קובץ {Path.GetFileNameWithoutExtension(fileDialog.FileName)}...";
            this.label1.Refresh();
            _currentFile = Program.GetSoundFilePath(fileDialog.FileName);
            if (Program.DoSoundFileConversion(fileDialog.FileName) &&
                File.Exists(_currentFile))
            {
                this.label1.Text = "הקובץ נקלט בהצלחה";
                this.btnPlay.Visible = true;
            }
            else
            {
                this.label1.Text = "העלת הקובץ נכשלה";
                this.btnPlay.Visible = false;
            }
        }

        private void PlaySoundFile()
        {
            try
            {
                Task.Run(async () =>
                {
                    await using var audioFile = new AudioFileReader(_currentFile);
                    using var outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(1000);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error running {_currentFile}.");
                MessageBox.Show($"Error running {_currentFile}. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ConvertOptions = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveInDirectory = textBox2.Text;
        }
    }
}
