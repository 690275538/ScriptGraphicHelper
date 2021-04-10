using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.ViewModels
{
    class ImgEditorViewModel : ViewModelBase
    {
        private WriteableBitmap drawBitmap;
        public WriteableBitmap DrawBitmap
        {
            get => drawBitmap;
            set => this.RaiseAndSetIfChanged(ref drawBitmap, value);
        }

        private int imgWidth;
        public int ImgWidth
        {
            get => imgWidth;
            set => this.RaiseAndSetIfChanged(ref imgWidth, value);
        }

        private int imgHeight;
        public int ImgHeight
        {
            get => imgHeight;
            set => this.RaiseAndSetIfChanged(ref imgHeight, value);
        }

        private ImgEditorHelper Helper;
        public ImgEditorViewModel(Models.Range range, byte[] data)
        {
            Helper = new ImgEditorHelper(range, data);
            DrawBitmap = Helper.DrawBitmap;
            ImgWidth = (int)DrawBitmap.Size.Width * 3;
            ImgHeight = (int)DrawBitmap.Size.Height * 3;
        }

        public async void CutImg_Click()
        {
            DrawBitmap = Helper.CutImg();
            ImgWidth = (int)DrawBitmap.Size.Width * 3;
            ImgHeight = (int)DrawBitmap.Size.Height * 3;
        }

        public void Reset_Click()
        {
            DrawBitmap = Helper.ResetImg();
        }

    }
}
