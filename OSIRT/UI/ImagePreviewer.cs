﻿using Cyotek.Windows.Forms;
using ImageMagick;
using Jacksonsoft;
using OSIRT.Enums;
using OSIRT.Helpers;
using OSIRT.Loggers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OSIRT.UI
{
    public partial class ImagePreviewer : Previewer
    {
        private ImageBox imageBox;
        private readonly int MaxImageHeight = 12500;
        private CannotOpenImagePanel cantOpenPanel;
        private string Url;

        public ImagePreviewer(Actions a, string url) : this(a, url, Constants.TempImgFile) { }

        public ImagePreviewer(Actions a, string url, string filePath) : base(a)
        {
            InitializeComponent();

            uiURLTextBox.Text = url;
            FormClosing += ImagePrevEx_FormClosing;
            Url = url;
            this.filePath = filePath;

        }

        private void ImagePrevEx_Load(object sender, EventArgs e)
        {
            //filePath = Constants.TempImgFile;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            Size imageSize = GetImageSize();

            if (imageSize.Height < MaxImageHeight)
            {
                CreateAndShowImageBox();
            }
            else
            {
                ShowCannotOpenPanel(imageSize);
            }
        }

        private void ImagePrevEx_FormClosing(object sender, FormClosingEventArgs e)
        {
            imageBox?.Image?.Dispose(); //the image box can be not null, but the image CAN be null!
            imageBox?.Dispose();
            cantOpenPanel?.CleanUp();
        }


        private void SaveAsPdf(object sender, WaitWindowEventArgs e)
        {
            string fileName = e.Arguments[0].ToString();
            string pathToSave = "";
            bool thrown = false;
            try
            {
                using (MagickImage image = new MagickImage(filePath))
                {
                    image.Format = MagickFormat.Pdf;
                    pathToSave = Path.Combine(Constants.ContainerLocation, Constants.Directories.GetSpecifiedCaseDirectory(action), fileName + SaveableFileTypes.Pdf);
                    image.Write(pathToSave);
                    e.Window.Message = "Rehashing PDF";
                    Hash = OsirtHelper.GetFileHash(pathToSave);
                }
            }
            catch (Exception ex) when (ex is MagickErrorException || ex is System.Runtime.InteropServices.SEHException || ex is ArgumentException || ex is System.Reflection.TargetInvocationException)
            {
                thrown = true;
                var message = "Unable to save as PDF. Reverting to saving as PNG.";
                Invoke((MethodInvoker)(() => uiFileExtensionComboBox.SelectedIndex = uiFileExtensionComboBox.Items.IndexOf(SaveableFileTypes.Png)));
                e.Window.Message = message;
                Task.Delay(2000).Wait(); //just so the user can see we're saving as PNG instead
                SaveAsPng(fileName);
            }
            finally
            {
                //delete temp pdf file
                if (thrown)
                {
                    if (File.Exists(pathToSave))
                    {
                        File.Delete(pathToSave);
                    }
                }
            }
        }

        private void SaveAsPng(string name)
        {
            try
            {
                string destLocation = Path.Combine(Constants.ContainerLocation, Constants.Directories.GetSpecifiedCaseDirectory(action), name + SaveableFileTypes.Png);
                string sourceFile = Path.Combine(Constants.CacheLocation, Constants.TempImgFile);
                File.Copy(sourceFile, destLocation); //use Copy for now, then delete cache later
            }
            catch (IOException ioe)
            {
                MessageBox.Show($"Can't move file to container due to an IOExcpetion... {ioe}");
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show($"Can't move file to container due to an Unauthorised Access Attempt... {uae}");
            }
        }

  


        private Size GetImageSize()
        {
            int width = 0, height = 0;
            try
            {
                using (MagickImage image = new MagickImage(filePath))
                {
                    width = image.Width;
                    height = image.Height;
                }
            }
            catch(MagickCoderErrorException cex)
            {
                System.Diagnostics.Debug.WriteLine(cex.ToString());
            }
            catch (MagickCorruptImageErrorException mic)
            {
                MessageBox.Show("CORRUPT: " + mic);
            }
            catch(Exception e)
            {
                MessageBox.Show("EX: " + e.ToString());
            }

            return new Size(width, height);
        }

        private void CreateAndShowImageBox()
        {
            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            uiPreviewerSplitContainer.Panel2.Controls.Add(imageBox);
            try
            {
                Image image = Image.FromFile(filePath);
                LoadImage(new Bitmap(image));
            }
            catch (OutOfMemoryException oom)
            {
                MessageBox.Show(@"Debugging: Known issue. Webpage takes a large chunk of memory, causing the image loading in the previewer to fail. 
                                The image is still saved/logged/hashed, it just can't be previewed. Further debug info:  " + oom);
                //we can carry on, show the previewer so they can name the image and leave a note.
                //TODO: put a friendly message on the right, and a link to the image currently residing in the cache
                //so they can see it.
            }
            catch (FileNotFoundException fnf)
            {
                MessageBox.Show($"Unable to find the image file! Debug: {fnf}");
                //safely close OSIRT, here???
            }
            catch(Exception e)
            {
                MessageBox.Show($"Unable to load image file! Debug: {e}");
            }
        }

        private void ShowCannotOpenPanel(Size originalSize)
        {
            cantOpenPanel = new CannotOpenImagePanel(filePath, originalSize);
            cantOpenPanel.Dock = DockStyle.Fill;
            uiPreviewerSplitContainer.Panel2.Controls.Add(cantOpenPanel);
        }

        private void LoadImage(Image image)
        {
            imageBox.Image = image;
            imageBox.ZoomToFit();
        }

        private void uiPreviewerSplitContainer_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SaveAs()
        {
            try
            {
                string destLocation = Path.Combine(Constants.ContainerLocation, Constants.Directories.GetSpecifiedCaseDirectory(action), Path.GetFileName(filePath));
                string sourceFile = Path.Combine(filePath);
                File.Copy(sourceFile, destLocation); //Will need to delete this from the cache
            }
            catch (IOException ioe)
            {
                MessageBox.Show($"Can't move file to container due to an IOExcpetion... {ioe}");
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show($"Can't move file to container due to an Unauthorised Access Attempt... {uae}");
            }
            catch(Exception e)
            {
                MessageBox.Show($"General... {e}");
            }
        }

        private void uiOKButton_Click(object sender, EventArgs e)
        {
            if (FileExtension == SaveableFileTypes.Pdf)
            {
                WaitWindow.Show(SaveAsPdf, "Saving as PDF. Please Wait...", FileName);
            }
            else if (FileExtension == SaveableFileTypes.Png)
            {
                SaveAsPng(FileName);
            }
            else
            {
                Debug.WriteLine(filePath);
                //TODO: need to set the file name for when it's logged
                //at the moment it's taking the file name from the combobox
                //but it's saved using the file name from the download using Path.GetFileName(filePath)
                SaveAs();
            }

           

            Logger.Log(new WebpageActionsLog(Url, action, Hash, FileName + FileExtension, Note));
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
