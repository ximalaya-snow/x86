﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;

namespace CaptureWindows
{
    public partial class Form1 : Form
    {
        Process WxProcess = null;

        //HDC GetWindowDC(
        //    HWND hWnd
        //);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);


        //int ReleaseDC(
        //    HWND hWnd,
        //    HDC hDC
        //);
        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);



        //BOOL GetWindowRect(
        //    HWND hWnd,
        //    LPRECT lpRect
        //);
        [DllImport("User32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);



        //HBITMAP CreateCompatibleBitmap(
        //    HDC hdc,
        //    int cx,
        //    int cy
        //);
        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);


        //HDC CreateCompatibleDC(
        //    HDC hdc
        //);
        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);


        //HGDIOBJ SelectObject(
        //    HDC hdc,
        //    HGDIOBJ h
        //);
        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);


        //BOOL PrintWindow(
        //    HWND hwnd,
        //    HDC hdcBlt,
        //    UINT nFlags
        //);
        //);
        [DllImport("User32.dll")]
        public static extern int PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);



        //BOOL DeleteObject(
        //    HGDIOBJ ho
        //);
        [DllImport("Gdi32.dll")]
        public static extern int DeleteObject(IntPtr ho);


        //BOOL DeleteDC(
        //    HDC hdc
        //);
        [DllImport("Gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);


        public Form1()
        {
            InitializeComponent();
        }

        Boolean Init()
        {

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (p.ProcessName == "WeChat")
                {
                    //WxProcess = new Process();
                    WxProcess = p;
                    break;

                }
            }


            if (WxProcess == null)
            {
                MessageBox.Show("Cannot open process.");
                return false;
            }

            return true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Init() == false) return;

            IntPtr windowsDCHandle = GetWindowDC(IntPtr.Zero);
            if (windowsDCHandle == IntPtr.Zero)
            {
                MessageBox.Show("Cannot open GetWindowDC.");
                return;
            }


            Rectangle rectangle = new Rectangle();
            if (GetWindowRect(WxProcess.MainWindowHandle, ref rectangle) == 0)
            {
                MessageBox.Show("Cannot open GetWindowRect.");
                return;
            }
            //int width = rectangle.Width - rectangle.X;
            //int height = rectangle.Height - rectangle.Y;

            int width = rectangle.Width;
            int height = rectangle.Height;

            IntPtr createCompatibleBitmap = CreateCompatibleBitmap(windowsDCHandle, width, height);

            if (createCompatibleBitmap == IntPtr.Zero)
            {
                MessageBox.Show("Cannot open CreateCompatibleBitmap.");
                return;
            }


            IntPtr createCompatibleDC = CreateCompatibleDC(windowsDCHandle);


            if (createCompatibleDC == IntPtr.Zero)
            {
                MessageBox.Show("Cannot open CreateCompatibleDC.");
                return;
            }



            if (SelectObject(createCompatibleDC, createCompatibleBitmap) == IntPtr.Zero)
            {
                MessageBox.Show("Cannot open SelectObject.");
                return;
            }



            if (PrintWindow(WxProcess.MainWindowHandle, createCompatibleDC, 0) == 0)
            {
                MessageBox.Show("Cannot open PrintWindow.");
                return;
            }

            this.pictureBox1.Width = width;
            this.pictureBox1.Height = height;
            this.pictureBox1.Image = Image.FromHbitmap(createCompatibleBitmap);

            this.pictureBox1.Image.Save(Environment.CurrentDirectory + "\\wx.png", ImageFormat.Png);

            int qrWidth = 280;
            int qrHeight = 280;

            Image qrImage = new Bitmap(qrWidth, qrHeight);
            Graphics graphics = Graphics.FromImage(qrImage);
            graphics.DrawImage(Image.FromHbitmap(createCompatibleBitmap),
                new Rectangle(0, 0, qrWidth, qrHeight),
                new Rectangle(40, 80, qrWidth, qrHeight), GraphicsUnit.Pixel);
            graphics.Dispose();

            this.pictureBox2.Width = qrWidth;
            this.pictureBox2.Height = qrHeight;
            this.pictureBox2.Image = qrImage;


            DeleteObject(createCompatibleBitmap);
            DeleteDC(createCompatibleBitmap);
            ReleaseDC(WxProcess.MainWindowHandle, windowsDCHandle);


            // create a barcode reader instance
            IBarcodeReader reader = new BarcodeReader();
            // load a bitmap
            var barcodeBitmap = (Bitmap)this.pictureBox1.Image;
            // detect and decode the barcode inside the bitmap
            var result = reader.Decode(barcodeBitmap);
            // do something with the result
            if (result != null)
            {

                this.textBox1.Text = result.Text;
            }

            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            options.Width = 280;
            options.Height = 280;
            options.Margin = 2;
            barcodeWriter.Options = options;
            Bitmap bitmap = barcodeWriter.Write(this.textBox1.Text);
            this.pictureBox3.Width = 280;
            this.pictureBox3.Height = 280;
            this.pictureBox3.Image = bitmap;




        }





    }
}
