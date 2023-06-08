using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Text;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveInEvent;
        private MemoryStream recordedStream;
        private Button StartRecordingButton;
        private Button StopRecordingButton;
        private TextBox TranscriptionTextBox;

        public Form1()
        {
            InitializeComponent();
            InitializeMicrophone();
        }

        private void InitializeComponent()
        {
            this.StartRecordingButton = new System.Windows.Forms.Button();
            this.StopRecordingButton = new System.Windows.Forms.Button();
            this.TranscriptionTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // StartRecordingButton
            // 
            this.StartRecordingButton.Location = new System.Drawing.Point(12, 12);
            this.StartRecordingButton.Name = "StartRecordingButton";
            this.StartRecordingButton.Size = new System.Drawing.Size(100, 23);
            this.StartRecordingButton.TabIndex = 0;
            this.StartRecordingButton.Text = "Start Recording";
            this.StartRecordingButton.UseVisualStyleBackColor = true;
            this.StartRecordingButton.Click += new System.EventHandler(this.StartRecordingButton_Click);
            // 
            // StopRecordingButton
            // 
            this.StopRecordingButton.Enabled = false;
            this.StopRecordingButton.Location = new System.Drawing.Point(118, 12);
            this.StopRecordingButton.Name = "StopRecordingButton";
            this.StopRecordingButton.Size = new System.Drawing.Size(100, 23);
            this.StopRecordingButton.TabIndex = 1;
            this.StopRecordingButton.Text = "Stop Recording";
            this.StopRecordingButton.UseVisualStyleBackColor = true;
            this.StopRecordingButton.Click += new System.EventHandler(this.StopRecordingButton_Click);
            // 
            // TranscriptionTextBox
            // 
            this.TranscriptionTextBox.Location = new System.Drawing.Point(12, 41);
            this.TranscriptionTextBox.Multiline = true;
            this.TranscriptionTextBox.Name = "TranscriptionTextBox";
            this.TranscriptionTextBox.ReadOnly = true;
            this.TranscriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TranscriptionTextBox.Size = new System.Drawing.Size(350, 200);
            this.TranscriptionTextBox.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 253);
            this.Controls.Add(this.TranscriptionTextBox);
            this.Controls.Add(this.StopRecordingButton);
            this.Controls.Add(this.StartRecordingButton);
            this.Name = "Form1";
            this.Text = "Speech to Text Client";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeMicrophone()
        {
            waveInEvent = new WaveInEvent();
            waveInEvent.DataAvailable += WaveInEvent_DataAvailable;
            waveInEvent.WaveFormat = new WaveFormat(16000, 1); // Adjust sample rate and channel count as needed
        }

        private void WaveInEvent_DataAvailable(object sender, WaveInEventArgs e)
        {
            recordedStream.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void StartRecordingButton_Click(object sender, EventArgs e)
        {
            recordedStream = new MemoryStream();
            waveInEvent.StartRecording();
            StartRecordingButton.Enabled = false;
            StopRecordingButton.Enabled = true;
        }

        private async void StopRecordingButton_Click(object sender, EventArgs e)
        {
            waveInEvent.StopRecording();
            StartRecordingButton.Enabled = true;
            StopRecordingButton.Enabled = false;

            // Send the recorded audio to the backend for transcription
            string transcription = await TranscribeAudio(recordedStream);
            TranscriptionTextBox.Text = transcription;

            recordedStream.Dispose();
        }
        private async Task<string> TranscribeAudio(MemoryStream audioStream)
        {
            try
            {
                // Convert the recorded audio to PCM WAV format
                audioStream.Seek(0, SeekOrigin.Begin);

                // Create a new MemoryStream to hold the PCM WAV data
                using (var convertedStream = new MemoryStream())
                {
                    // Write the WAV header manually
                    var writer = new BinaryWriter(convertedStream);
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write((uint)(audioStream.Length + 36));
                    writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                    writer.Write((uint)16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)1);
                    writer.Write((uint)16000);
                    writer.Write((uint)(16000 * 2));
                    writer.Write((ushort)2);
                    writer.Write((ushort)16);
                    writer.Write(Encoding.ASCII.GetBytes("data"));
                    writer.Write((uint)audioStream.Length);
                    writer.Write(audioStream.ToArray());

                    convertedStream.Seek(0, SeekOrigin.Begin);

                    // Send the converted PCM WAV audio to the backend for transcription
                    using (HttpClient client = new HttpClient())
                    {
                        using (MultipartFormDataContent formData = new MultipartFormDataContent())
                        {
                            formData.Add(new StreamContent(convertedStream), "audio", "audio.wav");
                            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/upload", formData);
                            if (response.IsSuccessStatusCode)
                            {
                                string transcription = await response.Content.ReadAsStringAsync();
                                return transcription;
                            }
                            else
                            {
                                return "Error: " + response.StatusCode;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

    }

}
