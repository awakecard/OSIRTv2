﻿using ImageMagick;
using Jacksonsoft;
using Microsoft.Win32;
using mshtml;
using OSIRT.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OSIRT.Browser
{
    public class ExtendedBrowser : WebBrowser
    {

        private string bitmap;
        private int MaxScrollHeight { get { return 20000; } }

        public ExtendedBrowser()
        {
            SetLatestIEKeyforWebBrowserControl();
            NativeMethods.DisableClickSounds();
            ScriptErrorsSuppressed = true;
        }




        /// <summary>
        /// Gets the current viewport of the browser
        /// </summary>
        /// <returns>A Bitmap of the current browser viewport</returns>
        public Bitmap GetCurrentViewScreenshot()
        {
            int width, height;
            width = ClientRectangle.Width;
            height = ClientRectangle.Height;
            using (Bitmap image = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {

                    Point p, upperLeftDestination;
                    Point upperLeftSource = new Point(0, 0);
                    p = new Point(0, 0);
                    upperLeftSource = PointToScreen(p);
                    upperLeftDestination = new Point(0, 0);
                    Size blockRegionSize = ClientRectangle.Size;
                    graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, blockRegionSize);

                }
                return new Bitmap(image);
            }


        }

        public async Task PutTaskDelay()
        {
            await Task.Delay(300);
        }

        private async void FullpageScreenshotByScrolling()
        {
            ((Control)this).Enabled = false;
            int viewportHeight = ClientRectangle.Size.Height;
            int viewportWidth = ClientRectangle.Size.Width;
            int scrollHeight = ScrollHeight();
            int scrollWidth = ScrollWidth();
            ToggleScrollbars(false);


            int count = 0;
            int pageLeft = scrollHeight;
            bool atBottom = false;
            while (!atBottom)
            {
                if (pageLeft > viewportHeight)
                {
                    //if we can scroll using the viewport, let's do that
                   
                    this.ScrollTo(0, count * viewportHeight);
                 
                    count++;

                    await PutTaskDelay();


                    using (Bitmap snap = GetCurrentViewScreenshot())
                    {
                        snap.Save(@"D:/comb/" + count + ".png", ImageFormat.Png);
                    }
                }
                else //TODO: what if it's exactly divisible?
                {
                    //find out what's left of the page to scroll, then take screenshot
                    //if it's the last image, we're going to need to crop what we need, as it'll take
                    //a capture of the entire viewport.

                    Navigate("javascript:var s = function() { window.scrollBy(0," + pageLeft.ToString() + "); }; s();");

                    atBottom = true;
                    count++;

                    await PutTaskDelay();
                    //Thread.Sleep(500);

                    Rectangle cropRect = new Rectangle(new Point(0, viewportHeight - pageLeft), new Size(viewportWidth, pageLeft));

                    using (Bitmap src = GetCurrentViewScreenshot())
                    using (Bitmap target = new Bitmap(cropRect.Width, cropRect.Height))
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
                        target.Save(@"D:/comb/" + count + ".png", ImageFormat.Png);
                    }

                }

                pageLeft = pageLeft - viewportHeight;
                ToggleFixedElements(false);


            }//end while

            ToggleScrollbars(true);
            ToggleFixedElements(true);
            //scroll page back to the top
            Document.Body.ScrollIntoView(true);

            ((Control)this).Enabled = true;

           
            WaitWindow.Show(GetScreenshot, Resources.strings.CombineScreenshots);
         }

        private void GetScreenshot(object sender, WaitWindowEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo("D:\\comb");
            FileSystemInfo[] files = directory.GetFileSystemInfos();
            bitmap = ScreenshotHelper.CombineScreenshot(files, e);
           
        }


        private void FullpageScreenshotGDI()
        {
            int width = ScrollWidth();
            int height = ScrollHeight();

            Debug.WriteLine("FULLPAGE: Width: {0}. Height: {1}", width, height);
            this.Dock = DockStyle.None;
            this.ToggleScrollbars(false);
            this.Size = new Size(width, height);
            Bitmap screenshot = new Bitmap(width, height);
            NativeMethods.GetImage(this.ActiveXInstance, screenshot, System.Drawing.Color.Black);
            this.Dock = DockStyle.Fill;
            this.ToggleScrollbars(true);

            using (screenshot)
            {
                screenshot.Save(@"D:/GDI_SAVE.png");
            }


            bitmap = @"D:/GDI_SAVE.png";
        }

        public string GetFullpageScreenshot()
        {

            if (ScrollHeight() > MaxScrollHeight)
            {
                FullpageScreenshotByScrolling();
            }
            else
            {
                FullpageScreenshotGDI();
            }
            return bitmap;

        }

        private void ScrollTo(int x, int y)
        {
            Document.Window.ScrollTo(x, y);
        }


        /// <summary>
        /// Inspects the registry and uses the latest version of IE
        /// </summary>
        private void SetLatestIEKeyforWebBrowserControl()
        {

            const string BROWSER_EMULATION_KEY = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

            String appname = Process.GetCurrentProcess().ProcessName + ".exe";

            // Internet Explorer 11. Webpages are displayed in IE11 edge mode, regardless of the !DOCTYPE directive.
            const int browserEmulationMode = 11001;

            RegistryKey browserEmulationKey =
            Registry.CurrentUser.OpenSubKey(BROWSER_EMULATION_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree) ??
            Registry.CurrentUser.CreateSubKey(BROWSER_EMULATION_KEY);

            if (browserEmulationKey != null)
            {
                browserEmulationKey.SetValue(appname, browserEmulationMode, RegistryValueKind.DWord);
                browserEmulationKey.Close();
            }

        }

        /// <summary>
        /// Toggles the all the elements that has a property of "position:fixed" in the document
        /// </summary>
        /// <param name="toggle">Elements are visible if true, other hidden if false</param>
        public void ToggleFixedElements(bool toggle)
        {
            string property = toggle ? "visible" : "hidden";
            HtmlElement h = this.Document.GetElementsByTagName("head")[0];
            HtmlElement s = this.Document.CreateElement("script");
            IHTMLScriptElement el = (IHTMLScriptElement)s.DomElement;
            el.text = "javascript: var f = function() { var elements =	document.querySelectorAll('*'); for (var i = 0; i < elements.length; i++) { var position = window.getComputedStyle(elements[i]).position; if(position === 'fixed') { elements[i].style.visibility = '" + property + "';  } }	 }; f();";
            h.AppendChild(s);
        }

        /// <summary>
        /// Obtains the Height of the current document.
        /// Inspects the documents scroll height, client height, the body's scroll height and the bounds of the body using ScrollRectangle.
        /// The highest height is returned.
        /// </summary>
        /// <returns>The document's current Height</returns>
        public int ScrollHeight()
        {
            int scrollHeight = 0;

            Rectangle bounds = this.Document.Body.ScrollRectangle;
            IHTMLElement2 body = this.Document.Body.DomElement as IHTMLElement2;
            IHTMLElement2 doc = (this.Document.DomDocument as IHTMLDocument3).documentElement as IHTMLElement2;

            scrollHeight = new[] { body.scrollHeight, bounds.Height, doc.scrollHeight, this.Document.Body.OffsetRectangle.Height, doc.clientHeight }.Max();

            return scrollHeight;
        }

        /// <summary>
        /// Obtains the Width of the current document.
        /// Inspects the documents scroll width, client width, the body's scroll width and the bounds of the body using ScrollRectangle.
        /// The highest width is returned.
        /// </summary>
        /// <returns>The document's current Width</returns>
        public int ScrollWidth()
        {
            int scrollWidth = 0;

            Rectangle bounds = this.Document.Body.ScrollRectangle;
            IHTMLElement2 body = this.Document.Body.DomElement as IHTMLElement2;
            IHTMLElement2 doc = (this.Document.DomDocument as IHTMLDocument3).documentElement as IHTMLElement2;

            scrollWidth = new[] { body.scrollWidth, bounds.Width, doc.scrollWidth, this.Document.Body.OffsetRectangle.Width, doc.clientWidth }.Max();

            return scrollWidth;
        }

        /// <summary>
        /// Displays or hides the document's scrollbars
        /// </summary>
        /// <param name="toggle">Scrolls visible if true, hidden if false</param>
        public void ToggleScrollbars(bool toggle)
        {
            string property = toggle ? "visible" : "hidden";
            string attribute = toggle ? "yes" : "no";
            this.Document.Body.Style = String.Format("overflow:{0}", property);
            this.Document.Body.SetAttribute("scroll", attribute);
        }


    }
}