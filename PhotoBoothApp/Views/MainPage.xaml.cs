using Microsoft.Graphics.Canvas;
using PhotoBoothApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PhotoBoothApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int undosAvailable = 6;
        int maxUndo = 6;

        public List<Color> AvailableColors { get; set; } = new List<Color>();

        public MainPage()
        {
            this.InitializeComponent();

            PaintCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;


            //TODO: Place/generate this in a separate static file or class
            AvailableColors.Add(Color.FromArgb(255, 0, 0, 0));
            AvailableColors.Add(Color.FromArgb(255, 128, 128, 128));
            AvailableColors.Add(Color.FromArgb(255, 192, 192, 192));
            AvailableColors.Add(Color.FromArgb(255, 255, 255, 255));
            AvailableColors.Add(Color.FromArgb(255, 255, 0, 0));
            AvailableColors.Add(Color.FromArgb(255, 255, 255, 0));
            AvailableColors.Add(Color.FromArgb(255, 128, 255, 0));
            AvailableColors.Add(Color.FromArgb(255, 0, 255, 255));
            AvailableColors.Add(Color.FromArgb(255, 0, 128, 192));
            AvailableColors.Add(Color.FromArgb(255, 128, 128, 192));
            AvailableColors.Add(Color.FromArgb(255, 255, 0, 255));
            AvailableColors.Add(Color.FromArgb(255, 128, 0, 255));
            AvailableColors.Add(Color.FromArgb(255, 0, 128, 64));
            AvailableColors.Add(Color.FromArgb(255, 128, 0, 0));
            AvailableColors.Add(Color.FromArgb(255, 0, 0, 255));
            AvailableColors.Add(Color.FromArgb(255, 0, 128, 255));

            //Dynamically generate rectangles and add them to drop down
            foreach (var c in AvailableColors)
            {
                Rectangle rec = new Rectangle();

                rec.Height = 20;
                rec.Name = c.ToString();
                rec.Fill = new SolidColorBrush(c);
                colorBox.Items.Add(rec);
            }

            PaintCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void ColorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InkDrawingAttributes ida = new InkDrawingAttributes();

            Rectangle b = (Rectangle)e.AddedItems[0];
            
            var color = (Color)XamlBindingHelper.ConvertValue(typeof(Color), b.Name);
            ida.Color = color;

            colorRectangle.Fill = new SolidColorBrush(color);
            PaintCanvas.InkPresenter.StrokeInput.InkPresenter.UpdateDefaultDrawingAttributes(ida);

        }


        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            //Add back to undoAvailable but only if its less than MaxUndos
            if (undosAvailable < maxUndo)
            {
                undosAvailable += 1;
            }
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            //Set captured image to null
            capturedImg.Source = null;

            //Clear the canvas
            PaintCanvas.InkPresenter.StrokeContainer.Clear();

        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            //Get all strokes
            var strokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            //Check if there are any strokes
            if (strokes.Count > 0)
            {
                //Check if we have undos available
                if (undosAvailable > 0)
                {
                    //Get the last stroke
                    strokes[strokes.Count - 1].Selected = true;

                    //Remove last stroke
                    PaintCanvas.InkPresenter.StrokeContainer.DeleteSelected();

                    //subtract available undos by 1
                    undosAvailable--;
                }
            }
        }

        private async void BtnOpen_ClickAsync(object sender, RoutedEventArgs e)
        {
            //Open file dialog
            var openPicker = new FileOpenPicker();

            //Only .jpg files
            openPicker.FileTypeFilter.Add(".jpg");

            var file = await openPicker.PickSingleFileAsync();

            //File is selected
            if (file != null)
            {
                //Remove current captured image
                capturedImg.Source = null;

                var stream = await file.OpenAsync(FileAccessMode.Read);

                // Read from file.
                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    //Set canvas strokes from stream
                    await PaintCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                }
                stream.Dispose();
            }
        }

        private async void BtnSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            var currentStrokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Strokes present on ink canvas.
            if (currentStrokes.Count > 0 || capturedImg.Source != null)
            {
                FileSavePicker fsp = new FileSavePicker();

                fsp.FileTypeChoices.Add("JPEG", new List<string>() { ".jpg" });
                fsp.FileTypeChoices.Add("PBD", new List<string>() { ".pbd" });      //custom file type?
                fsp.DefaultFileExtension = ".jpg";

                StorageFile file = await fsp.PickSaveFileAsync();


                if (file != null)
                {
                    //Prevent updates to the file until updates are 
                    CachedFileManager.DeferUpdates(file);

                    //Open file stream for writing.
                    var stream = await file.OpenAsync(FileAccessMode.ReadWrite);

                    if (capturedImg.Source != null)
                    {
                        DisplayInformation display = DisplayInformation.GetForCurrentView();
                        var renderTargetBitmap = new RenderTargetBitmap();
                        await renderTargetBitmap.RenderAsync(capturedImg, (int)capturedImg.Width, (int)capturedImg.Height);

                        IBuffer pixels = await renderTargetBitmap.GetPixelsAsync();
                        byte[] bytes = pixels.ToArray();

                        var canvasBytes = GetSignatureBitmapFullAsync();

                        // Create an encoder with the desired format
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                        

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)capturedImg.ActualWidth, (uint)capturedImg.ActualHeight, display.LogicalDpi, display.LogicalDpi, bytes);


                        
                        await encoder.FlushAsync();
                    }

                    //Write the ink strokes to the output stream.
                    //using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
                    //{
                    //    await PaintCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);

                    //    await outputStream.FlushAsync();
                    //}

                    stream.Dispose();

                    // Finalize write so other apps can update file.
                    Windows.Storage.Provider.FileUpdateStatus status =
                        await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        // File saved.
                    }
                    else
                    {
                        // File couldn't be saved.
                    }
                }
            }


            //BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);


            //SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            //SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        private byte[] ConvertInkCanvasToByteArray()
        {
            var canvasStrokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            if (canvasStrokes.Count > 0)
            {
                var width = (int)PaintCanvas.ActualWidth;
                var height = (int)PaintCanvas.ActualHeight;
                var device = CanvasDevice.GetSharedDevice();
                var renderTarget = new CanvasRenderTarget(device, width,
                    height, 96);

                using (var ds = renderTarget.CreateDrawingSession())
                {
                    ds.Clear(Colors.White);
                    ds.DrawInk(PaintCanvas.InkPresenter.StrokeContainer.GetStrokes());
                }

                return renderTarget.GetPixelBytes();
            }
            else
            {
                return null;
            }
        }

        private async Task<WriteableBitmap> GetSignatureBitmapFullAsync()
        {
            var bytes = ConvertInkCanvasToByteArray();

            if (bytes != null)
            {
                var width = (int)PaintCanvas.ActualWidth;
                var height = (int)PaintCanvas.ActualHeight;

                var bmp = new WriteableBitmap(width, height);
                using (var stream = bmp.PixelBuffer.AsStream())
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    return bmp;
                }
            }
            else
                return null;
        }

        private async void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(250, 250);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                //User cancelled photo capture
                return;
            }

            StorageFolder destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TempCaptureFolder", CreationCollisionOption.OpenIfExists);

            await photo.CopyAsync(destinationFolder, "CapturePhoto.jpg", NameCollisionOption.ReplaceExisting);
            

            //read stream
            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();


            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);


            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            //set image
            capturedImg.Source = bitmapSource;

            //delete photo afterwards
            await photo.DeleteAsync();
        }

    }
}
