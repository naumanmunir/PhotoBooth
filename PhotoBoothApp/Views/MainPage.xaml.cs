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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PhotoBoothApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;

        public Stack<InkStroke> UndoStrokes { get; set; } = new Stack<InkStroke>(6);

        public Queue<InkStroke> strokesQueue { get; set; } = new Queue<InkStroke>(6);

        public MainPage()
        {
            this.InitializeComponent();

            PaintCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;


            //colorPicker.ColorSpectrumComponents = ColorSpectrumComponents.

            //PaintCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            //PaintCanvas.InkPresenter.StrokeInput.StrokeEnded += StrokeInput_StrokeEnded;
            //PaintCanvas.InkPresenter.StrokeInput.StrokeContinued += StrokeInput_StrokeContinued;

            

        }


        private void StrokeInput_StrokeEnded(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            var strokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            if (UndoStrokes.Count < 7)
            {
                //strokesQueue.Enqueue(sender.InkPresenter.StrokeContainer.st);
                var t = strokes.LastOrDefault();
                if (t != null)
                {
                    UndoStrokes.Push(t);
                }
            }
            else
            {
                UndoStrokes.Reverse();
                UndoStrokes.Pop();
                UndoStrokes.Reverse();
            }
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            var strokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();

            if (strokesQueue.Count < 7)
            {
                //strokesQueue.Enqueue(sender.InkPresenter.StrokeContainer.st);
                
            }
            else
            {
                strokesQueue.Dequeue();
            }


        }

        public async void Application_Resuming(object sender, object e)
        {
            await InitializeCameraAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeCameraAsync();
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            WebCamControl.Visibility = Visibility.Collapsed;
            PaintCanvas.InkPresenter.StrokeContainer.Clear();



            
        }

        public async Task InitializeCameraAsync()
        {
            if (_mediaCapture == null)
            {
                // Get the camera devices
                var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                // try to get the back facing device for a phone
                var backFacingDevice = cameraDevices
                    .FirstOrDefault(c => c.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Back);

                // but if that doesn't exist, take the first camera device available
                var preferredDevice = backFacingDevice ?? cameraDevices.FirstOrDefault();

                // Create MediaCapture
                _mediaCapture = new MediaCapture();

                // Initialize MediaCapture and settings
                await _mediaCapture.InitializeAsync(
                    new MediaCaptureInitializationSettings
                    {
                        VideoDeviceId = preferredDevice.Id
                    });

                // Set the preview source for the CaptureElement
                WebCamControl.Source = _mediaCapture;

                // Start viewing through the CaptureElement 
                await _mediaCapture.StartPreviewAsync();
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            var strokes = PaintCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {
                strokes[strokes.Count - 1].Selected = true;

                PaintCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }

        }

        private async void BtnOpen_ClickAsync(object sender, RoutedEventArgs e)
        {

            var openPicker = new FileOpenPicker();

            openPicker.FileTypeFilter.Add(".jpg");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                WebCamControl.Visibility = Visibility.Collapsed;

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
                fsp.FileTypeChoices.Add("SVG", new List<string>() { ".svg" });
                fsp.DefaultFileExtension = ".jpg";

                StorageFile file = await fsp.PickSaveFileAsync();

                if (file != null)
                {
                    
                    // Prevent updates to the file until updates are 
                    // finalized with call to CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // Open a file stream for writing.
                    var stream = await file.OpenAsync(FileAccessMode.ReadWrite);

                    // Write the ink strokes to the output stream.
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
            else
            {
                //operation cancelled
            }
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            WebCamControl.Visibility = Visibility.Visible;

            Application.Current.Resuming += Application_Resuming;
        }

        private void colorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            InkDrawingAttributes ida = new InkDrawingAttributes();

            ida.Color = sender.Color;
            PaintCanvas.InkPresenter.StrokeInput.InkPresenter.UpdateDefaultDrawingAttributes(ida);
        }

        private async void BtnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            //WebCamControl.pa
            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            //if (photo == null)
            //{
            //    // User cancelled photo capture
            //    return;
            //}

            //StorageFolder destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePhotoFolder", CreationCollisionOption.OpenIfExists);

            //await photo.CopyAsync(destinationFolder, "ProfilePhoto.jpg", NameCollisionOption.ReplaceExisting);
            //await photo.DeleteAsync();

            ////read stream
            //IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            //BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            //SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();



            //SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8,BitmapAlphaMode.Premultiplied);

            //SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            //await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            //capturedImg.Source = bitmapSource;
        }
    }
}
