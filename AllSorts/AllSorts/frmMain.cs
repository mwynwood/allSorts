using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AllSorts
{
    public partial class frmMain : Form
    {
        FlatFileSettings settings = new FlatFileSettings("AllSorts.settings");
        public int currentImage = 0;
        public string currentPath = "";
        public bool recursive = false;
        public FileInfo[] fi;
        public DirectoryInfo di;
        public string[] fileTypes = { "*.jpg", "*.gif", "*.png", "*.jpeg", "*.bmp", "*.wmf" };
        public const char SEPERATOR = '|';
        private Point clickPosn; // Used for the drag-to-scroll thing
        private Point scrollPosn; // Used for the drag-to-scroll thing

        public frmMain()
        {
            InitializeComponent();
            //MessageBox.Show("THIS IS  A DEVELOPMENT VERSION - NOT FOR DISTRIBUTION!", Application.ProductName + " " + Application.ProductVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Text = Application.ProductName + " (Version " + Application.ProductVersion + ")";
            loadSettings();
        }

        // TODO: make a separate class with all the logic in it

        public void selectFolder()  // TODO: selectFolder() does too much stuff - it shouldn't have UI in it!
        {
            try
            {
                currentImage = 0;
                progressBarCurrentImage.Value = 0;
                labelFileCount.Text = "";
                //labelFileName.Text = "";
                toolStripLabelFileName.Text = "";

                folderBrowserDialog1.ShowNewFolderButton = false;
                folderBrowserDialog1.Description = Application.ProductName + Environment.NewLine + "Select a folder to sort:";

                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    currentPath = folderBrowserDialog1.SelectedPath;
                    di = new DirectoryInfo(currentPath);

                    // TODO: make it use more than just one file type
                    if (MessageBox.Show("Look for pictures in sub-folders too?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        recursive = true;
                        fi = di.GetFiles(fileTypes[0], SearchOption.AllDirectories);
                        // This code is for getting more than one file type
                        // but it throws an exception if there are too many files!
                        //fi = di.GetFiles(fileTypes[0], SearchOption.AllDirectories);
                        //for (int i = 1; i < fileTypes.Length; i++)
                        //{
                        //    di.GetFiles(fileTypes[i], SearchOption.AllDirectories).CopyTo(fi, 1);
                        //}
                    }
                    else
                    {
                        recursive = false;
                        fi = di.GetFiles(fileTypes[0], SearchOption.TopDirectoryOnly);
                        // This code is for getting more than one file type
                        // but it throws an exception if there are too many files!
                        //fi = di.GetFiles(fileTypes[0], SearchOption.TopDirectoryOnly);
                        //for (int i = 1; i < fileTypes.Length; i++)
                        //{
                        //    di.GetFiles(fileTypes[i], SearchOption.TopDirectoryOnly).CopyTo(fi, 1);
                        //}
                    }

                    if (fi.Length > 0)
                    {
                        //labelFileCount.Text = (currentImage + 1) + " of " + fi.Length.ToString();
                        //labelFileName.Text = fi[currentImage].FullName;
                        //pictureBox1.ImageLocation = fi[currentImage].FullName;

                        listBoxAllFiles.Items.Clear();
                        progressBarCurrentImage.Maximum = fi.Length; // +1?
                        foreach (FileInfo x in fi)
                        {
                            listBoxAllFiles.Items.Add(x.Name.ToString());
                        }
                        displayImageAtPosition(currentImage);
                    }
                    else
                    {
                        MessageBox.Show("No files found", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        selectFolder();
                    }
                    Cursor.Current = Cursors.Default;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void displayNextImage()
        {
            try
            {
                currentImage = currentImage + 1;
                pictureBox1.ImageLocation = fi[currentImage].FullName;
                //labelFileName.Text = fi[currentImage].FullName;
                toolStripLabelFileName.Text = fi[currentImage].FullName;
                labelFileCount.Text = (currentImage + 1) + " of " + fi.Length.ToString();
                progressBarCurrentImage.Value = currentImage + 1;
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex.ToString());
                currentImage = -1;
                displayNextImage();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                listBoxAllFiles.SelectedIndex = currentImage;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // TODO: Can I just use displayImageAtposotion() instead of this?

        public void displayPreviousImage()
        {
            try
            {
                currentImage = currentImage - 1;
                pictureBox1.ImageLocation = fi[currentImage].FullName;
                //labelFileName.Text = fi[currentImage].FullName;
                toolStripLabelFileName.Text = fi[currentImage].FullName;
                labelFileCount.Text = (currentImage + 1) + " of " + fi.Length.ToString();
                progressBarCurrentImage.Value = currentImage + 1;
            }
            catch(IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex.ToString());
                currentImage = fi.Length;
                displayPreviousImage();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            try
            {
                listBoxAllFiles.SelectedIndex = currentImage;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // TODO: Can I just use displayImageAtposotion(int) instead of this?

        public void displayImageAtPosition(int pos)
        {
            currentImage = pos - 1;
            displayNextImage();
            setZoomLevel();
        }

        public void toggleSlideShow()
        {
            timerSlideShow.Interval = Convert.ToInt16(numericUpDownSlideShow.Value) * 1000;
            
            if (timerSlideShow.Enabled)
            {
                timerSlideShow.Stop();
            }
            else
            {
                timerSlideShow.Start();
            }

            slideShowToolStripMenuItem.Checked = !slideShowToolStripMenuItem.Checked;
            checkBoxSlideShowRandom.Enabled = !checkBoxSlideShowRandom.Enabled;
            
        }

        public void deleteCurrentFile(bool AskIfItsOkBeforeDeletingTheFile)
        {
            try
            {
                // HACK: This is really bad
                bool WeAreRightToGo = false;

                if (AskIfItsOkBeforeDeletingTheFile == true)
                {
                    if (MessageBox.Show("Are you sure you want to delete:" + Environment.NewLine + fi[currentImage].FullName + "?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        WeAreRightToGo = true;
                    }
                }
                else
                {
                    WeAreRightToGo = true;
                }

                if (WeAreRightToGo)
                //if(MessageBox.Show("Are you sure you want to delete:" + Environment.NewLine + fi[currentImage].FullName + "?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    String fileNameToDelete = fi[currentImage].Name;
                    fi[currentImage].Delete();

                    fi = new FileInfo[fi.Length];
                    fi.Initialize();

                    // update the interface
                    if (recursive)
                    {
                        // Lots of calls to GetFiles :-(
                        fi = di.GetFiles(fileTypes[0], SearchOption.AllDirectories);
                        //for (int i = 1; i < fileTypes.Length; i++)
                        //{
                        //    di.GetFiles(fileTypes[i], SearchOption.AllDirectories).CopyTo(fi, 1);
                        //}
                    }
                    else
                    {
                        // Lots of calls to GetFiles :-(
                        fi = di.GetFiles(fileTypes[0], SearchOption.TopDirectoryOnly);
                        //for (int i = 1; i < fileTypes.Length; i++)
                        //{
                        //    di.GetFiles(fileTypes[i], SearchOption.TopDirectoryOnly).CopyTo(fi, 1);
                        //}
                    }
                    listBoxAllFiles.Items.Clear();

                    // HACK: this if is not that good either...but it seems to work
                    if (fi.Length > 0)
                    {
                        foreach (FileInfo x in fi)
                        {
                            listBoxAllFiles.Items.Add(x.Name.ToString());
                        }
                        progressBarCurrentImage.Maximum = fi.Length;
                    }
                    else
                    {
                        selectFolder();
                    }
                    Cursor.Current = Cursors.Default;
                    if (AskIfItsOkBeforeDeletingTheFile)
                    {
                        MessageBox.Show(fileNameToDelete + " has been deleted!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }

                    displayImageAtPosition(currentImage);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } // TODO: deleteCurrentFile() repeats code from selectFolder()

        public void moveCurrentFile() 
        {
            if (checkedListBoxDestinations.CheckedItems.Count > 0)
            {
                bool performDelete = true;
                foreach (Object x in checkedListBoxDestinations.CheckedItems)
                {
                    int theLength = x.ToString().Length;
                    int positionOfFirstBracket = x.ToString().IndexOf('(');
                    String pathWithoutTitle = x.ToString().Substring(positionOfFirstBracket + 1, ((theLength - 1) - positionOfFirstBracket) - 1);

                    //MessageBox.Show("Copying " + fi[currentImage].FullName + " to " + pathWithoutTitle);
                    try
                    {
                        fi[currentImage].CopyTo(pathWithoutTitle + "\\" + fi[currentImage].Name);
                    }
                    catch (Exception ex)
                    {
                        performDelete = false;
                        MessageBox.Show(ex.ToString());
                    }
                }

                if (performDelete)
                {
                    //MessageBox.Show("Deleting " + fi[currentImage]);
                    deleteCurrentFile(false);
                }
                else
                {
                    MessageBox.Show("File not deleted, something went wrong", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                MessageBox.Show("File successfully moved!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select a destination", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void advancedSort(string sortingRules, string baseFolder)
        {
            char[] RULE_SEPERATOR = { '+' };

            //Split up the sorting rules into chunks divided by the RULE_SEPERATOR
            string[] rules = sortingRules.Split(RULE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);

            foreach (string x in rules)
            {
                try
                {
                    //MessageBox.Show("Move the original file:" + Environment.NewLine + fi[currentImage].FullName + Environment.NewLine + "To:" + Environment.NewLine + baseFolder + "\\" + x + "\\" + fi[currentImage].Name, "DEBUG INFO");
                    Directory.CreateDirectory(baseFolder + "\\" + x);
                    fi[currentImage].CopyTo(baseFolder + "\\" + x + "\\" + fi[currentImage].Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR!" + Environment.NewLine + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // If the checkbox is ticked, ask before deleting the file
            // If the checkbox is NOT ticked, just delete it.
            if (checkBoxAskBeforeDeletingoriginalFile.Checked == true)
            {
                //MessageBox.Show("Delete:" + Environment.NewLine + fi[currentImage].FullName, "DEBUG INFO");
                if (MessageBox.Show("Delete the origional file?" + Environment.NewLine + fi[currentImage].FullName, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    deleteCurrentFile(false);
                }
            }
            else
            {
                deleteCurrentFile(false);
            }
        }

        public void addDestination()
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.Description = Application.ProductName + Environment.NewLine + "Select a destination folder:";

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                addDestination(folderBrowserDialog1.SelectedPath);
            }
        } // TODO: addDestination() has UI in it!

        public void addDestination(String destinationToAdd)
        {
            string folderName = destinationToAdd.Substring(destinationToAdd.LastIndexOf('\\') + 1);
            checkedListBoxDestinations.Items.Add(folderName + " (" + destinationToAdd + ")");
        }

        public void removeDestination()
        {
            try
            {
                checkedListBoxDestinations.Items.RemoveAt(checkedListBoxDestinations.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void saveSettings()
        {
            settings.saveSetting("ProductName", Application.ProductName);
            settings.saveSetting("ProductVersion", Application.ProductVersion);

            settings.saveSetting("SlideShowTime", numericUpDownSlideShow.Value.ToString());
            settings.saveSetting("SlideShowRandomMode", checkBoxSlideShowRandom.Checked.ToString());
            //settings.saveSetting("ShowPictureInformation", (!splitContainer1.Panel2Collapsed).ToString());
            settings.saveSetting("ShowToolBar", showToolBarToolStripMenuItem.Checked.ToString());
            settings.saveSetting("ShowSideMenu", (!splitContainer2.Panel1Collapsed).ToString());

            String destinationString = "";
            for (int i = 0; i < checkedListBoxDestinations.Items.Count; i++)
            {
                int theLength = checkedListBoxDestinations.Items[i].ToString().Length;
                int positionOfFirstBracket = checkedListBoxDestinations.Items[i].ToString().IndexOf('(');
                String pathWithoutTitle = checkedListBoxDestinations.Items[i].ToString().Substring(positionOfFirstBracket + 1, ((theLength - 1) - positionOfFirstBracket) - 1);
                destinationString = destinationString + pathWithoutTitle + SEPERATOR;
            }

            settings.saveSetting("Destinations", destinationString);
            settings.saveSetting("AdvancedSortBaseFolder", textBoxAdvancedSortingBaseFolder.Text);
            settings.saveSetting("ClearAdvancedRules", checkBoxClearAdvancedRules.Checked.ToString());
            settings.saveSetting("AskBeforeDeletingoriginalFile", checkBoxAskBeforeDeletingoriginalFile.Checked.ToString());
        }

        public void loadSettings()
        {
            numericUpDownSlideShow.Value = Convert.ToInt16(settings.loadSetting("SlideShowTime", "5"));
            checkBoxSlideShowRandom.Checked = Convert.ToBoolean(settings.loadSetting("SlideShowRandomMode", "false"));

            //splitContainer1.Panel2Collapsed = !Convert.ToBoolean(settings.loadSetting("ShowPictureInformation", "true"));
            //showPictureInformationToolStripMenuItem.Checked = Convert.ToBoolean(settings.loadSetting("ShowPictureInformation", "true"));

            showToolBarToolStripMenuItem.Checked = Convert.ToBoolean(settings.loadSetting("ShowToolBar", "true"));
            toolStrip1.Visible = Convert.ToBoolean(settings.loadSetting("ShowToolBar", "true"));

            splitContainer2.Panel1Collapsed = !Convert.ToBoolean(settings.loadSetting("ShowSideMenu", "true"));
            showSideMenuToolStripMenuItem.Checked = Convert.ToBoolean(settings.loadSetting("ShowSideMenu", "true"));

            String[] destinationsArray = settings.loadSetting("Destinations", "").Split(SEPERATOR);
            foreach (String value in destinationsArray)
            {
                if (!value.Equals(""))
                {
                    addDestination(value);
                }
            }

            textBoxAdvancedSortingBaseFolder.Text = settings.loadSetting("AdvancedSortBaseFolder", "");
            checkBoxClearAdvancedRules.Checked = Convert.ToBoolean(settings.loadSetting("ClearAdvancedRules", "false"));
            checkBoxAskBeforeDeletingoriginalFile.Checked = Convert.ToBoolean(settings.loadSetting("AskBeforeDeletingoriginalFile", "true"));
        }

        public void exit()
        {
            DialogResult answer = MessageBox.Show("Would you like to save your settings before you exit?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                saveSettings();
                Application.Exit();
            }
            if (answer == DialogResult.No)
            {
                Application.Exit();
            }
        }

        public void setZoomLevel()
        {
            if (scaleToFitToolStripMenuItem.Checked)
            {
                zoomToFit();
            }

            if (originalSizeToolStripMenuItem.Checked)
            {
                zoomOriginalSize();
            }

            if (doubleSizeToolStripMenuItem.Checked)
            {
                zoomDoubleSize();
            }
        }

        public void zoomToFit()
        {
            scaleToFitToolStripMenuItem.Checked = true;
            originalSizeToolStripMenuItem.Checked = false;
            doubleSizeToolStripMenuItem.Checked = false;

            pictureBox1.Cursor = Cursors.Default;

            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        public void zoomOriginalSize()
        {
            scaleToFitToolStripMenuItem.Checked = false;
            originalSizeToolStripMenuItem.Checked = true;
            doubleSizeToolStripMenuItem.Checked = false;

            pictureBox1.Cursor = Cursors.SizeAll;

            pictureBox1.Dock = DockStyle.None;
            pictureBox1.Width = Image.FromFile(fi[currentImage].FullName).Width;
            pictureBox1.Height = Image.FromFile(fi[currentImage].FullName).Height;
        }

        public void zoomDoubleSize()
        {
            scaleToFitToolStripMenuItem.Checked = false;
            originalSizeToolStripMenuItem.Checked = false;
            doubleSizeToolStripMenuItem.Checked = true;

            pictureBox1.Cursor = Cursors.SizeAll;

            pictureBox1.Dock = DockStyle.None;
            pictureBox1.Width = Image.FromFile(fi[currentImage].FullName).Width * 2;
            pictureBox1.Height = Image.FromFile(fi[currentImage].FullName).Height * 2;
        }

        // Event Handlers
        
        private void Form1_Load(object sender, EventArgs e)
        {
            selectFolder();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectFolder();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void buttonNextImage_Click(object sender, EventArgs e)
        {
            displayNextImage();
        }

        private void buttonPreviousImage_Click(object sender, EventArgs e)
        {
            displayPreviousImage();
        }

        private void slideShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSlideShow();
            numericUpDownSlideShow.Enabled = !numericUpDownSlideShow.Enabled;
            buttonSlideShowStart.Enabled = !buttonSlideShowStart.Enabled;
            buttonSlideShowStop.Enabled = !buttonSlideShowStop.Enabled;
        }

        private void timerSlideShow_Tick(object sender, EventArgs e)
        {
            if (checkBoxSlideShowRandom.Checked == true)
            {
                Random random = new Random();
                displayImageAtPosition(random.Next(0, fi.Length));
            }
            else
            {
                displayNextImage();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutBox a = new frmAboutBox();
            a.ShowDialog();
        }

        private void showSideMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer2.Panel1Collapsed = !splitContainer2.Panel1Collapsed;
            showSideMenuToolStripMenuItem.Checked = !showSideMenuToolStripMenuItem.Checked;
        }

        private void nextImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayNextImage();
        }

        private void previousImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayPreviousImage();
        }

        private void buttonSlideShowStart_Click(object sender, EventArgs e)
        {
            toggleSlideShow();
            numericUpDownSlideShow.Enabled = false;
            buttonSlideShowStart.Enabled = false;
            buttonSlideShowStop.Enabled = true;
        }

        private void buttonSlideShowStop_Click(object sender, EventArgs e)
        {
            toggleSlideShow();
            numericUpDownSlideShow.Enabled = true;
            buttonSlideShowStart.Enabled = true;
            buttonSlideShowStop.Enabled = false;
        }

        private void buttonDeleteFile_Click(object sender, EventArgs e)
        {
            deleteCurrentFile(true);
        }

        private void deleteCurrentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteCurrentFile(true);
        }

        private void buttonAddDestinations_Click(object sender, EventArgs e)
        {
            addDestination();
        }

        private void buttonRemoveDestination_Click(object sender, EventArgs e)
        {
            removeDestination();
        }

        private void buttonMoveFile_Click(object sender, EventArgs e)
        {
            moveCurrentFile();
        }

        private void toolStripMenuItemSaveSettings_Click(object sender, EventArgs e)
        {
            saveSettings();
            MessageBox.Show("Settings saved", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void listBoxAllFiles_SelectedValueChanged(object sender, EventArgs e)
        {
            displayImageAtPosition(listBoxAllFiles.SelectedIndex);
            this.Text = Application.ProductName + " (Version " + Application.ProductVersion + ") - " + fi[listBoxAllFiles.SelectedIndex].Name;
        }

        private void fullScreenModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showSideMenuToolStripMenuItem_Click(sender, e);
            //showPictureInformationToolStripMenuItem_Click(sender, e);
            //menuStrip1.Visible = !menuStrip1.Visible;
            //statusStrip1.Visible = !statusStrip1.Visible;
          
            if (this.FormBorderStyle == FormBorderStyle.Sizable)
            {
                MessageBox.Show("Press Ctrl+F to exit Full Screen Mode", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                menuStrip1.Visible = false;
                statusStrip1.Visible = false;
                toolStrip1.Visible = false;
                splitContainer2.Panel1Collapsed = true;
                showSideMenuToolStripMenuItem.Checked = false;
                showToolBarToolStripMenuItem.Checked = false;
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                menuStrip1.Visible = true;
                statusStrip1.Visible = true;
                toolStrip1.Visible = true;
                splitContainer2.Panel1Collapsed = false;
                showSideMenuToolStripMenuItem.Checked = true;
                showToolBarToolStripMenuItem.Checked = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void moveCurrentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPageMove;
            moveCurrentFile();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                exit();
            }
        }

        private void buttonChangeAdvancedSortingBaseFolder_Click(object sender, EventArgs e)
        {
                folderBrowserDialog1.ShowNewFolderButton = true;
                folderBrowserDialog1.Description = Application.ProductName + Environment.NewLine + "Select a base folder:";

                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBoxAdvancedSortingBaseFolder.Text = folderBrowserDialog1.SelectedPath;
                }
        }

        private void buttonAdvancedSort_Click(object sender, EventArgs e)
        {
            if (textBoxAdvancedSortingRules.Text.Length == 0)
            {
                MessageBox.Show("Pleae enter some sorting rules", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                advancedSort(textBoxAdvancedSortingRules.Text, textBoxAdvancedSortingBaseFolder.Text);
                if (checkBoxClearAdvancedRules.Checked == true)
                {
                    textBoxAdvancedSortingRules.Text = "";
                }
            }
            tabControl1.SelectedTab = tabPageAdvancedMove;
            textBoxAdvancedSortingRules.Focus();
        }

        private void checkedListBoxDestinations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBoxDestinations.SelectedItems.Count == 0)
            {
                buttonRemoveDestination.Enabled = false;
            }
            else
            {
                buttonRemoveDestination.Enabled = true;
            }
        }

        private void textBoxAdvancedSortingRules_KeyDown(object sender, KeyEventArgs e)
        {
            // Disables the ENTER key (maybe this should trigger the sort?)
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                //buttonAdvancedSort_Click(null, null);
            }
        }

        private void textBoxAdvancedSortingRules_KeyPress(object sender, KeyPressEventArgs e)
        {
           // Stops the user typing in any chars that can't be used in a filename
           if(e.KeyChar == Convert.ToInt32('\\') || e.KeyChar == Convert.ToInt32('/') || e.KeyChar == Convert.ToInt32(':') || e.KeyChar == Convert.ToInt32('*') || e.KeyChar == Convert.ToInt32('?') || e.KeyChar == Convert.ToInt32('"') || e.KeyChar == Convert.ToInt32('<') || e.KeyChar == Convert.ToInt32('>') || e.KeyChar == Convert.ToInt32('|'))
           {
               e.Handled = true;
           }
        }

        private void toolStripButtonNextImage_Click(object sender, EventArgs e)
        {
            displayNextImage();
        }

        private void toolStripButtonPreviousImage_Click(object sender, EventArgs e)
        {
            displayPreviousImage();
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            deleteCurrentFile(true);
        }

        private void showToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = !toolStrip1.Visible;
            showToolBarToolStripMenuItem.Checked = !showToolBarToolStripMenuItem.Checked;
        }

        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mwynwood/allSorts");
        }

        private void toolStripButtonEditImage_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mspaint", "\"" + fi[currentImage].FullName + "\"");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonFullScreen_Click(object sender, EventArgs e)
        {
            fullScreenModeToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonSelectAnotherFolder_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void scaleToFitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoomToFit();
        }

        private void originalSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoomOriginalSize();
        }

        private void doubleSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoomDoubleSize();
        }

        private void toolStripButtonShowImageLocation_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(fi[currentImage].DirectoryName);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAdvancedSortHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Write instructions for Advanced Sorting Rules");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            clickPosn.X = e.X;
            clickPosn.Y = e.Y;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                scrollPosn.X = scrollPosn.X + clickPosn.X - e.X;
                scrollPosn.Y = scrollPosn.Y + clickPosn.Y - e.Y;
                panelPictureBoxHolder.AutoScrollPosition = scrollPosn;
            }
        }

        //END TEST
    }
}
