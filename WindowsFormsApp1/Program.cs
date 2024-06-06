using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem aboutMenuItem;
        private Button selectFolderButton;
        private TextBox folderPathTextBox;
        private ListBox foldersListBox;
        private DataGridView filesDataGridView;
        private Button processFilesButton;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
           
            this.Text = "Окошко";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width * 75 / 100, Screen.PrimaryScreen.WorkingArea.Height * 75 / 100);

            menuStrip = new MenuStrip();
            aboutMenuItem = new ToolStripMenuItem("About");
            aboutMenuItem.Click += AboutMenuItem_Click;
            menuStrip.Items.Add(aboutMenuItem);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            selectFolderButton = new Button
            {
                Text = "Select Folder",
                Dock = DockStyle.Top
            };
            selectFolderButton.Click += SelectFolderButton_Click;
            this.Controls.Add(selectFolderButton);

            folderPathTextBox = new TextBox
            {
                ReadOnly = true,
                Dock = DockStyle.Top
            };
            this.Controls.Add(folderPathTextBox);

          
            processFilesButton = new Button
            {
                Text = "Process Files",
                Dock = DockStyle.Bottom,
                Visible = false
            };
            processFilesButton.Click += ProcessFilesButton_Click;
            this.Controls.Add(processFilesButton);

            
            filesDataGridView = new DataGridView
            {
                Top = menuStrip.Height + selectFolderButton.Height + folderPathTextBox.Height,
                Left = 0,
                Width = this.ClientSize.Width - (this.ClientSize.Width * 20 / 100),
                Height = this.ClientSize.Height - menuStrip.Height - selectFolderButton.Height - folderPathTextBox.Height - processFilesButton.Height,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            filesDataGridView.DoubleClick += FilesDataGridView_DoubleClick;

            
            filesDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                DataPropertyName = "Name"
            });
            filesDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Last Modified",
                DataPropertyName = "LastModified"
            });
            filesDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Size (bytes)",
                DataPropertyName = "Size"
            });
            filesDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Random Value",
                DataPropertyName = "RandomValue"
            });

            this.Controls.Add(filesDataGridView);

            
            foldersListBox = new ListBox
            {
                Dock = DockStyle.Right,
                Width = this.Width * 20 / 100
            };
            foldersListBox.DoubleClick += FoldersListBox_DoubleClick;
            this.Controls.Add(foldersListBox);
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Name: Melis\nCompany: JDrivers\ngroup: sest-122", "About");
        }


        private async void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPathTextBox.Text = folderBrowserDialog.SelectedPath;
                    LoadFolderContents(folderBrowserDialog.SelectedPath);
                    processFilesButton.Visible = true;
                }
            }
        }

        private void LoadFolderContents(string path)
        {
            foldersListBox.Items.Clear();
            filesDataGridView.Rows.Clear();

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                foldersListBox.Items.Add(new DirectoryInfo(directory));
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                filesDataGridView.Rows.Add(fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length, 0);
            }
        }

        private void FoldersListBox_DoubleClick(object sender, EventArgs e)
        {
            if (foldersListBox.SelectedItem != null)
            {
                var selectedDirectory = (DirectoryInfo)foldersListBox.SelectedItem;
                var folderInfoForm = new FolderInfoForm(selectedDirectory);
                folderInfoForm.Show();
            }
        }

        private void FilesDataGridView_DoubleClick(object sender, EventArgs e)
        {
            if (filesDataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = filesDataGridView.SelectedRows[0];
                var fileName = selectedRow.Cells[0].Value.ToString();
                var filePath = Path.Combine(folderPathTextBox.Text, fileName);

                var result = MessageBox.Show($"Do you want to duplicate the file '{fileName}'?", "Duplicate File", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    var newFilePath = Path.Combine(folderPathTextBox.Text, Path.GetFileNameWithoutExtension(fileName) + "_copy" + Path.GetExtension(fileName));
                    File.Copy(filePath, newFilePath);
                    LoadFolderContents(folderPathTextBox.Text);
                }
            }
        }

        private async void ProcessFilesButton_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(folderPathTextBox.Text);
            var random = new Random();
            List<Task> tasks = new List<Task>();

            foreach (DataGridViewRow row in filesDataGridView.Rows)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var randomValue = random.Next(1, files.Length + 1);
                    var fileName = row.Cells[0].Value.ToString();
                    var filePath = Path.Combine(folderPathTextBox.Text, fileName);
                    await Task.Delay(randomValue * 1000);
                    Invoke(new Action(() =>
                    {
                        row.Cells[3].Value = randomValue;
                    }));
                }));
            }
            await Task.WhenAll(tasks);
        }

    }

    public class FolderInfoForm : Form
    {
        private Label nameLabel;
        private Label lastModifiedLabel;

        public FolderInfoForm(DirectoryInfo directoryInfo)
        {
            this.Text = "Folder Info";
            this.Size = new System.Drawing.Size(300, 200);

            nameLabel = new Label
            {
                Text = $"Name: {directoryInfo.Name}",
                Dock = DockStyle.Top
            };
            this.Controls.Add(nameLabel);

            lastModifiedLabel = new Label
            {
                Text = $"Last Modified: {directoryInfo.LastWriteTime}",
                Dock = DockStyle.Top
            };
            this.Controls.Add(lastModifiedLabel);
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

}
