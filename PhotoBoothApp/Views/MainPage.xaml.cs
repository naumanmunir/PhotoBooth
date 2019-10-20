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
        int undoCounter = 3;

        int strokeCounter = 0;

        public Stack<InkStroke> UndoStrokes { get; set; } = new Stack<InkStroke>();
        public Stack<InkStroke> CollectedStrokes { get; set; } = new Stack<InkStroke>();

        public List<Color> AvailableColors { get; set; } = new List<Color>();

        public MainPage()
        {
            this.InitializeComponent();

            PaintCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;


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
            var strokes = sender.StrokeContainer.GetStrokes();

            if (strokes.Last() != null)
            {
                if (CollectedStrokes.Count <= 3)
                {
                    CollectedStrokes.Push(strokes.Last());
                }
                else
                {

                    //CollectedStrokes.Push(strokes.Last());
                }
            }
            
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            capturedImg.Source = null;
            PaintCanvas.InkPresenter.StrokeContainer.Clear();

        }

        int currIndex = 0;

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            var strokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {

                if (undoCounter > 0)
                {
                    strokes[strokes.Count - 1].Selected = true;

                    PaintCanvas.InkPresenter.StrokeContainer.DeleteSelected();

                    undoCounter--;
                }


            }

            //if (CollectedStrokes.Count > 0)
            //{
            //    var stroke = CollectedStrokes.Pop();

            //    stroke.Selected = true;

            //    PaintCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            //}

        }

        private async void BtnOpen_ClickAsync(object sender, RoutedEventArgs e)
        {

            var openPicker = new FileOpenPicker();

            openPicker.FileTypeFilter.Add(".jpg");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                capturedImg.Source = null;

                var stream = await file.OpenAsync(FileAccessMode.Read);

                // Read from file.
                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    await PaintCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                }
                stream.Dispose();
            }
        }

        private async void BtnSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            var currentStrokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Strokes present on ink canvas.
            if (currentStrokes.Count > 0)
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

                    //Write the ink strokes to the output stream.
                    using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
                    {
                        await PaintCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                        await outputStream.FlushAsync();
                    }
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

            StorageFolder destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePhotoFolder", CreationCollisionOption.OpenIfExists);

            await photo.CopyAsync(destinationFolder, "ProfilePhoto.jpg", NameCollisionOption.ReplaceExisting);
            

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
