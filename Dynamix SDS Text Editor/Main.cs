using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Dynamix_SDS_Text_Editor
{
    public partial class Main : Form
    {
        Manager.Brain Pinky = null;

        public Main()
        {
            InitializeComponent();

            UnloadSDSInfo();
            UnloadSDSMessages();
        }

        #region Scripted Events
        private void UnloadSDSInfo()
        {
            MarkCaption.Text = "---";
            VersionCaption.Text = "---";
            IndexCaption.Text = "---";

            SaveFileButton.Enabled = false;

            SaveFileTButton.Enabled = false;
        }
        private void UnloadSDSMessages()
        {
            MessageList.Items.Clear();
            MessageList.Enabled = false;

            FirstButton.Enabled = false;
            PrevButton.Enabled = false;
            NextButton.Enabled = false;
            LastButton.Enabled = false;

            FirstTButton.Enabled = false;
            PrevTButton.Enabled = false;
            NextTButton.Enabled = false;
            LastTButton.Enabled = false;

            BubbleWidth.Enabled = false;
            BubbleWidth.Text = "0";
            BubbleHeight.Enabled = false;
            BubbleHeight.Text = "0";

            MessageContentView.Text = string.Empty;
        }

        private void ActiveGUI()
        {
            MessageList.Enabled = true;

            FirstButton.Enabled = true;
            PrevButton.Enabled = true;
            NextButton.Enabled = true;
            LastButton.Enabled = true;

            FirstTButton.Enabled = true;
            PrevTButton.Enabled = true;
            NextTButton.Enabled = true;
            LastTButton.Enabled = true;

            BubbleWidth.Enabled = true;
            BubbleHeight.Enabled = true;
        }

        private void LoadSDSInfo(FileFormat.Chunks.SDS ChunkSDS)
        {
            MarkCaption.Text = Encoding.Default.GetString(ChunkSDS.Mark);
            VersionCaption.Text = new string(ChunkSDS.Version);
            IndexCaption.Text = Convert.ToString(ChunkSDS.Index);

            SaveFileButton.Enabled = true;

            SaveFileTButton.Enabled = true;
        }
        private void LoadSDSMessages(FileFormat.Chunks.SDS ChunkSDS)
        {
            if (ChunkSDS.Messages.Count > 0)
            {
                MessageList.Items.Clear();
                ActiveGUI();

                foreach (Resource.SDS.Message Message in ChunkSDS.Messages)
                {
                    if (Message.ContentClean.Length > 25)
                    {
                        MessageList.Items.Add(Message.ContentClean.Substring(0, 25) + " [...]");
                    }
                    else
                    {
                        MessageList.Items.Add(Message.ContentClean);
                    }
                }

                MessageList.SelectedIndex = 0;
            }
            else
            {
                UnloadSDSMessages();
            }
        }

        private void ShowMessage(Resource.SDS.Message Message)
        {
            MessageContentView.Text = Message.ContentClean;
            MessageContent.Text = string.Empty;
        }
        private void ShowSize(FileFormat.Chunks.SDS ChunkSDS)
        {
            int index = MessageList.SelectedIndex;

            if (index < 0) { return; }

            BubbleWidth.Text = Convert.ToString(ChunkSDS.Sizes[index][0]);
            BubbleHeight.Text = Convert.ToString(ChunkSDS.Sizes[index][1]);
        }

        private void OpenSDSFile(string fileName)
        {
            try
            {
                Pinky = new Manager.Brain(Lib.Unpack.ChunkSDS(fileName));

                LoadSDSInfo(Pinky.SDSOpened);
                LoadSDSMessages(Pinky.SDSOpened);
            }
            catch (Exception errOpen)
            {
                MessageBox.Show(
                    "Se ha producido el siguiente error al abrir el archivo:" + Environment.NewLine + Environment.NewLine + errOpen.Message,
                    "Error abriendo el archivo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                UnloadSDSInfo();
                UnloadSDSMessages();
            }
        }
        private void SaveSDSFile(string fileName)
        {
            try
            {
                File.WriteAllBytes(fileName, Pinky.SDSOpened.ToByte());
            }
            catch (Exception errSave)
            {
                MessageBox.Show(
                    "Se ha producido el siguiente error al guardar el archivo:" + Environment.NewLine + Environment.NewLine + errSave.Message,
                    "Error guardando el archivo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool IsNumber(KeyPressEventArgs e, int value, int maxLength)
        {
            bool isNumber = false;

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                isNumber = true;
            }

            return isNumber;
        }
        #endregion
        #region Software Events
        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog.Filter = "Archivos SDS|*.sds|Todos los archivos|*.*";
            OpenFileDialog.FileName = string.Empty;

            if (OpenFileDialog.ShowDialog() == DialogResult.Cancel) { return; }

            OpenSDSFile(OpenFileDialog.FileName);
        }
        private void SaveFileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog.Filter = "Archivos SDS|*.sds|Todos los archivos|*.*";
            SaveFileDialog.FileName = "S" + IndexCaption.Text;

            if (SaveFileDialog.ShowDialog() == DialogResult.Cancel) { return; }

            SaveSDSFile(SaveFileDialog.FileName);
        }

        private void FirstButton_Click(object sender, EventArgs e)
        {
            MessageList.SelectedIndex = 0;
        }
        private void LastButton_Click(object sender, EventArgs e)
        {
            MessageList.SelectedIndex = MessageList.Items.Count - 1;
        }
        private void PrevButton_Click(object sender, EventArgs e)
        {
            int index = MessageList.SelectedIndex - 1;

            if (index <= 0) { index = 0; }

            MessageList.SelectedIndex = index;
        }
        private void NextButton_Click(object sender, EventArgs e)
        {
            int index = MessageList.SelectedIndex + 1;

            if (index > MessageList.Items.Count - 1) { index = MessageList.Items.Count - 1; }

            MessageList.SelectedIndex = index;
        }

        private void ModifyButton_Click(object sender, EventArgs e)
        {
            int index = MessageList.SelectedIndex;

            int posWidth = Pinky.SDSOpened.SegmentsCode[index].Content.Length - 14;
            int posHeight = Pinky.SDSOpened.SegmentsCode[index].Content.Length - 12;

            ushort[] size = new ushort[2] {
                Convert.ToUInt16(BubbleWidth.Text),
                Convert.ToUInt16(BubbleHeight.Text) };

            Pinky.SDSOpened.ReplaceSize(index, size);
            Pinky.SDSOpened.Messages[index].SetContentClean(MessageContent.Text);

            LoadSDSMessages(Pinky.SDSOpened);
            MessageList.SelectedIndex = index;
        }
        private void FillTextButton_Click(object sender, EventArgs e)
        {
            MessageContent.Text = MessageContentView.Text;
        }

        private void MessageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = MessageList.SelectedIndex;

            if (index < 0) { return; }

            ShowMessage(Pinky.SDSOpened.Messages[index]);
            ShowSize(Pinky.SDSOpened);
        }
        private void MessageContentView_TextChanged(object sender, EventArgs e)
        {
            if (MessageContentView.Text != string.Empty)
            {
                FillTextButton.Enabled = true;
                FillTextTButton.Enabled = true;
            }
            else
            {
                FillTextButton.Enabled = false;
                FillTextTButton.Enabled = false;
            }
        }
        private void MessageContent_TextChanged(object sender, EventArgs e)
        {
            if (MessageContent.Text != string.Empty)
            {
                ModifyButton.Enabled = true;
                ModifyTButton.Enabled = true;
            }
            else
            {
                ModifyButton.Enabled = false;
                ModifyTButton.Enabled = false;
            }
        }

        private void BubbleWidth_TextChanged(object sender, EventArgs e)
        {
            const int MAX_WIDTH = 320;

            if (Convert.ToInt32(BubbleWidth.Text) > MAX_WIDTH)
            {
                BubbleWidth.Text = BubbleWidth.Text.Substring(0, BubbleWidth.Text.Length - 1);

                MessageBox.Show(
                    "El ancho máximo permitido es de " + Convert.ToString(MAX_WIDTH),
                    "Ancho máximo superado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        private void BubbleHeight_TextChanged(object sender, EventArgs e)
        {
            const int MAX_HEIGHT = 200;

            if (Convert.ToInt32(BubbleHeight.Text) > MAX_HEIGHT)
            {
                BubbleHeight.Text = BubbleHeight.Text.Substring(0, BubbleHeight.Text.Length - 1);

                MessageBox.Show(
                    "El alto máximo permitido es de " + Convert.ToString(MAX_HEIGHT),
                    "Alto máximo superado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void BubbleWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = IsNumber(e, Convert.ToInt32(BubbleWidth.Text), 320);
        }
        private void BubbleHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = IsNumber(e, Convert.ToInt32(BubbleHeight.Text), 200);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("¿Quieres salir de la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        #endregion
        #region Toolbar buttons
        private void OpenFileTButton_Click(object sender, EventArgs e)
        {
            OpenFileButton.PerformClick();
        }
        private void SaveFileTButton_Click(object sender, EventArgs e)
        {
            SaveFileButton.PerformClick();
        }

        private void FirstTButton_Click(object sender, EventArgs e)
        {
            FirstButton.PerformClick();
        }
        private void PrevTButton_Click(object sender, EventArgs e)
        {
            PrevButton.PerformClick();
        }
        private void NextTButton_Click(object sender, EventArgs e)
        {
            NextButton.PerformClick();
        }
        private void LastTButton_Click(object sender, EventArgs e)
        {
            LastButton.PerformClick();
        }

        private void ModifyTButton_Click(object sender, EventArgs e)
        {
            ModifyButton.PerformClick();
        }
        private void FillTextTButton_Click(object sender, EventArgs e)
        {
            FillTextButton.PerformClick();
        }

        private void AboutTButton_Click(object sender, EventArgs e)
        {
            Manager.AssemblyInfo SoftInfo = new Manager.AssemblyInfo();

            MessageBox.Show(
                SoftInfo.Product + " " +
                "v" + SoftInfo.FileVersion + Environment.NewLine +
                SoftInfo.Description + Environment.NewLine + Environment.NewLine +
                SoftInfo.Company + Environment.NewLine +
                SoftInfo.Copyright + Environment.NewLine,
                SoftInfo.Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        #endregion
    }
}
